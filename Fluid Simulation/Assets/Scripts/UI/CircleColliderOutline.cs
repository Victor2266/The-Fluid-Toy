using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[ExecuteInEditMode] // This makes it run in editor
public class CircleColliderOutline : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private int segments = 50;

    private Vector3[] circlePositions;
    
    void Start()
    {
        // Setup Line Renderer
        PrecalculateCircle();
        InitializeLineRenderer();
        // Get collider bounds
        UpdateOutline();
        
    }
    
    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.enabled = true;
    }

    void PrecalculateCircle()
    {
        circlePositions = new Vector3[segments];
        float angleStep = 360f / segments;
        
        for(int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            circlePositions[i] = new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * angle),
                Mathf.Cos(Mathf.Deg2Rad * angle),
                0
            );
        }
    }

    void UpdateOutline()
    {
        float radius = transform.localScale.x / 2;
        
        Vector2 centerPosition = transform.position;
        
        for(int i = 0; i < segments; i++)
        {
            lineRenderer.SetPosition(i, centerPosition + (Vector2)(circlePositions[i] * radius));
        }
    }



    private Vector3 lastSize;
    private Vector3 lastPosition;

    void Update()
    {
        // Only update if something has changed
        if (lastSize != transform.localScale || lastPosition != transform.position)
        {
            UpdateOutline();
            
            // Store current values
            lastSize = transform.localScale;
        }
    }


}