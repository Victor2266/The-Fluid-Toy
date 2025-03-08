using UnityEngine;
using UnityEngine.Rendering;

public class ThermalSensor : MonoBehaviour
{
    public enum DetectionType
    {
        Disabled,
        GreaterThan,
        LessThan,
        Equals
    }

    [Header("Detection Settings")]
    [Tooltip("Temperature Threshold")]
    public float temperatureThreshold = 100f;

    [Tooltip("Detection Type")]
    public DetectionType detectType = DetectionType.GreaterThan;

    [Tooltip("How often to check temperature")]
    public float checkInterval = 0.1f;

    [Tooltip("Size of the detection area")]
    public float detectionRadius = 2f;

    [Tooltip("Percent Margin for Threshold (0.1 = 10%)")]
    public float errorMargin = 0.01f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;
    public bool showTempValue = true;
    public bool isManagedSensor = false;
    [SerializeField] private Vector2 displayOffset = new Vector2(0, 30f);
    public bool metThreshold { get; set; }
    public float currentTemperature { get; set; }

    private GameObject simulationGameobject;
    private IFluidSimulation fluidSimulation;
    private float nextCheckTime;

    public bool isRequestMade { get; set; } = false;

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
        currentTemperature = particleCount == 0 ? 0 : totalTemp/particleCount;
        metThreshold = doCompare(currentTemperature);

        // Notify if state changed
        if (previousState != metThreshold)
        {
            OnTempThresholdMet();
        }

        if (showDebugLogs)
            Debug.Log($"Avg Tmp is: {currentTemperature} at {gameObject.name}");

        isRequestMade = false;
    }

    public bool doCompare(float currentValue)
    {
        bool toReturn;
        switch (detectType)
        {
            case DetectionType.GreaterThan:
                toReturn = currentValue > (temperatureThreshold * (1 + errorMargin));
                break;
            case DetectionType.LessThan:
                toReturn = currentValue < (temperatureThreshold * (1 + errorMargin));
                break;
            case DetectionType.Equals:
                toReturn = Mathf.Abs(currentValue - temperatureThreshold) < (errorMargin * temperatureThreshold);
                break;
            case DetectionType.Disabled:
                toReturn = false;
                break;
            default:
                toReturn = false;
                break;
        }
        return toReturn;
    }

    public void OnTempThresholdMet()
    {
        // You can add custom events or UnityEvents here to notify other scripts
        if (showDebugLogs)
            Debug.Log($"Temperature threshold condition changed to: {metThreshold} at {gameObject.name}");
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw detection radius
        Gizmos.color = metThreshold ? Color.blue : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void OnGUI()
    {
        if (!showTempValue) return;

        // Convert world position to screen position
        Vector3 worldPosition = transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // Adjust for GUI coordinate system and offset
        screenPos.y = Screen.height - screenPos.y; // Flip Y coordinate
        Vector2 displayPos = new Vector2(screenPos.x + displayOffset.x, screenPos.y + displayOffset.y);

        // Display the density value
        string text = $"Temperature: {currentTemperature:F2}";
        GUI.Label(new Rect(displayPos.x - 50, displayPos.y, 150, 20), text);
    }

    void OnDestroy()
    {
        if (isRequestMade)
        {
            AsyncGPUReadback.WaitAllRequests();
        }

    }
}