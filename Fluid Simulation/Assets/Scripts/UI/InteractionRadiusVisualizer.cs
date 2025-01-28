using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InteractionRadiusVisualizer : MonoBehaviour
{
    [Header("Dependencies")]
    private IFluidSimulation simulation;
    private GameObject simulationGameObject;
    
    [Header("Visual Settings")]
    [SerializeField] private Color attractColor = Color.green;
    [SerializeField] private Color repelColor = Color.red;
    [SerializeField] private Color neutralColor = Color.grey;
    [SerializeField] private bool alwaysShowDefault = true;
    [SerializeField] [Range(8, 64)] private int segments = 32;
    
    private LineRenderer lineRenderer;
    private Vector3[] circlePositions;
    private float lastRadius;
    private Vector2 lastMousePosition;

    void Start()
    {
        simulationGameObject = GameObject.FindGameObjectWithTag("Simulation");
        simulation = simulationGameObject.GetComponent<IFluidSimulation>();
        InitializeLineRenderer();
        PrecalculateCircle();
    }

    void Update()
    {
        UpdateVisualizationState();
        UpdateCirclePositionsIfNeeded();
    }

    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.enabled = alwaysShowDefault;
        
        if (alwaysShowDefault)
        {
            SetLineColor(neutralColor);
        }
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

    void UpdateVisualizationState()
    {
        bool isInteracting = Input.GetMouseButton(0) || Input.GetMouseButton(1);
        
        if (alwaysShowDefault)
        {
            lineRenderer.enabled = true;
            if (isInteracting)
            {
                SetLineColor(Input.GetMouseButton(0) ? attractColor : repelColor);
            }
            else
            {
                SetLineColor(neutralColor);
            }
        }
        else
        {
            lineRenderer.enabled = isInteracting;
            if (isInteracting)
            {
                SetLineColor(Input.GetMouseButton(0) ? attractColor : repelColor);
            }
        }
    }

    void UpdateCirclePositionsIfNeeded()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float currentRadius = simulation.GetInteractionRadius();

        if (currentRadius != lastRadius || mousePosition != lastMousePosition)
        {
            UpdateCirclePositions(mousePosition, currentRadius);
            lastRadius = currentRadius;
            lastMousePosition = mousePosition;
        }
    }

    void UpdateCirclePositions(Vector2 center, float radius)
    {
        for(int i = 0; i < segments; i++)
        {
            lineRenderer.SetPosition(i, center + (Vector2)(circlePositions[i] * radius));
        }
    }

    void SetLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}