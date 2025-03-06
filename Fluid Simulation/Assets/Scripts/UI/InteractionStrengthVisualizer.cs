using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InteractionStrengthVisualizer : MonoBehaviour
{
    [Header("Dependencies")]
    private IFluidSimulation simulation;
    private GameObject simulationGameObject;
    
    [Header("Visual Settings")]
    [SerializeField] private Color circleColor = Color.blue;
    [SerializeField] private bool alwaysShow = true;
    [SerializeField] [Range(8, 64)] private int segments = 32;
    
    private LineRenderer lineRenderer;
    private Vector3[] circlePositions;
    private float lastRadius;
    float currentRadius;

    private float lastStrength;
    private float currentStrength;
    private Vector2 lastMousePosition;

    private float fadeProgress = 0f;
    private float fadeOutTime = 0.15f;

    void Start()
    {
        simulationGameObject = GameObject.FindGameObjectWithTag("Simulation");
        simulation = simulationGameObject.GetComponent<IFluidSimulation>();
        InitializeLineRenderer();
        PrecalculateCircle();
    }

    void Update()
    {
        if (Time.timeScale != 0){
            currentRadius = simulation.getBrushStrengthPercent() * simulation.getInteractionRadius();
            currentStrength = simulation.getBrushStrengthPercent();
            UpdateVisualizationState();
            UpdateCirclePositionsIfNeeded();
        }
        else {
            lineRenderer.enabled = false;
        }

    }

    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.enabled = alwaysShow;
        
        if (alwaysShow)
        {
            SetLineColor(circleColor);
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
        if (currentStrength != lastStrength)
        {
            lastStrength = currentStrength;
            fadeProgress = 0f;
        } else
        {
            fadeProgress += Time.deltaTime;
        }
        
        if (alwaysShow)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = fadeProgress < fadeOutTime;
        }

        SetLineColor(circleColor);
    }

    void UpdateCirclePositionsIfNeeded()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        

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
        Color fadeColor = color;
        fadeColor.a = Mathf.Lerp(1f, 0f, fadeProgress / fadeOutTime);
        lineRenderer.startColor = fadeColor;
        lineRenderer.endColor = fadeColor;
    }
}
