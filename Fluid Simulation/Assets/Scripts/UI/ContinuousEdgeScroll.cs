using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ContinuousEdgeScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Reference to the scroll rect component")]
    public ScrollRect scrollRect;

    [Tooltip("Percentage of screen width for edge detection zone")]
    [Range(0.01f, 0.15f)]
    public float edgeDetectionPercentage = 0.07f;

    [Tooltip("Scrolling speed when near screen edges")]
    [Range(0.1f, 128f)]
    public float scrollSpeed = 3f;

    [Tooltip("Right edge offset as percentage of screen width")]
    [Range(0f, 0.1f)]
    public float rightEdgeOffsetPercentage = 0.1f;

    // Reference resolution values
    private const float REFERENCE_WIDTH = 1280f; // 720p width
    private const float REFERENCE_HEIGHT = 720f;

    private RectTransform viewportRectTransform;
    private RectTransform contentRectTransform;
    private float edgeDetectionWidth;
    private float rightEdgeOffset;

    private bool isOnMobile = false;

    private void Start()
    {
        // Validate scroll rect reference
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (scrollRect == null)
        {
            Debug.LogError("No ScrollRect component found. Please assign one in the inspector.");
            enabled = false;
            return;
        }

        isOnMobile = Application.isMobilePlatform;

        if (isOnMobile)
        {
            // Ensure horizontal scrolling is enabled
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
        }
        else
        {
            // Ensure horizontal scrolling is disabled
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
        }

        // Cache rect transforms for performance
        viewportRectTransform = scrollRect.viewport;
        contentRectTransform = scrollRect.content;
        
        // Calculate scaled values based on current resolution
        UpdateScaledValues();
    }
    
    private void UpdateScaledValues()
    {
        // Calculate edge detection width and offset based on current screen width
        edgeDetectionWidth = Screen.width * edgeDetectionPercentage;
        rightEdgeOffset = Screen.width * rightEdgeOffsetPercentage;
    }

    private void Update()
    {
        if (isOnMobile)
        {
            return;
        }
        
        // Update scaled values if resolution changes
        UpdateScaledValues();
        
        // Only scroll if content is wider than viewport
        if (contentRectTransform.rect.width <= viewportRectTransform.rect.width)
            return;

        Vector2 mousePosition = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Check if mouse is within screen height
        if (mousePosition.y > 0 && mousePosition.y < screenHeight)
        {

            // Calculate proportional scroll based on mouse x position
            float normalizedMouseX = (mousePosition.x - edgeDetectionWidth) / (screenWidth - edgeDetectionWidth - rightEdgeOffset);
            // Calculate scroll value based on mouse position
            float scrollValue = Mathf.Clamp01(normalizedMouseX);


            // Smoothly update scroll position
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition,
                scrollValue,
                scrollSpeed * Time.deltaTime
            );
        }
    }

    // Optional: Add this to make adjustments visible in the inspector
    private void OnValidate()
    {
        // For visualization purposes in editor
        edgeDetectionWidth = REFERENCE_WIDTH * edgeDetectionPercentage;
        rightEdgeOffset = REFERENCE_WIDTH * rightEdgeOffsetPercentage;
    }
}