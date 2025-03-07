using UnityEngine;
using UnityEngine.Rendering;

// Generic Sensor Interface.
public abstract class FluidPropertySensor : MonoBehaviour
{
    public enum SensorEvent
    {
        Disabled,
        GreaterThan,
        LessThan,
        Equals
    }

    public string sensorName = "Default Name";
    public string propertyName = "Property Value";

    [Header("Detection Settings")]
    [Tooltip("Property Threshold")]
    public float propertyThreshold = 100f;

    [Tooltip("Sensor Polling Interval (in seconds)")]
    public float checkInterval = 0.1f;

    [Tooltip("Size of the detection area")]
    public float detectionRadius = 2f;

    [Tooltip("Event Trigger Conditions")]
    public SensorEvent eventTrigger = SensorEvent.Disabled;

    [Tooltip("Throw Event Continuously (Only Affects Sensor's \"ThrowEvent\" Function)")]
    public bool continuousThrow = false;

    [Tooltip("Trigger Margins (Allowable Percent Error for Threshold, e.g. 0.1 = 10%)")]
    public float percentMargin = 0f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;
    public bool showPropertyValue = true;
    [SerializeField] private Vector2 propertyDisplayOffset = new Vector2(0, 30f);
    public bool throwEvent { get; protected set; }
    public float currentValue { get; protected set; }

    protected GameObject simulationGameobject;
    protected IFluidSimulation fluidSimulation;
    protected volatile float nextCheckTime;

    protected volatile bool isRequestMade = false;

    protected virtual void Start()
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

    protected virtual void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            //sends async data request to GPU after each check time.
            if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid())
                return;
            if (!isRequestMade)
            {
                AsyncGPUReadback.Request(fluidSimulation.GetParticleBuffer(), SenseAsync);
                isRequestMade = true;
            }
            nextCheckTime = Time.time + checkInterval;
        }
    }

    // Performs fluid check as callback to async read
    // Overriding implementation must be thread-safe
    protected virtual void SenseAsync(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU ASync Readback Error in Fluid Simulation Readback");
            return;
        }
        if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid() || this == null)
            return;

        // Create temporary array to get particle values
        Particle[] particles = request.GetData<Particle>().ToArray();

        // Call check handler
        bool newThrowState = PerformCheck(particles);
        bool previousThrowState = throwEvent;

        // Throw event based on trigger conditions
        if ( (previousThrowState != newThrowState && newThrowState) || (continuousThrow && newThrowState) )
        {
            ThrowEvent();
        }

        isRequestMade = false;
    }

    // Must be implemented for each sensor, must be thread safe.
    // Should update "currentValue"
    // Should return a bool representing the new value of "throwEvent"
    // If eventTrigger is disabled, return does not matter.
    protected abstract bool PerformCheck(Particle[] particles);

    // Performs comparison based on event trigger settings
    // Is thread safe.
    protected bool doCompare(float currentValue, float thresholdValue)
    {
        bool toReturn;
        switch (eventTrigger)
        {
            case SensorEvent.GreaterThan:
                toReturn = currentValue > (propertyThreshold * (1 + percentMargin));
                break;
            case SensorEvent.LessThan:
                toReturn = currentValue < (propertyThreshold * (1 + percentMargin));
                break;
            case SensorEvent.Equals:
                toReturn = Mathf.Abs(currentValue - propertyThreshold) < (percentMargin * propertyThreshold);
                break;
            case SensorEvent.Disabled:
                toReturn = false;
                break;
            default:
                toReturn = false;
                break;
        }
        return toReturn;
    }

    // Any overrides for this should be thread safe.
    protected virtual void ThrowEvent()
    {
        // You can add custom events or UnityEvents here to notify other scripts
        if (showDebugLogs)
            Debug.Log($"{sensorName} threw event! {propertyName}: {currentValue} at {gameObject.name}");
    }

    protected virtual void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw detection radius
        Gizmos.color = throwEvent ? Color.blue : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    protected virtual void OnGUI()
    {
        if (!showPropertyValue) return;

        // Convert world position to screen position
        Vector3 worldPosition = transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // Adjust for GUI coordinate system and offset
        screenPos.y = Screen.height - screenPos.y; // Flip Y coordinate
        Vector2 displayPos = new Vector2(screenPos.x + propertyDisplayOffset.x, screenPos.y + propertyDisplayOffset.y);

        // Display the property
        string text = $"{propertyName}: {currentValue:F2}";
        GUI.Label(new Rect(displayPos.x - 50, displayPos.y, 100, 20), text);
    }

    protected virtual void OnDestroy()
    {
        if (isRequestMade)
        {
            AsyncGPUReadback.WaitAllRequests();
        }

    }
}