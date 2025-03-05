using UnityEngine;
using UnityEngine.UI;

public class ContinuousEdgeScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Reference to the scroll rect component")]
    public ScrollRect scrollRect;

    [Tooltip("Width of the screen edge detection zone")]
    [Range(10f, 100f)]
    public float edgeDetectionWidth = 50f;

    [Tooltip("Scrolling speed when near screen edges")]
    [Range(0.1f, 128f)]
    public float scrollSpeed = 3f;

    [Tooltip("Offset from the right edge (in pixels)")]
    public float rightEdgeOffset = 40f;

    private RectTransform viewportRectTransform;
    private RectTransform contentRectTransform;

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

        // Ensure horizontal scrolling is enabled
        scrollRect.horizontal = false;
        scrollRect.vertical = false;

        // Cache rect transforms for performance
        viewportRectTransform = scrollRect.viewport;
        contentRectTransform = scrollRect.content;
    }

    private void Update()
    {
        // Only scroll if content is wider than viewport
        if (contentRectTransform.rect.width <= viewportRectTransform.rect.width)
            return;

        Vector2 mousePosition = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Check if mouse is within screen height
        if (mousePosition.y > 0 && mousePosition.y < screenHeight)
        {
            // Calculate scroll value based on mouse position
            float scrollValue = 0f;

            // Left edge scrolling
            if (mousePosition.x <= edgeDetectionWidth)
            {
                // Map edge detection width to 0 scroll position
                scrollValue = 0f;
            }
            // Right edge scrolling (accounting for offset)
            else if (mousePosition.x >= (screenWidth - edgeDetectionWidth - rightEdgeOffset))
            {
                // Map edge detection width to 1 (full) scroll position
                scrollValue = 1f;
            }
            // Intermediate scrolling
            else
            {
                // Calculate proportional scroll based on mouse x position
                float normalizedMouseX = mousePosition.x / screenWidth;
                scrollValue = Mathf.Clamp01(normalizedMouseX);
            }

            // Smoothly update scroll position
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition, 
                scrollValue, 
                scrollSpeed * Time.deltaTime
            );
        }
    }
}