using UnityEngine;
using UnityEngine.EventSystems;

public class Snapper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Snapper Settings")]
    public GameObject objectToSnapOn;
    public float snapDistance = 0.5f;
    public Vector3 snapOffset;

    [Header("Event (Optional)")]
    private SnapEventSO snapEventSO;

    private bool _isSnapped = false;
    private Rigidbody2D _rb;
    private Vector3 _dragStartPos;
    private Draggable _draggable;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _draggable = GetComponent<Draggable>();
    }

    void Update()
    {
        // Only check for snap if not currently dragging
        if (!_isSnapped && !_draggable.isDragging &&
            Vector3.Distance(transform.position, GetSnapPosition()) <= snapDistance)
        {
            SnapObj();
        }
        
        if (_isSnapped &&
            Vector3.Distance(transform.position, GetSnapPosition()) > snapDistance)
        {
            Unsnap();
        }
    }

    void SnapObj()
    {
        _isSnapped = true;
        Debug.Log("Snap!");

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
        }

        transform.position = objectToSnapOn.transform.position + snapOffset;
        transform.rotation = objectToSnapOn.transform.rotation;
        transform.SetParent(objectToSnapOn.transform);

        if (snapEventSO != null) snapEventSO.RaiseSnap(objectToSnapOn);
    }

    public void Unsnap()
    {
        _isSnapped = false;
        Debug.Log("Unsnap!");

        transform.SetParent(null);

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
        }

        if (snapEventSO != null) snapEventSO.RaiseUnsnap(objectToSnapOn);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSnapped)
        {
            Unsnap();
        }
        _dragStartPos = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Empty - let Draggable handle the actual dragging
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