using UnityEngine;
using DG.Tweening;

public class RotateAndFade : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Child object with the sprite to fade")]
    public SpriteRenderer targetSprite;

    [Header("Rotation Settings")]
    [Tooltip("Starting rotation on Z axis")]
    public float startRotation = -41f;
    
    [Tooltip("Ending rotation on Z axis")]
    public float endRotation = -200f;
    
    [Tooltip("Duration of the rotation in seconds")]
    public float rotationDuration = 2f;
    
    [Tooltip("Easing function for rotation")]
    public Ease rotationEase = Ease.InOutQuad;

    [Header("Fade Settings")]
    [Tooltip("Duration of the fade in seconds")]
    public float fadeDuration = 2f;
    
    [Tooltip("Easing function for fading")]
    public Ease fadeEase = Ease.InOutSine;
    
    [Tooltip("Delay before starting fade out")]
    public float fadeOutDelay = 0.5f;

    [Header("Loop Settings")]
    [Tooltip("Should the animation loop?")]
    public bool loopAnimation = false;
    
    [Tooltip("Number of times to loop (0 = infinite)")]
    [Min(0)]
    public int loopCount = 0;
    
    [Tooltip("Delay between loops in seconds")]
    [Min(0)]
    public float loopDelay = 0.5f;
    
    [Tooltip("How loops should behave")]
    public LoopType loopType = LoopType.Restart;

    private Sequence mainSequence;

    void Start()
    {
        // Initialize component if not assigned
        if (targetSprite == null)
        {
            // Try to find sprite renderer in children
            targetSprite = GetComponentInChildren<SpriteRenderer>();
            
            if (targetSprite == null)
            {
                Debug.LogError("No SpriteRenderer found! Assign it in the inspector.");
                return;
            }
        }

        // Set initial states
        transform.localRotation = Quaternion.Euler(0, 0, startRotation);
        Color spriteColor = targetSprite.color;
        spriteColor.a = 0f;
        targetSprite.color = spriteColor;

        // Start the animations
        PlayAnimations();
    }

    public void PlayAnimations()
    {
        // Kill previous animations if they exist
        if (mainSequence != null)
        {
            mainSequence.Kill();
        }

        // Create main sequence that will contain all animations
        mainSequence = DOTween.Sequence();

        // Create a nested sequence for one complete animation cycle
        Sequence animationCycle = DOTween.Sequence();

        // Add rotation
        animationCycle.Join(transform.DOLocalRotate(new Vector3(0, 0, endRotation), rotationDuration)
            .SetEase(rotationEase));

        // Add fade in/out
        Sequence fadeSequence = DOTween.Sequence();
        fadeSequence.Append(targetSprite.DOFade(1f, fadeDuration / 2).SetEase(fadeEase));
        fadeSequence.AppendInterval(fadeOutDelay);
        fadeSequence.Append(targetSprite.DOFade(0f, fadeDuration / 2).SetEase(fadeEase));
        
        animationCycle.Join(fadeSequence);

        // Add the main animation cycle to the main sequence
        mainSequence.Append(animationCycle);
        
        // Set up looping
        if (loopAnimation)
        {
            // Calculate the total loops (-1 means infinite)
            int totalLoops = loopCount == 0 ? -1 : loopCount - 1;
            
            if (totalLoops != 0)
            {
                // Add delay between loops if specified
                if (loopDelay > 0)
                {
                    mainSequence.AppendInterval(loopDelay);
                }
                
                // Configure the loop settings
                mainSequence.SetLoops(totalLoops, loopType);
            }
        }

        // Play the full sequence
        mainSequence.Play();
    }

    void OnDestroy()
    {
        // Clean up tweens when the object is destroyed
        if (mainSequence != null)
        {
            mainSequence.Kill();
        }
    }

    // Restart animations with a button in the inspector
    [ContextMenu("Replay Animations")]
    public void RestartAnimations()
    {
        // Reset to starting position
        transform.localRotation = Quaternion.Euler(0, 0, startRotation);
        
        // Play animations
        PlayAnimations();
    }
}