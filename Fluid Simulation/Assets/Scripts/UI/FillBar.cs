using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    public LevelManager manager;

    public Image fillImage;

    public float fillSpeed;
    // Start is called before the first frame update
    void Start()
    {
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
            float targetFillAmount = Mathf.Clamp01((float) manager2.targetHits / (float) manager2.totalTargetHitsNeeded);
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, fillSpeed * Time.deltaTime);
        }
        
    }
}
