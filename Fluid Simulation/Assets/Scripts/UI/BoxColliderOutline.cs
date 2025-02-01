using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[ExecuteInEditMode] // This makes it run in editor
public class BoxColliderOutline : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private BoxCollider2D boxCollider;
    
    void Start()
    {
        // Setup Line Renderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 4; // 4 points to close the rectangle
        // Get collider bounds
        boxCollider = GetComponent<BoxCollider2D>();
        UpdateOutline();
    }
    
    void UpdateOutline()
    {
        Vector2 size = boxCollider.size;
        Vector2 offset = boxCollider.offset;
        
        // Calculate corners
        Vector3[] positions = new Vector3[4];
        positions[0] = transform.TransformPoint(new Vector3(-size.x/2 + offset.x, -size.y/2 + offset.y, 0));
        positions[1] = transform.TransformPoint(new Vector3(size.x/2 + offset.x, -size.y/2 + offset.y, 0));
        positions[2] = transform.TransformPoint(new Vector3(size.x/2 + offset.x, size.y/2 + offset.y, 0));
        positions[3] = transform.TransformPoint(new Vector3(-size.x/2 + offset.x, size.y/2 + offset.y, 0));
        
        lineRenderer.SetPositions(positions);
    }



    private Vector3 lastSize;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Update()
    {
        // Only update if something has changed
        if (lastSize != transform.localScale || 
            lastPosition != transform.position ||
            lastRotation != transform.rotation)
        {
            UpdateOutline();
            
            // Store current values
            lastSize = transform.localScale;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }


}