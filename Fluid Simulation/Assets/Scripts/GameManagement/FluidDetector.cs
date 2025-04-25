using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class FluidDetector : FluidSensor
{
    [Header("Detection Settings")]
    [Tooltip("The density threshold above which fluid is considered present")]
    public float densityThreshold = 0.5f;

    [Tooltip("How often to check for fluid presence (in seconds; overwritten by any Sensor Managers)")]
    public float checkInterval = 0.1f;

    [Tooltip("Size of the detection area")]
    public float detectionRadius = 2f;

    [Header("Fluid Type Detection / Discrimination")]
    [Tooltip("If enabled, detector will check the fluid type before setting the 'isFluidPresent' flag")]
    public bool typeDiscrimination = false; // Default to original
    [Tooltip("Fluid type to check with purityPercentThreshold. (Setting to \"Disabled\" will use the majority type)")]
    public FluidType typeToDetect = FluidType.Disabled;
    [Tooltip("% of particles that must be of correct type to set the 'isFluidPresent' flag. (0 to 1) (1 means 100% pure)")]
    [Range(0f, 1f)]
    public float purityPercentThreshold = 0.75f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;
    public bool showDensityValue = true;
    [SerializeField] private Vector2 densityDisplayOffset = new Vector2(0, 30f);
    public bool isFluidPresent { get; private set; }
    public float currentDensity { get; private set; }

    // Below 3 only take meaningful values if "typeDetection" is enabled. Pollable by game managers with getters.
    public FluidType majorityType { get; private set; }
    public float particlePercentage { get; private set; }
    public int numParticles { get; private set; }

    private GameObject simulationGameobject;
    private IFluidSimulation fluidSimulation;
    private float nextCheckTime;
    private bool isRequestMade = false;

    // Below only take meaningful values/are initialized if "typeDetection" is true
    private int numFluidTypes = Enum.GetValues(typeof(FluidType)).Length - 1;
    private int[] numType; // Number of particles of correct type

    void Start()
    {
        simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        // Find the fluid simulation in the scene
        fluidSimulation = simulationGameobject.GetComponent<IFluidSimulation>();
        if (fluidSimulation == null)
        {
            Debug.LogError("No Simulation2D found in the scene!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!isManagedSensor && Time.time >= nextCheckTime)
        {
            // CheckFluidDensity();
            //sends async data request to GPU after each check time.
            if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid())
                return;
            if (!isRequestMade)
            {
                AsyncGPUReadback.Request(fluidSimulation.GetParticleBuffer(), ParticleRequest);
                isRequestMade = true;
            }
            nextCheckTime = Time.time + checkInterval;
        }
    }

    protected override void ParticleRequest(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU ASync Readback Error in Fluid Simulation Readback");
            isRequestMade = false;
            return;
        }
        if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid() || this == null)
            return;

        // Create temporary array to get particle positions
        Particle[] particles = request.GetData<Particle>().ToArray();

        CheckSensor(particles);
        isRequestMade = false;
    }

    // Performs fluid check as callback to async read
    public override void CheckSensor(Particle[] particles)
    {
        Vector2 checkPosition = transform.position;
        float totalDensity = 0f;

        // Calculate density similar to the simulation's density calculation
        float sqrRadius = detectionRadius * detectionRadius;

        FluidType oldMajorityType = majorityType;
        if (typeDiscrimination)
        {
            numParticles = 0; // Total number of particles
            numType = new int[numFluidTypes]; // Number of particles of each type
            majorityType = FluidType.Disabled;
        }

        foreach (Particle particle in particles)
        {
            if(particle.type == FluidType.Disabled){
                continue;
            }
            Vector2 particlePos = particle.position;
            Vector2 offsetToParticle = particlePos - checkPosition;
            float sqrDstToParticle = Vector2.Dot(offsetToParticle, offsetToParticle);

            if (sqrDstToParticle < sqrRadius)
            {
                float dst = Mathf.Sqrt(sqrDstToParticle);
                // Using a simplified density kernel for detection
                totalDensity += (1 - (dst / detectionRadius)) * (1 - (dst / detectionRadius));

                if (typeDiscrimination)
                {
                    numType[(int)particle.type - 1]++; // -1 because we don't count Disabled particles
                    numParticles++;
                }
            }
        }

        // Update fluid presence flag
        bool previousState = isFluidPresent;
        currentDensity = totalDensity;

        if (typeDiscrimination) // Typed detection
        {
            int maxPCount = numType.Max(); // Largest particle count
            majorityType = numParticles == 0 ? FluidType.Disabled : (FluidType)(numType.ToList().IndexOf(maxPCount)) + 1; // Type with majority of particles; index + 1, because we did not log Disabled particles

            if (typeToDetect != FluidType.Disabled) // We only discriminate on type if "typeToDetect" is not disabled
            {
                particlePercentage = numParticles == 0 ? 0 : numType[(int)typeToDetect - 1] / numParticles; // Use targetted type
            } 
            else
            {
                particlePercentage = numParticles == 0 ? 0 : numType[(int)majorityType - 1] / numParticles; // Use majority type
            }

            isFluidPresent = totalDensity > densityThreshold && particlePercentage > purityPercentThreshold; // Check if fluid is present, and that it is pure

            if (showDebugLogs && oldMajorityType != majorityType)
            {
                Debug.Log("[" + gameObject.name + "]: New majorityType is: " + majorityType);
            }
        }
        else // Original functionality
        {
            isFluidPresent = totalDensity > densityThreshold; // Check that fluid is present
        }

        // Notify if state changed
        if (previousState != isFluidPresent)
        {
            OnFluidPresenceChanged();
        }
    }

    void OnFluidPresenceChanged()
    {
        // You can add custom events or UnityEvents here to notify other scripts
        if (showDebugLogs)
            Debug.Log($"Fluid presence changed to: {isFluidPresent} at {gameObject.name}");
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw detection radius
        Gizmos.color = isFluidPresent ? Color.blue : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void OnGUI()
    {
        if (!showDensityValue) return;

        // Convert world position to screen position
        Vector3 worldPosition = transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        
        // Adjust for GUI coordinate system and offset
        screenPos.y = Screen.height - screenPos.y; // Flip Y coordinate
        Vector2 displayPos = new Vector2(screenPos.x + densityDisplayOffset.x, screenPos.y + densityDisplayOffset.y);

        // Display the density value
        string densityText = $"Density: {currentDensity:F2}";
        GUI.Label(new Rect(displayPos.x - 50, displayPos.y, 100, 20), densityText);
    }

    void OnDestroy()
    {
        if (isRequestMade)
        {
            AsyncGPUReadback.WaitAllRequests();
        }

    }
}