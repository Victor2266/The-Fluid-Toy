using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ThermalBoxContextMenu : EditableObject
{
    // Thermal properties
    private ThermalBoxInitData thermalData;

    [Tooltip("Gradients representing the color transitions.")]
    public Gradient hotColorGradient;
    public Gradient coldColorGradient;

    protected override void Awake()
    {
        base.Awake();
        thermalData = GetComponent<ThermalBoxInitData>();
    }
    
    protected override void SetupMenuControls()
    {
        // Call base class implementation first
        base.SetupMenuControls();
        
        // Add thermal-specific controls
        Transform conductivityInput = content.transform.Find("ConductivityInput");
        Transform temperatureInput = content.transform.Find("TemperatureInput");
        
        if (conductivityInput != null && temperatureInput != null)
        {
            // Set initial values
            TMP_InputField conductivityField = conductivityInput.GetComponentInChildren<TMP_InputField>();
            TMP_InputField temperatureField = temperatureInput.GetComponentInChildren<TMP_InputField>();
            
            conductivityField.text = thermalData.conductivity.ToString("F2");
            temperatureField.text = thermalData.temperature.ToString("F2");
            
            // Add listeners
            conductivityField.onEndEdit.AddListener((value) => {
                if (float.TryParse(value, out float newConductivity))
                {
                    thermalData.conductivity = newConductivity;
                    RescanForObstacles();
                }
            });
            
            temperatureField.onEndEdit.AddListener((value) => {
                if (float.TryParse(value, out float newTemperature))
                {
                    thermalData.temperature = newTemperature;
                    if (newTemperature > 22f){
                        GetComponent<LineRendererColorTransition>().colorGradient = hotColorGradient;
                    } else {
                        GetComponent<LineRendererColorTransition>().colorGradient = coldColorGradient;
                    }
                    RescanForObstacles();
                }
            });
        }
    }
}