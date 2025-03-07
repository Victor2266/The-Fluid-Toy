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
    [SerializeField] private GameObject strengthTextPrefab;
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
    private GameObject strengthTextObject;
    private TextMeshProUGUI strengthText;

    void Start()
    {
        simulationGameObject = GameObject.FindGameObjectWithTag("Simulation");
        simulation = simulationGameObject.GetComponent<IFluidSimulation>();
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
        // If a prefab is provided, instantiate it
        if (strengthTextPrefab != null)
        {
            strengthTextObject = Instantiate(strengthTextPrefab, transform);
        }
        // Otherwise create a new TextMeshPro object
        else
        {
            strengthTextObject = new GameObject("StrengthText");
            strengthTextObject.transform.SetParent(transform);
            
            // Add TextMeshPro component
            strengthText = strengthTextObject.AddComponent<TextMeshProUGUI>();
            
            // Setup text properties
            strengthText.fontSize = 1;
            strengthText.alignment = TextAlignmentOptions.Center;
            strengthText.color = textColor;
            
            // Make sure it renders on top
            Canvas canvas = strengthTextObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            
            // Add a RectTransform and set its properties
            RectTransform rectTransform = strengthText.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(3, 2);
        }
        
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
        if (mouseY > halfScreenHeight)
        {
            textOffset = new Vector2(0, -(simulation.getInteractionRadius() + 1f));
        }
        else
        {
            textOffset = new Vector2(0, simulation.getInteractionRadius() + 1f);
        }
        if (simulation.getInteractionRadius() > 6.5f){
            textOffset = new Vector2(0, 0);
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