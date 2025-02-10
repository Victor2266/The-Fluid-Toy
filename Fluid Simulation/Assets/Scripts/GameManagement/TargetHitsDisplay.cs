using UnityEngine;
using TMPro;

/// <summary>
/// Updates text display using level 2 values, can be updated to work on any level.
/// </summary>
public class targetHitsDisplay : MonoBehaviour
{
    [Header("References")]
    public LevelManager manager;
    public TextMeshProUGUI displayText;
    
    [Header("Display Settings")]
    public string prefix = "";
    public string suffix = "/";
    
    [Header("Color Settings")]
    public Color startColor = Color.black;
    public Color endColor = Color.white;
    public Color thresholdColor = Color.red;

    void Start()
    {
        
        // Auto-find references if not set
        if (manager == null)
        {
            manager = FindObjectOfType<LevelManager>();
            if (manager == null)
            {
                Debug.LogError("No levelManager found in scene!");
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
        if (manager == null || displayText == null) return;
        

        if(manager is Level2Manager manager2){
            // get target hits
            int targetHits = manager2.targetHits;
            int maxHits = manager2.totalTargetHitsNeeded;

            // get current percentage
            int totalPercentage = Mathf.RoundToInt((float)targetHits / maxHits * 100f);
            
            // Update text
            displayText.text = $"{prefix}{totalPercentage}{suffix}";
            if(targetHits < manager2.totalTargetHitsNeeded){
                // Calculate color based on percentage
                float colorLerpValue = (float) targetHits / manager2.totalTargetHitsNeeded;
                Color currentColor = Color.Lerp(startColor, endColor, colorLerpValue);
                displayText.color = currentColor;
            }else{
                // When threshold is reached, show text in threshold color
                displayText.color = thresholdColor;
            }
        }
        
    }


}