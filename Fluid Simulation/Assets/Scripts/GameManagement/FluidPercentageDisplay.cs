using UnityEngine;
using TMPro;

public class FluidPercentageDisplay : MonoBehaviour
{
    [Header("References")]
    public FluidDetector fluidDetector;
    public TextMeshProUGUI displayText;
    
    [Header("Display Settings")]
    public string prefix = "Container: ";
    public string suffix = "%";
    public string completionString = "STOP";
    public int decimalPlaces = 1;
    public float smoothingSpeed = 5f; // Higher value = faster smoothing
    
    [Header("Color Settings")]
    public Color startColor = Color.black;
    public Color endColor = Color.white;
    public Color thresholdColor = Color.red;
    
    private float currentDisplayValue = 0f;
    
    void Start()
    {
        // Auto-find references if not set
        if (fluidDetector == null)
        {
            fluidDetector = FindFirstObjectByType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in scene!");
                enabled = false;
                return;
            }
        }
        
        if (displayText == null)
        {
            displayText = GetComponent<TextMeshProUGUI>();
            if (displayText == null)
            {
                Debug.LogError("No TextMeshProUGUI component found!");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (fluidDetector == null || displayText == null) return;
        
        // Calculate percentage based on current density and threshold
        float targetPercentage = (fluidDetector.currentDensity / fluidDetector.densityThreshold) * 100f;
        targetPercentage = Mathf.Min(targetPercentage, 100f); // Cap at 100%
        
        // Smooth the display value
        currentDisplayValue = Mathf.Lerp(currentDisplayValue, targetPercentage, Time.deltaTime * smoothingSpeed);
        
        // Format the text with the specified decimal places
        string percentageText = currentDisplayValue.ToString($"F{decimalPlaces}");
        
        // Update text and color based on threshold
        if (!fluidDetector.isFluidPresent)
        {
            // Update text
            displayText.text = $"{prefix}{percentageText}{suffix}";
            
            // Calculate color based on percentage
            float colorLerpValue = currentDisplayValue / 100f;
            Color currentColor = Color.Lerp(startColor, endColor, colorLerpValue);
            displayText.color = currentColor;
        }
        else
        {
            // When threshold is reached, show text in threshold color
            displayText.text = completionString;
            displayText.color = thresholdColor;
        }
    }
}