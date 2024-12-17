using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(ScrollRect))]
public class ScrollReset : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float resetDuration = 0.5f;
    [SerializeField] private Ease easeType = Ease.InOutQuad;
    
    private ScrollRect scrollRect;
    private RectTransform contentRectTransform;
    private Tween currentTween;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentRectTransform = scrollRect.content;
    }

    private void OnEnable()
    {
        // Kill any existing tween to prevent conflicts
        currentTween?.Kill();
        
        // Get the current normalized position
        Vector2 currentPos = scrollRect.normalizedPosition;
        
        // Create a new tween to smoothly move to the top
        // For vertical scroll: (1,0) is bottom, (0,0) is top
        // For horizontal scroll: (0,0) is left, (1,0) is right
        currentTween = DOTween.To(
            () => currentPos,
            (Vector2 pos) => {
                scrollRect.normalizedPosition = pos;
            },
            new Vector2(0, 1), // Target position (top for vertical scroll)
            resetDuration
        )
        .SetEase(easeType)
        .SetUpdate(true); // Make it update even if time scale is 0
    }

    private void OnDisable()
    {
        // Clean up the tween when the object is disabled
        currentTween?.Kill();
    }
}