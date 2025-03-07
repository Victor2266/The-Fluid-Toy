using UnityEngine;
using TMPro;

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
    
    [Header("Strength Text")]
    [SerializeField] private GameObject strengthTextObject;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Vector2 textOffset = new Vector2(0, 30f);
    
    private LineRenderer lineRenderer;
    private Vector3[] circlePositions;
    private float lastRadius;
    float currentRadius;

    private float lastStrength;
    private float currentStrength;
    private Vector2 lastMousePosition;

    private float fadeProgress = 0f;
    private float fadeOutTime = 0.15f;
    
    // Text component references
    private TextMeshProUGUI strengthText;

    void Start()
    {
        simulationGameObject = GameObject.FindGameObjectWithTag("Simulation");
        simulation = simulationGameObject.GetComponent<IFluidSimulation>();

        fadeProgress = fadeOutTime;
        currentStrength = simulation.getBrushStrengthPercent();
        lastStrength = currentStrength;

        InitializeLineRenderer();
        PrecalculateCircle();
        SetupStrengthText();
    }

    void Update()
    {
        if (Time.timeScale != 0){
            currentRadius = simulation.getBrushStrengthPercent() * simulation.getInteractionRadius();
            currentStrength = simulation.getBrushStrengthPercent();
            UpdateVisualizationState();
            UpdateCirclePositionsIfNeeded();
            UpdateStrengthText();
        }
        else {
            lineRenderer.enabled = false;
            strengthTextObject.SetActive(false);
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

    void SetupStrengthText()
    {
        // Get the TextMeshPro component if we didn't create it above
        if (strengthText == null)
        {
            strengthText = strengthTextObject.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Hide initially if not always shown
        strengthTextObject.SetActive(alwaysShow);
    }

    void UpdateVisualizationState()
    {
        if (currentStrength != lastStrength)
        {
            lastStrength = currentStrength;
            fadeProgress = 0f;
        } else
        {
            if (fadeProgress < fadeOutTime)
                fadeProgress += Time.deltaTime;
        }
        
        bool shouldShow = alwaysShow || fadeProgress < fadeOutTime;
        
        lineRenderer.enabled = shouldShow;
        Cursor.visible = !shouldShow;

        strengthTextObject.SetActive(shouldShow);

        SetLineColor(circleColor);
    }

    void UpdateCirclePositionsIfNeeded()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Calculate text offset
        float mouseY = mousePosition.y;
        float halfScreenHeight = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).y;
        textOffset = new Vector2(0, (mouseY > halfScreenHeight ? -1 : 1) * (simulation.getInteractionRadius() + 0.85f) );
        if (simulation.getInteractionRadius() > 4f)
        {
            textOffset = Vector2.zero;
        }
        

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

    void UpdateStrengthText()
    {
        // Update text content - show percentage
        strengthText.text = $"{Mathf.Round(currentStrength * 100)}%";
        
        // Update color with same fade as circle
        Color fadeTextColor = textColor;
        fadeTextColor.a = Mathf.Lerp(1f, 0f, fadeProgress / fadeOutTime);
        strengthText.color = fadeTextColor;

        // Position text above the circle
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        strengthTextObject.transform.position = new Vector3(mousePosition.x + textOffset.x, mousePosition.y + textOffset.y, 0);
    }

    void SetLineColor(Color color)
    {
        Color fadeColor = color;
        fadeColor.a = Mathf.Lerp(1f, 0f, fadeProgress / fadeOutTime);
        lineRenderer.startColor = fadeColor;
        lineRenderer.endColor = fadeColor;
    }
}