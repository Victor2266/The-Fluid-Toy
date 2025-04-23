using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class Draggable : MonoBehaviour
{
    protected bool isDragging = false;
    protected bool draggingLastFrame = false;
    protected Vector3 offset;
    protected Vector3 currentVelocity;

    [Header("General Settings")]
    [Tooltip("Smoothing Settings")]
    public bool enableSmoothing = true; // Boolean to enable/disable smoothing
    public float smoothingSpeed = 0.2f; // Adjust this value to control the smoothing speed

    [Tooltip("Resizing Settings")]
    public bool resizable = false; // Controls whether the object can be resized
    public bool uniformScaling = false; // Controls whether the object scales uniformly
    public float scaleSpeed = 0.1f; // Controls how fast the object scales

    [Tooltip("Rotation Settings")]
    public bool rotatable = false; // Controls whether the object can be rotated with ctrl shortcut
    public float rotationSpeed = 10f; // Degrees per scroll tick

    [Tooltip("Max Speed Settings")]
    public float maxDragSpeed = Mathf.Infinity; // Maximum dragging speed (units per second)
    private float currentSpeed;

    [Tooltip("Raycast Settings")]
    public LayerMask raycastLayerMask = -1; // Default to "Everything"
    public float raycastMaxDistance = 100f; // Maximum raycast distance
    
    [Tooltip("Collider Cache Settings")]
    public bool autoRefreshColliders = true; // Whether to automatically refresh colliders cache
    public float colliderRefreshInterval = 1.0f; // How often to check for collider changes (in seconds)
    private float lastColliderRefreshTime = 0f;

    public float minScale = 0.1f; // Minimum scale limit
    public float maxScale = 5f; // Maximum scale limit
    public Vector3 targetScale;
    private Rigidbody2D rb2d;
    
    // Cache all colliders in this object and its children
    private Collider2D[] allColliders;

    protected virtual void Start()
    {
        targetScale = transform.localScale;
        rb2d = GetComponent<Rigidbody2D>();
        
        // Cache all colliders in this object and its children
        RefreshColliders();
    }
    
    /// <summary>
    /// Refreshes the cached colliders array. Call this method when children are added or removed.
    /// </summary>
    public void RefreshColliders()
    {
        allColliders = GetComponentsInChildren<Collider2D>();
    }
    
    /// <summary>
    /// Called by Unity when children are added or removed from this transform.
    /// Automatically refreshes the colliders cache.
    /// </summary>
    protected virtual void OnTransformChildrenChanged()
    {
        RefreshColliders();
    }
    
    /// <summary>
    /// Called by Unity when this object is enabled.
    /// Ensures the colliders cache is up-to-date.
    /// </summary>
    protected virtual void OnEnable()
    {
        RefreshColliders();
    }

    protected virtual void Update()
    {
        // Periodically refresh colliders if enabled
        if (autoRefreshColliders && Time.time - lastColliderRefreshTime > colliderRefreshInterval)
        {
            RefreshColliders();
            lastColliderRefreshTime = Time.time;
        }
        
        HandleMouseInput();
        HandleDragging();
        HandleResizing();
        HandleRotating();

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, smoothingSpeed);
    }
    
    protected virtual void HandleMouseInput()
    {
        // Check for mouse button down
        if (Input.GetMouseButtonDown(0) && IsMouseOverObject())
        {
            if (!isDragging) isDragging = true;
            else draggingLastFrame = true;

            if (rb2d != null && !draggingLastFrame)
            {
                rb2d.bodyType = RigidbodyType2D.Kinematic;
                rb2d.freezeRotation = true;
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }

            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        // Check for mouse button up
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            draggingLastFrame = false;
            if (rb2d != null)
            {
                rb2d.bodyType = RigidbodyType2D.Dynamic;
                rb2d.freezeRotation = false;
                rb2d.AddForce(currentVelocity, ForceMode2D.Impulse);
            }
        }
    }
    
    protected virtual bool IsMouseOverObject()
    {
        // Get mouse position in world space
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // First, check if we're already dragging - in that case, we want to maintain the drag
        // even if the mouse moves outside the colliders
        if (isDragging && Input.GetMouseButton(0))
        {
            return true;
        }
        
        // Check if any of our cached colliders are under the mouse
        if (allColliders != null && allColliders.Length > 0)
        {
            foreach (Collider2D collider in allColliders)
            {
                // Skip null colliders (might have been destroyed)
                if (collider == null) continue;
                
                // Skip disabled colliders
                if (!collider.enabled) continue;
                
                // Check if the collider contains the mouse point
                if (collider.OverlapPoint(mousePosition))
                {
                    return true;
                }
            }
        }
        
        // If direct collider check failed, try a raycast as a fallback
        // This can help with complex collider shapes or edge cases
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero, raycastMaxDistance, raycastLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // Check if the hit collider is part of this object or its children
                Transform hitTransform = hit.collider.transform;
                while (hitTransform != null)
                {
                    if (hitTransform == transform)
                    {
                        return true;
                    }
                    hitTransform = hitTransform.parent;
                }
            }
        }
        
        return false;
    }

    protected virtual void HandleDragging()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPosition = mousePosition + offset;

            // Calculate raw movement vector
            Vector3 movement = targetPosition - transform.position;

            // Apply speed limiting if maxDragSpeed is not Infinity
            if (maxDragSpeed < Mathf.Infinity)
            {
                // Calculate speed in units per second
                currentSpeed = movement.magnitude / Time.deltaTime;

                if (currentSpeed > maxDragSpeed)
                {
                    // Normalize and scale by max speed
                    movement = movement.normalized * maxDragSpeed * Time.deltaTime;
                    targetPosition = transform.position + movement;
                }
            }

            // Save post-processed velocity
            currentVelocity = (targetPosition - transform.position) / Time.deltaTime;

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

    public bool getIsDragging()
    {
        return isDragging;
    }
}
