using UnityEngine;
using UnityEngine.EventSystems;

public class Snapper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Object snapper
    [Header("Snapper Settings")]
    [Tooltip("Target GameObject to snap to")]
    public GameObject objectToSnapOn;
    [Tooltip("Circular snapping distance")]
    public float snapDistance = 0.5f;
    [Tooltip("Where to snap to on GameObject (relative position)")]
    public Vector3 snapOffset;

    [Header("Event (Optional)")]
    private SnapEventSO snapEventSO;

    private bool _isSnapped = false;
    private Rigidbody2D _rb; // Physics (optional)
    private Vector3 _dragStartPos;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Debug.Log("Distance for snap: "+ Vector3.Distance(transform.position, GetSnapPosition()));
        if (!_isSnapped && Vector3.Distance(transform.position, GetSnapPosition()) <= snapDistance)
        {
            SnapObj();
        }
    }

    void SnapObj()
    {
        _isSnapped = true;
        Debug.Log("Snap!");
        // Kill velocity and convert bodytype
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
        }
        transform.position = objectToSnapOn.transform.position + snapOffset;
        transform.rotation = objectToSnapOn.transform.rotation;

        // Set parent to track position, and then notify listeners if event exists
        transform.SetParent(objectToSnapOn.transform);
        if(snapEventSO != null) snapEventSO.RaiseSnap(objectToSnapOn);
    }

    public void Unsnap()
    {
        _isSnapped = false;
        Debug.Log("Unsnap!");
        transform.SetParent(null); // Undo
        if (_rb != null) _rb.bodyType = RigidbodyType2D.Dynamic; // Undo
        if(snapEventSO != null) snapEventSO.RaiseUnsnap(objectToSnapOn); // Notify
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSnapped) Unsnap();
        _dragStartPos = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Camera.main.ScreenToWorldPoint(eventData.position) + (Vector3.forward * 10f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isSnapped && Vector3.Distance(transform.position, GetSnapPosition()) > snapDistance)
        {
            transform.position = _dragStartPos;
        }
    }

    Vector3 GetSnapPosition()
    {
        return objectToSnapOn.transform.position + snapOffset;
    }
}
