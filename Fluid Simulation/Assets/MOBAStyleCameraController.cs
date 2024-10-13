using UnityEngine;

public class MOBAStyleCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float minMoveSpeed = 2f; // Minimum speed when movement starts
    public float maxMoveSpeed = 100f; // Max movement speed
    public float speedRampUpTime = 3f; // Time in seconds to reach full speed
    public float mouseEdgeThreshold = 60f; // Threshold distance from the screen edges in pixels
    public bool lockHorizontal = false; // Lock movement to horizontal axis
    public bool lockVertical = false;   // Lock movement to vertical axis
    public float smoothTime = 0.2f; // Smoothing factor for the camera movement
    
    [Header("Camera Movement Boundaries")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -50f;
    public float maxY = 50f;

    private Vector3 targetPosition; // The target position the camera is moving towards
    private Vector3 velocity = Vector3.zero; // Required for smooth dampening
    private float currentSpeed; // Current ramping speed
    private float speedRamp; // The calculated ramp based on time near edge
    void Start()
    {
        // Initialize targetPosition and currentSpeed
        targetPosition = transform.position;
        currentSpeed = minMoveSpeed; // Start at minimum speed
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
        bool isMoving = false; // Track if the camera should be moving

        // Check horizontal screen edges
        if (!lockVertical)
        {
            if (mousePos.x <= mouseEdgeThreshold) // Left edge
            {
                moveDirection.x = -1;
                isMoving = true;
            }
            else if (mousePos.x >= Screen.width - mouseEdgeThreshold) // Right edge
            {
                moveDirection.x = 1;
                isMoving = true;
            }
        }

        // Check vertical screen edges
        if (!lockHorizontal)
        {
            if (mousePos.y <= mouseEdgeThreshold) // Bottom edge
            {
                moveDirection.y = -1;
                isMoving = true;
            }
            else if (mousePos.y >= Screen.height - mouseEdgeThreshold) // Top edge
            {
                moveDirection.y = 1;
                isMoving = true;
            }
        }

        // If moving, ramp up the speed over time
        if (isMoving)
        {
            speedRamp += Time.deltaTime / speedRampUpTime; // Ramp speed over time
            currentSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, speedRamp); // Interpolate speed
        }
        else
        {
            // Reset speed and ramp when not moving
            currentSpeed = minMoveSpeed;
            speedRamp = 0f;
        }

        // Calculate the new target position based on ramped-up speed
        targetPosition += moveDirection * currentSpeed * Time.deltaTime;

        // Clamp the target position to stay within defined boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
    }

    // Smoothly move the camera to the target position
    private void MoveCameraSmoothly()
    {
        // Use SmoothDamp to gradually move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
