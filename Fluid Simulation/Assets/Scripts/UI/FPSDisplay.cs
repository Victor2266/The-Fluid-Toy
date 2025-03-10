using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private RectTransform panelRectTransform;
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private Vector2 padding = new Vector2(10f, 5f);
    [SerializeField] [Range(0, 2)] private int fractionDigits = 1;
    [SerializeField] private string displayFormat = "FPS: {0}";
    
    // FPS calculation variables
    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    private float fps = 0f;
    
    private void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("FPSDisplay: TextMeshProUGUI reference not set!");
            enabled = false;
            return;
        }
        
        if (panelRectTransform == null)
        {
            Debug.LogError("FPSDisplay: Panel RectTransform reference not set!");
            enabled = false;
            return;
        }
        
        // Initialize the update interval
        timeLeft = updateInterval;
    }
    
    private void Update()
    {
        // FPS calculation
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;
        
        // Update the FPS display at specified intervals
        if (timeLeft <= 0.0)
        {
            fps = accum / frames;
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
            
            // Update the text
            fpsText.text = string.Format(displayFormat, fps.ToString("F" + fractionDigits));
            
            // Resize the panel to fit the text
            ResizePanelToFitText();
        }
    }
    
    private void ResizePanelToFitText()
    {
        if (fpsText == null || panelRectTransform == null) return;
        
        // Force text to update its layout
        fpsText.ForceMeshUpdate();
        
        // Get the preferred width and height of the text
        Vector2 textSize = fpsText.GetPreferredValues();
        
        // Apply padding to create some space around the text
        Vector2 newSize = textSize + padding * 2;
        
        // Set the panel size to match the text size plus padding
        panelRectTransform.sizeDelta = newSize;
        
        // If needed, you can also reposition the text within the panel
        RectTransform textRectTransform = fpsText.GetComponent<RectTransform>();
        if (textRectTransform != null)
        {
            textRectTransform.anchoredPosition = new Vector2(padding.x, -padding.y);
        }
    }
    
    // This can be called from the Unity Inspector to test the sizing
    [ContextMenu("Test Resize")]
    public void TestResize()
    {
        if (Application.isPlaying) return;
        
        fpsText.text = string.Format(displayFormat, 60.ToString("F" + fractionDigits));
        ResizePanelToFitText();
    }
}