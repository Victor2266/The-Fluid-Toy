using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float smoothSpeed = 5f;

    private void Awake()
    {
        // If no camera is assigned, use the main camera
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z; // Maintain original z-position

        if (smoothFollow)
        {
            // Smooth interpolation for smoother movement
            transform.position = Vector3.Lerp(transform.position, mousePosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Direct position assignment for maximum performance
            transform.position = mousePosition;
        }
    }
}