using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageAlphaPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image targetImage;

    [Header("Pulse Settings")]
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float pulseDuration = 1.0f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private int loopCount = -1; // -1 means infinite loops
    [SerializeField] private bool startOnAwake = true;

    private Sequence pulseSequence;

    private void Awake()
    {
        // If no image is assigned, try to get one from this gameObject
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (startOnAwake)
        {
            StartPulse();
        }
    }

    public void StartPulse()
    {
        // Kill any existing sequence to prevent duplicate animations
        if (pulseSequence != null)
        {
            pulseSequence.Kill();
        }

        // Ensure the image exists
        if (targetImage == null)
        {
            Debug.LogError("Image Alpha Pulse: No Image component found!");
            return;
        }

        // Create the pulse sequence
        pulseSequence = DOTween.Sequence();

        // Add the fade out tween
        pulseSequence.Append(targetImage.DOFade(minAlpha, pulseDuration / 2)
            .SetEase(easeType));

        // Add the fade in tween
        pulseSequence.Append(targetImage.DOFade(maxAlpha, pulseDuration / 2)
            .SetEase(easeType));

        // Set the loop count and start the sequence
        pulseSequence.SetLoops(loopCount)
            .Play();
    }

    public void StopPulse()
    {
        if (pulseSequence != null)
        {
            pulseSequence.Kill();
        }
    }

    private void OnDestroy()
    {
        // Clean up the sequence when the object is destroyed
        StopPulse();
    }

    // Optional: Reset the alpha to a specific value when stopping
    public void ResetAlpha(float alpha = 1.0f)
    {
        StopPulse();
        if (targetImage != null)
        {
            Color color = targetImage.color;
            color.a = alpha;
            targetImage.color = color;
        }
    }
}