using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ParticleCountUIDisplay : FluidCounter
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI UIText;
    [SerializeField] private RectTransform panelRectTransform;
    
    [Header("Settings")]
    [SerializeField] private Vector2 padding = new Vector2(10f, 5f);
    [SerializeField] private string displayFormat = "Particle Count: {0}";
    private float timeLeft;

    protected override void Start()
    {
        base.Start();
        
        if (UIText == null)
        {
            Debug.LogError("ParticleCountUIDisplay: TextMeshProUGUI reference not set!");
            enabled = false;
            return;
        }
        
        if (panelRectTransform == null)
        {
            Debug.LogError("ParticleCountUIDisplay: Panel RectTransform reference not set!");
            enabled = false;
            return;
        }
        
        // Initialize the update interval
        timeLeft = checkInterval;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // FPS calculation
        timeLeft -= Time.deltaTime;
        
        // Update the FPS display at specified intervals
        if (timeLeft <= 0.0)
        {
            timeLeft = checkInterval;
            
            // Update the text
            UIText.text = string.Format(displayFormat, TotalActiveParticles);
            
            // Resize the panel to fit the text
            ResizePanelToFitText();
        }
    }
    
    private void ResizePanelToFitText()
    {
        if (UIText == null || panelRectTransform == null) return;
        
        // Force text to update its layout
        UIText.ForceMeshUpdate();
        
        // Get the preferred width and height of the text
        Vector2 textSize = UIText.GetPreferredValues();
        
        // Apply padding to create some space around the text
        Vector2 newSize = textSize + padding * 2;
        
        // Set the panel size to match the text size plus padding
        panelRectTransform.sizeDelta = newSize;
    }
}
