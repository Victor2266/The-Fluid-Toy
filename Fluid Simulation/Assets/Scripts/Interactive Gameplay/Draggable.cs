using Unity.VisualScripting;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    protected bool isDragging = false;
    protected Vector3 offset;

    public float smoothingSpeed = 0.2f; // Adjust this value to control the smoothing speed
    public bool enableSmoothing = true; // Boolean to enable/disable smoothing
    public bool resizable = false; // Controls whether the object can be resized
    public bool uniformScaling = false; // Controls whether the object scales uniformly
    public bool rotatable = false; // Controls whether the object can be rotated with ctrl shortcut
    public float scaleSpeed = 0.1f; // Controls how fast the object scales
    public float rotationSpeed = 10f; // Degrees per scroll tick

    public float minScale = 0.1f; // Minimum scale limit
    public float maxScale = 5f; // Maximum scale limit
    public Vector3 targetScale;
    private Rigidbody2D rb2d;

    protected virtual void Start()
    {
        targetScale = transform.localScale;
        rb2d = GetComponent<Rigidbody2D>();
        
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)){
            isDragging = true;
            if (rb2d != null) {
                rb2d.bodyType = RigidbodyType2D.Kinematic;
                rb2d.freezeRotation = true;
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }

            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (rb2d != null) {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.freezeRotation = false;
        }
        
    }

    protected virtual void Update()
    {
        HandleDragging();
        HandleResizing();
        HandleRotating();

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, smoothingSpeed);
    }

    protected virtual void HandleDragging()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPosition = mousePosition + offset;

            if (enableSmoothing)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }

    private void HandleResizing()
    {
        if (!resizable || !isDragging) return;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta == 0) return;

        // Calculate the scaling factor (10% per scroll tick)
        float scaleFactor = scrollDelta > 0 ? 1.1f : 0.9f;

        if (uniformScaling)
        {
            float newScaleX = targetScale.x * scaleFactor;
            float newScaleY = targetScale.y * scaleFactor;

            newScaleX = Mathf.Clamp(newScaleX, minScale, maxScale);
            newScaleY = Mathf.Clamp(newScaleY, minScale, maxScale);
            targetScale = new Vector3(newScaleX, newScaleY, targetScale.z);
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Horizontal scaling
                float newScaleX = targetScale.x * scaleFactor;
                newScaleX = Mathf.Clamp(newScaleX, minScale, maxScale);
                targetScale = new Vector3(newScaleX, targetScale.y, targetScale.z);
            }
            else
            {
                // Vertical scaling
                float newScaleY = targetScale.y * scaleFactor;
                newScaleY = Mathf.Clamp(newScaleY, minScale, maxScale);
                targetScale = new Vector3(targetScale.x, newScaleY, targetScale.z);
            }
        }
    }

    private void HandleRotating()
    {
        if (!rotatable || !isDragging) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta == 0) return;

        // Check if Ctrl key is held down
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Rotate clockwise when scrolling down, counter-clockwise when scrolling up
            float rotationAmount = scrollDelta > 0 ? rotationSpeed : -rotationSpeed;
            transform.Rotate(0, 0, rotationAmount);
        }
    }

    public void setTargetScale(Vector3 newScale) {
        if (uniformScaling)
        {
            // newScale.x = newScale.x;
            newScale.y = newScale.x;
        }
        if (resizable)
        {
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            targetScale = newScale;
        }
    }
}