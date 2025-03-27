using UnityEngine;
using DG.Tweening;

public class InteractivePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Color to pulse from (original sprite color)")]
    public Color originalColor;

    [Tooltip("Color to pulse to")]
    public Color pulseColor = Color.white;

    [Tooltip("Duration of one pulse cycle")]
    public float pulseDuration = 1f;

    [Tooltip("Stop pulsing after first interaction")]
    public bool stopAfterFirstTouch = true;

    [Header("Interaction Settings")]
    [Tooltip("Enable mouse hover detection")]
    public bool enableHoverDetection = true;

    [Tooltip("Hover scale multiplier")]
    public float hoverScaleMultiplier = 1.1f;

    public SpriteRenderer spriteRenderer;
    private Tweener pulseTweener;
    // private bool hasBeenInteractedWith = false;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // If no original color is set, use the current sprite color
        if (originalColor == Color.clear)
        {
            originalColor = spriteRenderer.color;
        }

        StartPulsing();
    }

    void StartPulsing()
    {
        // Create a pulsing tween that goes between original and pulse colors
        pulseTweener = spriteRenderer.DOColor(pulseColor, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnMouseEnter()
    {
        if (!enableHoverDetection) return;
        
        // Optional additional visual feedback on hover
        transform.DOScale(hoverScaleMultiplier, 0.2f);
    }

    void OnMouseExit()
    {
        if (!enableHoverDetection) return;
        
        // Return to original scale
        transform.DOScale(1f, 0.2f);
    }

    void OnMouseDown()
    {
        // Mark as interacted
        // hasBeenInteractedWith = true;

        // Stop pulsing if option is enabled
        if (stopAfterFirstTouch)
        {
            // Kill the pulsing tween
            pulseTweener?.Kill();

            // Smoothly return to original color
            spriteRenderer.DOColor(originalColor, 0.3f);
        }
    }

    // Public method to manually restart pulsing
    public void RestartPulsing()
    {
        // hasBeenInteractedWith = false;
        StartPulsing();
    }

    // Optional: Cleanup when object is destroyed
    void OnDestroy()
    {
        pulseTweener?.Kill();
    }
}