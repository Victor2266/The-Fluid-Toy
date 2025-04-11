using UnityEngine;
using UnityEngine.SceneManagement;

public class WebGPUDetector : MonoBehaviour
{
    #if UNITY_WEBGL && !UNITY_EDITOR
    [SerializeField] private string webGPUFallbackSceneName = "WebGPUInstructions";
    [SerializeField] private bool debugMode = false;
    
    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("WebGPUDetector started");
            Debug.Log("Current graphics device: " + SystemInfo.graphicsDeviceType);
        }
        
        // Check if we're using WebGPU
        bool usingWebGPU = IsUsingWebGPU();
        
        if (debugMode)
        {
            Debug.Log("Using WebGPU: " + usingWebGPU);
        }
        
        // If we're not using WebGPU, switch to instructions scene
        if (!usingWebGPU)
        {
            if (debugMode)
            {
                Debug.Log("WebGPU not detected, loading instructions scene: " + webGPUFallbackSceneName);
            }
            
            // Make sure the scene is included in build settings
            if (SceneUtility.GetBuildIndexByScenePath(webGPUFallbackSceneName) != -1)
            {
                SceneManager.LoadSceneAsync(webGPUFallbackSceneName);
            }
            else
            {
                Debug.LogError("Scene not found in build settings: " + webGPUFallbackSceneName);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private bool IsUsingWebGPU()
    {
        // Check the graphics device type directly
        // WebGPU will be reported as "WebGPU" in SystemInfo.graphicsDeviceType
        return SystemInfo.graphicsDeviceType.ToString() == "WebGPU";
    }
    #endif
}