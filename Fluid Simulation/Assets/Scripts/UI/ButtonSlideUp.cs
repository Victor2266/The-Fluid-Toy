using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonSlideUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup; // For fading the entire button

    [Header("Animation Settings")]
    [SerializeField] private float startOffsetY = -50f;
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private Ease easeType = Ease.OutQuint;
    [SerializeField] private bool animateOnEnable = true;

    private Vector3 originalPosition;
    private bool hasAnimated = false;

    private void Awake()
    {
        // Get references if not set
        if (button == null)
            button = GetComponent<Button>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Store original position
        originalPosition = button.transform.localPosition;

        // Set initial state
        ResetPosition();
    }

    private void OnEnable()
    {
        if (animateOnEnable)
        {
            // Ensure we start from the reset position when enabled
            ResetPosition();
            // Trigger animation
            AnimateButton();
        }
    }

    private void ResetPosition()
    {
        // Reset button position and alpha
        Vector3 startPos = originalPosition;
        startPos.y += startOffsetY;
        button.transform.localPosition = startPos;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        hasAnimated = false;
    }

    public void AnimateButton()
    {
        if (hasAnimated) return;

        // Kill any existing tweens to prevent conflicts
        DOTween.Kill(button.transform);
        if (canvasGroup != null)
            DOTween.Kill(canvasGroup);

        // Animate position
        button.transform.DOLocalMove(originalPosition, animationDuration)
            .SetEase(easeType)
            .SetUpdate(true); // Make it time scale independent

        // Animate opacity
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(1f, animationDuration * 0.8f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Make it time scale independent
        }

        hasAnimated = true;
    }

    // Optional: Call this if you need to manually reset and replay the animation
    public void ReplayAnimation()
    {
        ResetPosition();
        AnimateButton();
    }

    void OnDestroy()
    {
        // Clean up tweens when the object is destroyed
        DOTween.Kill(button.transform);
        if (canvasGroup != null)
            DOTween.Kill(canvasGroup);
    }
}