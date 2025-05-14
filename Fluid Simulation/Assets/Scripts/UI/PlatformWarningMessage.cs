using UnityEngine;
using TMPro;

/// <summary>
/// Controls the text of a TextMeshPro component based on build platform.
/// Attach this to a GameObject with a TextMeshPro component.
/// </summary>
public class PlatformWarningMessage : MonoBehaviour
{
    public GameObject backgroundImage;
    public bool usePlatformOverOS = true; // Uses the build platform check over the operating system check at runtime

    [Header("Platform-Specific Messages")]
    [TextArea(3, 5)]
    [Tooltip("Warning message for WebGL builds")]
    public string webGLMessage = "<color=red>WARNING:</color> On the web player you cannot change the resolution.";
    
    [TextArea(3, 5)]
    [Tooltip("Warning message for Android builds")]
    public string androidMessage = "<color=red>WARNING:</color> This game requires internet connection.";
    
    [TextArea(3, 5)]
    [Tooltip("Warning message for iOS builds")]
    public string iOSMessage = "<color=red>WARNING:</color> Game progress is not saved between sessions.";
    
    [TextArea(3, 5)]
    [Tooltip("Warning message for Windows/Mac/Linux builds (leave empty to hide)")]
    public string desktopMessage = "";
    
    [TextArea(3, 5)]
    [Tooltip("Warning message for console builds")]
    public string consoleMessage = "<color=yellow>NOTE:</color> Controller required to play.";
    
    [TextArea(3, 5)]
    [Tooltip("Fallback message for other platforms")]
    public string defaultMessage = "";

    [Header("OS-Specific Messages")]
    [TextArea(3, 5)]
    [Tooltip("Warning message for iPhone/iPad OS")]
    public string iOS_OSMessage = "<color=red>WARNING:</color> Game progress is not saved between sessions.";
    
    [Header("Settings")]
    [Tooltip("If true, warning will be displayed in the Unity Editor")]
    public bool showInEditor = true;
    
    [Tooltip("Message to show in the Unity Editor")]
    [TextArea(3, 5)]
    public string editorMessage = "<color=blue>DEVELOPMENT BUILD</color>";
    
    private TextMeshProUGUI textComponent;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError("WarningMessageController requires a TextMeshProUGUI component!");
            enabled = false;
            return;
        }
        
        if (usePlatformOverOS)
        {
            SetAppropriateMessage();
        }
        else
        {
            SetOSAppropriateMessage();
        }
    }
    
    void SetAppropriateMessage()
    {
        string message = "";
        
        #if UNITY_EDITOR
        if (showInEditor)
        {
            message = editorMessage;
        }
        #elif UNITY_WEBGL
        message = webGLMessage;
        #elif UNITY_ANDROID
        message = androidMessage;
        #elif UNITY_IOS
        message = iOSMessage;
        #elif UNITY_STANDALONE
        message = desktopMessage;
        #elif UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT || UNITY_SWITCH
        message = consoleMessage;
        #else
        message = defaultMessage;
        #endif
        
        textComponent.text = message;
        
        // Hide the GameObject if no message is set
        if (string.IsNullOrEmpty(message))
        {
            gameObject.SetActive(false);
            if (backgroundImage != null) backgroundImage.SetActive(false);
        }
    }

    void SetOSAppropriateMessage()
    {
        if (SystemInfo.operatingSystem.ToLower().Contains("iphone os") || SystemInfo.operatingSystem.ToLower().Contains("ipad os") || SystemInfo.operatingSystem.ToLower().Contains("mac"))
        {
            textComponent.text = iOS_OSMessage;
            return;
        }
        // Call the platform-based message setting if not using iOS
        SetAppropriateMessage();
    }

}