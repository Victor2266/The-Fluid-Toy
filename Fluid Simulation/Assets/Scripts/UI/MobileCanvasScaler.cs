using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class MobileCanvasScaler : MonoBehaviour
{
    [Tooltip("The reference resolution height to use on mobile platforms")]
    public float mobileReferenceHeight = 675f;
    
    [Tooltip("The reference resolution height to use on non-mobile platforms")]
    public float desktopReferenceHeight = 720f;
    
    private CanvasScaler canvasScaler;
    
    private void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        
        if (canvasScaler == null)
        {
            Debug.LogError("CanvasScaler component not found on this GameObject!");
            return;
        }
        
        // Check if the current platform is a mobile platform
        if (IsMobilePlatform())
        {
            // Keep the original width but change the height to the mobile reference height
            Vector2 referenceResolution = canvasScaler.referenceResolution;
            referenceResolution.y = mobileReferenceHeight;
            canvasScaler.referenceResolution = referenceResolution;
            
            Debug.Log("Mobile platform detected. Canvas reference height set to: " + mobileReferenceHeight);
        }
        else
        {
            // Keep the original width but change the height to the desktop reference height
            Vector2 referenceResolution = canvasScaler.referenceResolution;
            referenceResolution.y = desktopReferenceHeight;
            canvasScaler.referenceResolution = referenceResolution;
            
            Debug.Log("Desktop platform detected. Canvas reference height set to: " + desktopReferenceHeight);
        }
    }
    
    private bool IsMobilePlatform()
    {
        // Check if the current platform is a mobile platform
        #if UNITY_ANDROID || UNITY_IOS
            return true;
        #else
            // You can add additional checks for device type if needed
            return Application.isMobilePlatform;
        #endif
    }
}