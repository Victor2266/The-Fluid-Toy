using UnityEngine;
using TMPro;

public class TemperatureDisplay : MonoBehaviour
{
    [Header("References")]
    public ThermalSensor thermalSensor;
    public TextMeshProUGUI displayText;
    
    [Header("Display Settings")]
    public string prefix = "";
    public string suffix = "";
    public string thresholdMetString = "HOT";
    public int decimalPlaces = 0;
    public float smoothingSpeed = 5f; // Higher value = faster smoothing
    
    [Header("Color Settings")]
    public Color startColor = Color.blue;
    public Color endColor = Color.red;
    public Color thresholdColor = Color.green;
    
    [Header("Debug")]
    public bool debugMode = true;
    
    private float currentDisplayValue = 0f;
    
    void Start()
    {
        // Auto-find references if not set
        if (thermalSensor == null)
        {
            thermalSensor = FindFirstObjectByType<ThermalSensor>();
            if (thermalSensor == null)
            {
                Debug.LogError("No ThermalSensor found in scene!");
                enabled = false;
                return;
            }
        }
        
        if (displayText == null)
        {
            displayText = GetComponent<TextMeshProUGUI>();
            if (displayText == null)
            {
                // Try to find in children
                displayText = GetComponentInChildren<TextMeshProUGUI>();
                if (displayText == null)
                {
                    Debug.LogError("No TextMeshProUGUI component found!");
                    enabled = false;
                    return;
                }
            }
        }
        
        // Force initial update
        UpdateDisplay();
    }

    void Update()
    {
        if (thermalSensor == null || displayText == null) return;
        
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        // Get the current temperature from the sensor
        float targetTemperature = thermalSensor.currentTemperature;
        
        // Smooth the display value
        currentDisplayValue = Mathf.Lerp(currentDisplayValue, targetTemperature, Time.deltaTime * smoothingSpeed);
        
        // Format the text with the specified decimal places (ensure it's less than 6 characters)
        string temperatureText = Mathf.RoundToInt(currentDisplayValue).ToString();
        
        if (debugMode)
        {
            Debug.Log($"Raw Temperature: {targetTemperature}, Threshold: {thermalSensor.temperatureThreshold}, Display: {currentDisplayValue}, Text: {temperatureText}");
        }
        
        // Update text and color based on threshold
        if (!thermalSensor.metThreshold)
        {
            // Update text
            displayText.text = $"{prefix}{temperatureText}{suffix}";
            
            // Calculate color based on temperature
            // Map the temperature to a range between 0 and 1 for color lerping
            float minTemp = 0f;  // Adjust this based on your expected temperature range
            float maxTemp = thermalSensor.temperatureThreshold * 1.5f;  // Adjust this based on your expected temperature range
            float colorLerpValue = Mathf.InverseLerp(minTemp, maxTemp, currentDisplayValue);
            Color currentColor = Color.Lerp(startColor, endColor, colorLerpValue);
            displayText.color = currentColor;
        }
        else
        {
            // When threshold is reached, show text in threshold color
            displayText.text = thresholdMetString;
            displayText.color = thresholdColor;
        }
    }
}
