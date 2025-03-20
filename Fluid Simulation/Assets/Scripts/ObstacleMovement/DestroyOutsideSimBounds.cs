using UnityEngine;

public class DestroyOutsideSimulationBounds : MonoBehaviour
{
    [Tooltip("Tag of the simulation GameObject")]
    public string simulationTag = "Simulation";
    
    [Tooltip("Optional buffer outside bounds before destroying")]
    public float boundsBuffer = 0.5f;
    
    [Tooltip("How often to check if object is outside bounds (seconds)")]
    public float checkInterval = 0.5f;
    
    private IFluidSimulation sim;
    private Vector2 simulationBounds;
    private float timeSinceLastCheck = 0f;
    
    void Start()
    {
        // Find the fluid simulation in the scene
        GameObject simulationGameobject = GameObject.FindGameObjectWithTag(simulationTag);
        
        if (simulationGameobject == null)
        {
            Debug.LogError($"No GameObject with tag '{simulationTag}' found. DestroyOutsideSimulationBounds script will not work correctly.");
            enabled = false;
            return;
        }
        
        // Get the simulation component
        sim = simulationGameobject.GetComponent<IFluidSimulation>();
        
        if (sim == null)
        {
            Debug.LogError("IFluidSimulation component not found on the tagged GameObject. DestroyOutsideSimulationBounds script will not work correctly.");
            enabled = false;
            return;
        }
        
        // Get initial simulation bounds
        UpdateSimulationBounds();
    }
    
    void Update()
    {
        // Check bounds periodically for performance
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkInterval)
        {
            timeSinceLastCheck = 0f;
            
            // Update bounds from simulation in case they changed
            UpdateSimulationBounds();
            
            // Check if object is completely outside bounds
            if (IsCompletelyOutsideSimulationBounds())
            {
                DestroyObject();
            }
        }
    }
    
    private void UpdateSimulationBounds()
    {
        if (sim != null)
        {
            // Get the Vector2 bounds from the simulation
            simulationBounds = sim.getBounds();
        }
    }
    
    private bool IsCompletelyOutsideSimulationBounds()
    {
        // Assuming simulation is centered at (0,0)
        // If your simulation has a different center, adjust these calculations
        float halfWidth = simulationBounds.x / 2f + boundsBuffer;
        float halfHeight = simulationBounds.y / 2f + boundsBuffer;
        
        // Define simulation boundaries
        float minX = -halfWidth;
        float maxX = halfWidth;
        float minY = -halfHeight;
        float maxY = halfHeight;
        
        // Get the object's position
        Vector3 position = transform.position;
        
        // Get the object's scale (assuming scale represents the size of a 1x1 collider)
        Vector3 scale = transform.lossyScale;
        
        // Calculate the extent (half size) of the object in each direction
        float extentX = scale.x / 2f;
        float extentY = scale.y / 2f;
        
        // Calculate the outermost points of the object
        float objectMinX = position.x - extentX;
        float objectMaxX = position.x + extentX;
        float objectMinY = position.y - extentY;
        float objectMaxY = position.y + extentY;
        
        // Check if the object is completely outside any of the boundaries
        return objectMaxX < minX || // Completely to the left
               objectMinX > maxX || // Completely to the right
               objectMaxY < minY || // Completely below
               objectMinY > maxY;   // Completely above
    }
    
    private void DestroyObject()
    {
        // Log for debugging
        Debug.Log($"Object {gameObject.name} at position {transform.position} with scale {transform.lossyScale} destroyed because it went completely outside simulation bounds");
        
        // Destroy the GameObject
        Destroy(gameObject);
    }
}