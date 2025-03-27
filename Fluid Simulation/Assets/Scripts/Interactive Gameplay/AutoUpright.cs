using UnityEngine;

[RequireComponent(typeof(Draggable))]
public class AutoUpright : MonoBehaviour
{
    [Header("Uprighter Settings")]
    public bool enableUprighting = true;
    public float rotationalSpeed = 5f; // How fast the object rotates back to upright
    public float delay = 0.5f; // How long after pickup before correction starts
    public float maxAngularVelocity; // Limits the angular acceleration that can be applied. Generally not an issue
    [Tooltip("How quickly to dampen rotation when not correcting")]
    public float rotationDamping = 2f;

    private Draggable _draggable;
    private Rigidbody2D _rb;
    private float timeSincePickup;
    private bool isCorrecting;
    private float initialAngularVelocity;

    private void Awake()
    {
        _draggable = GetComponent<Draggable>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_draggable.getIsDragging())
        {
            // While dragging, allow rotation but dampen it slightly
            timeSincePickup = 0f;
            isCorrecting = false;

            if (_rb != null && !Input.GetMouseButton(1)) // Only dampen when not actively rotating
            {
                _rb.angularVelocity = Mathf.Lerp(_rb.angularVelocity, 0f, rotationDamping * Time.deltaTime);
            }
        }
        else
        {
            // Count time since release
            timeSincePickup += Time.deltaTime;

            // Start correction after delay
            if (timeSincePickup > delay)
            {
                isCorrecting = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        if (isCorrecting)
        {
            // Current rotation normalized to [-180,180]
            float currentRotation = _rb.rotation % 360;
            if (currentRotation > 180) currentRotation -= 360;

            // Calculate target rotation (progressively move toward 0)
            float targetRotation = Mathf.Lerp(currentRotation, 0f, rotationalSpeed * Time.fixedDeltaTime);

            // Calculate the shortest rotation difference
            float rotationDelta = Mathf.DeltaAngle(currentRotation, targetRotation);

            // Apply rotation while preserving physics
            _rb.angularVelocity += rotationDelta;

            // Apply damping
            _rb.angularVelocity = Mathf.Lerp(_rb.angularVelocity, 0f, rotationalSpeed * Time.fixedDeltaTime);

            // Stop correcting when nearly upright
            if (Mathf.Abs(currentRotation) < 0.5f && Mathf.Abs(_rb.angularVelocity) < 1f)
            {
                _rb.rotation = 0f;
                _rb.angularVelocity = 0f;
                isCorrecting = false;
            }
        }
    }
}