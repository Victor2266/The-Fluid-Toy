using UnityEngine;

public class FluidDetector : MonoBehaviour 
// This only works for the regular simulation and not the AoS version right now because it accesses the buffers like positionBuffer,
// which we replaced with the particle struct buffer
{
    [Header("Detection Settings")]
    [Tooltip("The density threshold above which fluid is considered present")]
    public float densityThreshold = 0.5f;
    
    [Tooltip("How often to check for fluid presence (in seconds)")]
    public float checkInterval = 0.1f;
    
    [Tooltip("Size of the detection area")]
    public float detectionRadius = 2f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;
    public bool showDensityValue = true;
    [SerializeField] private Vector2 densityDisplayOffset = new Vector2(0, 30f);
    public bool isFluidPresent { get; private set; }
    public float currentDensity { get; private set; }
    private Simulation2D fluidSimulation;
    private float nextCheckTime;

    void Start()
    {
        // Find the fluid simulation in the scene
        fluidSimulation = FindObjectOfType<Simulation2D>();
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
            CheckFluidDensity();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    void CheckFluidDensity()
    {
        if (fluidSimulation == null || fluidSimulation.positionBuffer == null)
            return;

        Vector2 checkPosition = transform.position;
        float totalDensity = 0f;
        
        // Create temporary array to get particle positions
        Vector2[] positions = new Vector2[fluidSimulation.numParticles];
        fluidSimulation.positionBuffer.GetData(positions);

        // Calculate density similar to the simulation's density calculation
        float sqrRadius = detectionRadius * detectionRadius;

        foreach (Vector2 particlePos in positions)
        {
            Vector2 offsetToParticle = particlePos - checkPosition;
            float sqrDstToParticle = Vector2.Dot(offsetToParticle, offsetToParticle);

            if (sqrDstToParticle < sqrRadius)
            {
                float dst = Mathf.Sqrt(sqrDstToParticle);
                // Using a simplified density kernel for detection
                totalDensity += (1 - (dst / detectionRadius)) * (1 - (dst / detectionRadius));
            }
        }

        // Update fluid presence flag
        bool previousState = isFluidPresent;
        currentDensity = totalDensity;
        isFluidPresent = totalDensity > densityThreshold;

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
}