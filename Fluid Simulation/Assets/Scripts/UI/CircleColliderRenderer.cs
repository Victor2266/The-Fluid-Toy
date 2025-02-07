using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[ExecuteInEditMode] // This makes it run in editor
public class CircleColliderOutline : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private int segments = 50;
    
    void Start()
    {
        // Setup Line Renderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1; // 4 points to close the rectangle
        // Get collider bounds
        UpdateOutline();
    }
    
    void UpdateOutline()
    {
        float radius = transform.localScale.x / 2;
        
        Vector2 centerPosition = transform.position;
        
        // Calculate corners
         for (int i = 0; i <= segments; i++)

        {

            float angle = i * (Mathf.PI * 2) / segments;

            float x = centerPosition.x + Mathf.Cos(angle) * radius;

            float y = centerPosition.y + Mathf.Sin(angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0)); 

        }
        
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