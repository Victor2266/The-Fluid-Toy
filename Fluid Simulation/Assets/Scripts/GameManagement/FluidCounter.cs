using System.Linq; // Needed for LINQ Count()
using UnityEngine;
using UnityEngine.Rendering; // Needed for AsyncGPUReadback


public class FluidCounter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How often to update the count (in seconds)")]
    public float checkInterval = 0.2f; // Update 5 times per second

    [Tooltip("Display the count on the screen using OnGUI")]
    public bool showCountOnGUI = true;

    [Header("Live Data")]
    [Tooltip("The total number of active (non-disabled) particles found in the last check.")]
    [SerializeField] // Show in inspector but read-only is implied by private set
    private int _totalActiveParticles = 0;
    public int TotalActiveParticles => _totalActiveParticles; // Public read-only property

    // Internal state
    private IFluidSimulation fluidSimulation;
    private float nextCheckTime;
    private bool isRequestMade = false;

    protected virtual void Start()
    {
        // Find the simulation component in the scene
        FindSimulation();
        nextCheckTime = Time.time + checkInterval; // Schedule the first check
    }

    void FindSimulation()
    {
        if (fluidSimulation != null) return; // Already found

        GameObject simObject = GameObject.FindGameObjectWithTag("Simulation");
        if (simObject != null)
        {
            fluidSimulation = simObject.GetComponent<IFluidSimulation>();
            if (fluidSimulation == null)
            {
                //Debug.LogError($"[{nameof(FluidCounter)}]: GameObject '{simObject.name}' with tag 'Simulation' found, but it doesn't have an IFluidSimulation component.", this);
            }
            else {
                //Debug.Log($"[{nameof(FluidCounter)}]: Found simulation component on '{simObject.name}'.", this);
            }
        }
        else
        {
            //Debug.LogError($"[{nameof(FluidCounter)}]: Could not find GameObject with tag 'Simulation'. Make sure one exists and is tagged.", this);
        }
    }

    protected virtual void Update()
    {
        // Is it time to perform a check and no request is currently pending?
        if (Time.time >= nextCheckTime && !isRequestMade)
        {
            // Ensure the simulation buffer we need is valid before requesting
            if (fluidSimulation.IsPositionBufferValid())
            {
                // Request the entire particle buffer data from the GPU asynchronously
                AsyncGPUReadback.Request(fluidSimulation.GetParticleBuffer(), OnParticleDataReceived);
                isRequestMade = true; // Mark that a request is in flight
            }
            else
            {
                // Optional: Log a warning if the buffer isn't ready when expected
                // Debug.LogWarning($"[{nameof(TotalParticleCounter)}]: Simulation buffer not valid at check time.");
            }

            // Schedule the next check regardless of whether a request was made
            // This prevents rapid-fire checks if the buffer is often invalid
            nextCheckTime = Time.time + checkInterval;
        }
    }

    // Callback function executed when the GPU data is ready
    private void OnParticleDataReceived(AsyncGPUReadbackRequest request)
    {
        // Ensure this component instance hasn't been destroyed while waiting for the GPU
        if (this == null) return;

        isRequestMade = false; // Request is complete, allow a new one next cycle

        if (request.hasError)
        {
            Debug.LogError($"[{nameof(FluidCounter)}]: GPU Async Readback Error!");
            // Consider resetting count or disabling component if errors persist
            _totalActiveParticles = -1; // Indicate error state
            return;
        }

        // Double-check simulation still exists (less likely, but safe)
        if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid())
        {
            _totalActiveParticles = 0; // Reset count if simulation vanished
            return;
        }

        // --- Core Logic: Count Active Particles ---
        // Get the data from the GPU request (contains all particles)
        var particleData = request.GetData<Particle>();

        // Use LINQ to count particles where the type is not 'Disabled'
        // Note: Using ToArray() might be necessary if Count() doesn't work directly on ReadOnlySpan in your Unity version.
        _totalActiveParticles = particleData.ToArray().Count(p => p.type != FluidType.Disabled);
        // --- End Core Logic ---
    }

    // Display the count on screen using Unity's immediate mode GUI
    void OnGUI()
    {
        if (!showCountOnGUI) return; // Allow disabling the display

        // Define position and size for the label
        Rect labelRect = new Rect(10, 10, 300, 25); // Top-left corner

        // Format the text to display
        string countText = $"Total Active Particles: {_totalActiveParticles}";
        if (_totalActiveParticles < 0) {
            countText = "Particle Count Error"; // Show error state
        }

        // --- Optional: Basic styling for better visibility ---
        GUIStyle style = new GUIStyle(GUI.skin.label); // Start with default label style
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 14; // Make text a bit larger

        // Store original colors to restore them later
        Color originalBgColor = GUI.backgroundColor;
        Color originalContentColor = GUI.contentColor;

        // Set background and text colors for the label
        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.75f); // Dark semi-transparent background
        GUI.contentColor = Color.white; // White text

        // Draw a background box slightly larger than the text area
        GUI.Box(new Rect(labelRect.x - 2, labelRect.y - 2, labelRect.width + 4, labelRect.height + 4), GUIContent.none);

        // Draw the label with the count text and custom style
        GUI.Label(labelRect, countText, style);

        // Restore original GUI colors
        GUI.backgroundColor = originalBgColor;
        GUI.contentColor = originalContentColor;
        // --- End Optional Styling ---
    }

    // Optional: Handle cleanup if the object is destroyed
    void OnDestroy()
    {
        // AsyncGPUReadback requests usually handle themselves if the target is destroyed,
        // but explicit cleanup might be needed in complex scenarios.
        // Avoid blocking calls like WaitAllRequests() here.
        // if (isRequestMade) { Debug.LogWarning($"[{nameof(TotalParticleCounter)}] Destroyed with pending GPU readback request."); }
    }
}