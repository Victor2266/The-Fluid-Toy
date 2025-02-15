using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ButtonSlideUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float startOffsetY = -50f;
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private Ease easeType = Ease.OutQuint;
    [SerializeField] private bool animateOnEnable = true;
    [SerializeField] private float delayAfterSceneLoad = 0.1f; // Small delay after scene load

    private Vector3 originalPosition;
    private bool hasAnimated = false;
    private bool isSceneLoaded = false;

    private void Awake()
    {
        // Get references if not set
        if (button == null)
            button = GetComponent<Button>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Store original position
        originalPosition = button.transform.localPosition;

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneLoaded = true;
        if (gameObject.activeSelf)
        {
            ResetPosition();
            // Add a small delay to ensure everything is properly initialized
            DOVirtual.DelayedCall(delayAfterSceneLoad, AnimateButton)
                .SetUpdate(true);
        }
    }

    private void OnEnable()
    {
        // Reset and start animation whenever the object is enabled
        // Only animate if the scene is already loaded
        // THIS WILL NOT RUN IMMEDIATELY WHEN THE SCENE IS LOADED BECAUSE isSceneLoaded IS FALSE
        // THIS IS FOR ANIMATIONS AFTER DISABLING AND RE-ENABLING THE BUTTON
        if (animateOnEnable && isSceneLoaded)
        {
            ResetPosition();

            DOVirtual.DelayedCall(delayAfterSceneLoad, AnimateButton)
                .SetUpdate(true);
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
            .SetUpdate(true);

        // Animate opacity
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(1f, animationDuration * 0.8f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        hasAnimated = true;
    }

    public void ReplayAnimation()
    {
        ResetPosition();
        AnimateButton();
    }

    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Clean up tweens
        DOTween.Kill(button.transform);
        if (canvasGroup != null)
            DOTween.Kill(canvasGroup);
    }
}