using UnityEngine;

public class MOBAStyleCameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Camera movement speed
    public float edgeThreshold = 20f; // Threshold distance from the screen edges in pixels
    public bool lockHorizontal = false; // Lock movement to horizontal axis
    public bool lockVertical = false;   // Lock movement to vertical axis
    public float smoothTime = 0.2f; // Smoothing factor for the camera movement

    // Boundaries for camera movement
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    private Vector3 targetPosition; // The target position the camera is moving towards
    private Vector3 velocity = Vector3.zero; // Required for smooth dampening

    void Start()
    {
        // Initialize targetPosition to the current camera position
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleMouseMovement();
        MoveCameraSmoothly();
    }

    // Detect mouse position and set the movement direction
    private void HandleMouseMovement()
    {
        Vector3 moveDirection = Vector3.zero; // Reset movement direction each frame
        Vector2 mousePos = Input.mousePosition;

        // Check horizontal screen edges
        if (!lockVertical)
        {
            if (mousePos.x <= edgeThreshold) // Left edge
            {
                moveDirection.x = -1;
            }
            else if (mousePos.x >= Screen.width - edgeThreshold) // Right edge
            {
                moveDirection.x = 1;
            }
        }

        // Check vertical screen edges
        if (!lockHorizontal)
        {
            if (mousePos.y <= edgeThreshold) // Bottom edge
            {
                moveDirection.z = -1;
            }
            else if (mousePos.y >= Screen.height - edgeThreshold) // Top edge
            {
                moveDirection.z = 1;
            }
        }

        // Calculate the new target position based on movement direction and speed
        targetPosition += moveDirection * moveSpeed * Time.deltaTime;

        // Clamp the target position to stay within defined boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
    }

    // Smoothly move the camera to the target position
    private void MoveCameraSmoothly()
    {
        // Use SmoothDamp to gradually move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
