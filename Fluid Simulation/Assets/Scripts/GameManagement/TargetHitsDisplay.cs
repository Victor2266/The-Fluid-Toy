using UnityEngine;
using TMPro;

public class targetHitsDisplay : MonoBehaviour
{
    [Header("References")]
    public Level2Manager manager;
    public TextMeshProUGUI displayText;
    
    [Header("Display Settings")]
    public string prefix = "";
    public string suffix = "/";
    
    [Header("Color Settings")]
    public Color startColor = Color.black;
    public Color endColor = Color.white;
    public Color thresholdColor = Color.red;
    
    private int totalTargetHitsNeeded;

    void Start()
    {
        
        // Auto-find references if not set
        if (manager == null)
        {
            manager = FindObjectOfType<Level2Manager>();
            if (manager == null)
            {
                Debug.LogError("No level2manager found in scene!");
                enabled = false;
                return;
            }

            
        }
        
        totalTargetHitsNeeded = manager.totalTargetHitsNeeded;

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
        if (manager == null || displayText == null) return;
        
        // get target hits
        int targetHits = manager.targetHits;
        
        // Update text
        displayText.text = $"{prefix}{targetHits.ToString()}{suffix}{totalTargetHitsNeeded.ToString()}";
        if(targetHits < totalTargetHitsNeeded){
            // Calculate color based on percentage
            float colorLerpValue = (float) targetHits / totalTargetHitsNeeded;
            Color currentColor = Color.Lerp(startColor, endColor, colorLerpValue);
            displayText.color = currentColor;
        }else{
            // When threshold is reached, show text in threshold color
            displayText.color = thresholdColor;
        }
    }


}