using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InteractionRadiusVisualizer : MonoBehaviour
{
    public IFluidSimulation simulation;
    public GameObject simulationGameObject;
    public Color attractColor = Color.green;
    public Color repelColor = Color.red;
    
    private LineRenderer lineRenderer;
    public int segments = 32;

    void Start()
    {
        simulation = simulationGameObject.GetComponent<IFluidSimulation>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments;
        lineRenderer.loop = true;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        bool isInteracting = Input.GetMouseButton(0) || Input.GetMouseButton(1);
        lineRenderer.enabled = isInteracting;

        if (!isInteracting) return;

        // Set color based on interaction type
        lineRenderer.startColor = Input.GetMouseButton(0) ? attractColor : repelColor;
        lineRenderer.endColor = lineRenderer.startColor;

        // Update circle positions
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float radius = simulation.GetInteractionRadius();
        
        float angle = 0f;
        float angleStep = 360f / segments;
        
        for(int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(mousePos.x + x, mousePos.y + y, 0));
            angle += angleStep;
        }
    }
}