using UnityEngine;
using TMPro;
using DG.Tweening;

public class SettingsHeaderAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text headerText;
    
    [Header("Animation Settings")]
    [SerializeField] private float startOffsetY = -50f;  // Smaller offset since it's just a header
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private Ease easeType = Ease.OutQuint;
    
    private Vector3 originalPosition;
    private bool hasAnimated = false;
    
    private void Awake()
    {
        // Store original position
        originalPosition = headerText.transform.localPosition;
        
        // Set initial state
        ResetPosition();
    }
    
    private void OnEnable()
    {
        // Ensure we start from the reset position when enabled
        ResetPosition();
        // Trigger animation
        AnimateHeader();
    }
    
    private void ResetPosition()
    {
        // Reset text position and alpha
        Vector3 startPos = originalPosition;
        startPos.y += startOffsetY;
        headerText.transform.localPosition = startPos;
        
        Color textColor = headerText.color;
        textColor.a = 0f;
        headerText.color = textColor;
        
        hasAnimated = false;
    }
    
    public void AnimateHeader()
    {
        if (hasAnimated) return;
        
        // Kill any existing tweens to prevent conflicts
        DOTween.Kill(headerText.transform);
        DOTween.Kill(headerText);
        
        // Animate position
        headerText.transform.DOLocalMove(originalPosition, animationDuration)
            .SetEase(easeType);
        
        // Animate opacity
        headerText.DOFade(1f, animationDuration * 0.8f)
            .SetEase(Ease.OutQuad);
        
        hasAnimated = true;
    }
    
    // Optional: Call this if you need to manually reset and replay the animation
    public void ReplayAnimation()
    {
        ResetPosition();
        AnimateHeader();
    }
}