using UnityEngine;

[RequireComponent(typeof(Draggable))]
public class Snapper : MonoBehaviour
{
    [Header("Positional Snap Settings")]
    public GameObject objectToSnapOn;
    public float snapDistance = 0.5f;
    public Vector3 snapOffset;
    public float rotationalOffset;

    [Header("Angular Snap Settings")]
    [Tooltip("Allowed angle difference before unsnap (degrees)")]
    public float angleSlack = 15f;

    [Header("Event (Optional)")]
    private SnapEventSO snapEventSO;

    private bool _isSnapped = false;
    private Rigidbody2D _rb;
    private Draggable _draggable;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _draggable = GetComponent<Draggable>();
    }

    void FixedUpdate()
    {
        if (!_isSnapped && !_draggable.getIsDragging()) {
            // Check if we should snap (position and angle)
            if (ShouldSnap()) SnapObj();
        } else if (_isSnapped) {
            // Check if we should unsnap (position or angle)
            if (ShouldUnsnap()) Unsnap();
        }
    }

    bool ShouldSnap()
    {
        // Check distance
        bool inPosition = Vector3.Distance(transform.position, GetSnapPosition()) <= snapDistance;

        // Check angle (convert to 0-360 range)
        float currentAngle = transform.eulerAngles.z;
        float targetAngle = objectToSnapOn.transform.eulerAngles.z;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

        bool inAngle = angleDifference <= angleSlack;

        return inPosition && inAngle;
    }

    bool ShouldUnsnap()
    {
        // Unsnap if too far positionally
        if (Vector3.Distance(transform.position, GetSnapPosition()) > snapDistance)
            return true;

        // Unsnap if rotated beyond angle slack
        float currentAngle = transform.eulerAngles.z;
        float targetAngle = objectToSnapOn.transform.eulerAngles.z;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

        return angleDifference > angleSlack;
    }

    void SnapObj()
    {
        _isSnapped = true;
        Debug.Log("Snap!");

        // NOTE: Do not set linear or angular velocity to 0. This results in strange acceleration when snapping/unsnapping, is it is undefined behaviour
        if (_rb != null) _rb.bodyType = RigidbodyType2D.Kinematic;

        // Immediate snap to position and rotation
        transform.SetParent(objectToSnapOn.transform);
        transform.localPosition = snapOffset;
        transform.localRotation = Quaternion.Euler(0, 0, rotationalOffset); //Quaternion.identity;

        if (snapEventSO != null) snapEventSO.RaiseSnap(objectToSnapOn);
    }

    public void Unsnap()
    {
        _isSnapped = false;
        Debug.Log("Unsnap!");

        transform.SetParent(null);

        if (_rb != null) _rb.bodyType = RigidbodyType2D.Dynamic;

        if (snapEventSO != null) snapEventSO.RaiseUnsnap(objectToSnapOn);
    }

    Vector3 GetSnapPosition()
    {
        return objectToSnapOn.transform.position + snapOffset;
    }
}