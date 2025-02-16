using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Performes bar fill like loading bar, can be updated to work in any level.
/// </summary>
public class FillBar : MonoBehaviour
{
    public LevelManager manager;

    public Image fillImage;

    public float fillSpeed;

    public bool isOverheatBar;

    void Start()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<LevelManager>();
            if (manager == null)
            {
                Debug.LogError("No levelManager found in scene!");
                enabled = false;
                return;
            }

            
        }

        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
            if (fillImage == null)
            {
                Debug.LogError("No image found in Object!");
                enabled = false;
                return;
            }

            
        }
    }
 
    // Update is called once per frame
    void Update()
    {
        if(manager is Level2Manager manager2){
            if(!isOverheatBar){
                float targetFillAmount = Mathf.Clamp01((float) manager2.targetHits / (float) manager2.totalTargetHitsNeeded);
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, fillSpeed * Time.deltaTime);
            }
            else
            {
                float targetFillAmount = Mathf.Clamp01((float) manager2.currentHeatLevel / (float) manager2.maxHeatLevel);
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, fillSpeed * Time.deltaTime);
            }

        } 
        
    }
}
