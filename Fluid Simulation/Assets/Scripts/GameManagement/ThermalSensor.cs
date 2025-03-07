using UnityEngine;
using UnityEngine.Rendering;

public class ThermalSensor : MonoBehaviour
{
    public enum DetectionType
    {
        Greaterthan,
        Lessthan,
        Equals
    }

    [Header("Detection Settings")]
    [Tooltip("Temperature Threshold")]
    public float temperatureThreshold = 100f;

    [Tooltip("Detection Type")]
    DetectionType detectType = DetectionType.Greaterthan;

    [Tooltip("How often to check temperature")]
    public float checkInterval = 0.1f;

    [Tooltip("Size of the detection area")]
    public float detectionRadius = 2f;

    [Tooltip("Percent Margin for Threshold (0.1 = 10%)")]
    public float errorMargin = 0.01f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;
    public bool showDensityValue = true;
    [SerializeField] private Vector2 displayOffset = new Vector2(0, 30f);
    public bool metThreshold { get; private set; }
    public float currentTemperature { get; private set; }

    private GameObject simulationGameobject;
    private IFluidSimulation fluidSimulation;
    private float nextCheckTime;

    private bool isRequestMade = false;

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
        if (Time.time >= nextCheckTime)
        {
            // CheckFluidDensity();
            //sends async data request to GPU after each check time.
            if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid())
                return;
            if (!isRequestMade)
            {
                AsyncGPUReadback.Request(fluidSimulation.GetParticleBuffer(), CheckTemperature);
                isRequestMade = true;
            }


            nextCheckTime = Time.time + checkInterval;
        }
    }


    // Performs fluid check as callback to async read
    void CheckTemperature(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU ASync Readback Error in Fluid Simulation Readback");
            return;
        }
        if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid() || this == null)
            return;

        Vector2 checkPosition = transform.position;
        float totalTemp = 0f;
        int particleCount = 0;

        // Create temporary array to get particle positions
        Particle[] particles = request.GetData<Particle>().ToArray();

        // Calculate density similar to the simulation's density calculation
        float sqrRadius = detectionRadius * detectionRadius;

        foreach (Particle particle in particles)
        {
            if (particle.type == FluidType.Disabled)
            {
                continue;
            }
            Vector2 particlePos = particle.position;
            Vector2 offsetToParticle = particlePos - checkPosition;
            float sqrDstToParticle = Vector2.Dot(offsetToParticle, offsetToParticle);

            if (sqrDstToParticle < sqrRadius)
            {
                totalTemp += particle.temperature;
                particleCount++;
            }
        }

        // Update fluid presence flag
        bool previousState = metThreshold;
        currentTemperature = totalTemp/particleCount;

        switch (detectType)
        {
            case 
        }
        isFluidPresent = totalDensity > densityThreshold;

        // Notify if state changed
        if (previousState != isFluidPresent)
        {
            OnFluidPresenceChanged();
        }

        isRequestMade = false;
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