using UnityEngine;

public class DestroyOffScreenObject : MonoBehaviour
{
    [Tooltip("How far below the camera's view the object should be before being destroyed")]
    public float destroyBelowScreenBy = 10f;
    
    [Tooltip("Optional: Also destroy if object goes beyond sides or top of screen")]
    public bool destroyIfOutsideScreenHorizontally = false;
    public bool destroyIfAboveScreen = false;
    
    [Tooltip("Horizontal buffer beyond screen edges")]
    public float horizontalBuffer = 5f;
    
    [Tooltip("Buffer above screen")]
    public float aboveScreenBuffer = 5f;
    
    private Camera mainCamera;
    
    void Start()
    {
        // Cache the main camera reference
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera found. DestroyOffScreenObject script will not work correctly.");
        }
    }
    
    void Update()
    {
        if (mainCamera == null) return;
        
        // Convert the object's position to viewport coordinates
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        
        // Check if below screen (y < 0 minus buffer)
        if (viewportPosition.y < 0 - (destroyBelowScreenBy / mainCamera.orthographicSize / 2))
        {
            DestroyObject();
            return;
        }
        
        // Optionally check if outside screen horizontally
        if (destroyIfOutsideScreenHorizontally)
        {
            float horizontalBufferViewport = horizontalBuffer / (mainCamera.orthographicSize * mainCamera.aspect * 2);
            if (viewportPosition.x < 0 - horizontalBufferViewport || viewportPosition.x > 1 + horizontalBufferViewport)
            {
                DestroyObject();
                return;
            }
        }
        
        // Optionally check if above screen
        if (destroyIfAboveScreen)
        {
            float aboveBufferViewport = aboveScreenBuffer / mainCamera.orthographicSize / 2;
            if (viewportPosition.y > 1 + aboveBufferViewport)
            {
                DestroyObject();
                return;
            }
        }
    }
    
    private void DestroyObject()
    {
        // Log for debugging
        Debug.Log($"Object {gameObject.name} destroyed because it went off screen");
        
        // Destroy the GameObject
        Destroy(gameObject);
    }
}