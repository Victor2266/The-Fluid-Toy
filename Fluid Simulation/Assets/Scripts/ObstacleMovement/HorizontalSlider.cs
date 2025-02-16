using UnityEngine;
using DG.Tweening;

public class HorizontalSlider : MonoBehaviour
{
    [Header("Movement Configuration")]
    [Tooltip("Distance to move left and right from the starting position")]
    public float slideDistance = 5f;

    [Tooltip("Duration of one complete movement (one way)")]
    public float slideDuration = 1f;

    [Tooltip("Delay before starting the movement")]
    public float initialDelay = 0f;

    [Tooltip("Delay between movements")]
    public float delayBetweenSlides = 0f;

    [Header("Easing Configuration")]
    [Tooltip("Ease type for the movement")]
    public Ease easeType = Ease.InOutQuad;

    [Tooltip("Should the easing be the same for both directions")]
    public bool useSameEasing = true;

    [Tooltip("Ease type for the return movement (if different easing is enabled)")]
    public Ease returnEaseType = Ease.InOutQuad;

    [Header("Behavior Configuration")]
    [Tooltip("Should the object move indefinitely")]
    public bool loopInfinitely = true;

    [Tooltip("Number of complete cycles (if not looping infinitely)")]
    public int numberOfCycles = 1;

    [Tooltip("Should the object return to its starting position when the script is disabled")]
    public bool returnToStartOnDisable = true;

    [Tooltip("Should movement start automatically on enable")]
    public bool autoStart = true;

    [Header("Direction Configuration")]
    [Tooltip("Should the movement start towards the right")]
    public bool startRight = true;

    [Tooltip("Should the movement alternate directions")]
    public bool alternateDirection = true;

    [Header("Randomization")]
    [Tooltip("Enable random variations in movement")]
    public bool enableRandomization = false;

    [Tooltip("Random variation in slide distance (plus or minus this amount)")]
    public float randomDistanceVariation = 2f;

    [Tooltip("Random variation in duration (plus or minus this amount)")]
    public float randomDurationVariation = 0.5f;

    [Tooltip("Random variation in delay between slides (plus or minus this amount)")]
    public float randomDelayVariation = 0.5f;

    [Tooltip("Chance to suddenly change direction mid-movement (0-1)")]
    [Range(0, 1)]
    public float directionChangeChance = 0f;

    [Tooltip("Minimum time between random direction changes")]
    public float minTimeBetweenDirectionChanges = 1f;

    [Header("Boundary Configuration")]
    [Tooltip("Enable boundary constraints")]
    public bool useBoundaries = true;

    [Tooltip("Maximum distance the object can move to the right from its starting position")]
    [Min(0)]
    public float rightBoundary = 10f;

    [Tooltip("Maximum distance the object can move to the left from its starting position")]
    [Min(0)]
    public float leftBoundary = 10f;

    [Header("Advanced Configuration")]
    [Tooltip("Use local position instead of world position")]
    public bool useLocalPosition = false;

    private Vector3 startPosition;
    private Sequence slideSequence;
    private bool isPlaying = false;
    private float lastDirectionChangeTime = 0f;
    private bool movingRight;

    private void Awake()
    {
        startPosition = useLocalPosition ? transform.localPosition : transform.position;
        movingRight = startRight;
    }

    private void OnEnable()
    {
        if (autoStart)
        {
            StartSliding();
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);

        if (slideSequence != null)
        {
            slideSequence.Kill(complete: false);
            slideSequence = null;
        }

        if (transform != null)
        {
            transform.DOKill();
        }
    }

    private void OnDisable()
    {
        StopSliding();

        if (returnToStartOnDisable)
        {
            if (useLocalPosition)
            {
                transform.localPosition = startPosition;
            }
            else
            {
                transform.position = startPosition;
            }
        }
    }

    public void StartSliding()
    {
        if (isPlaying) return;

        isPlaying = true;
        CreateSlideSequence();
    }

    public void StopSliding()
    {
        if (!isPlaying) return;

        isPlaying = false;
        if (slideSequence != null)
        {
            slideSequence.Kill(complete: false);
            slideSequence = null;
        }

        DOTween.Kill(transform);
    }

    public void PauseSliding()
    {
        slideSequence?.Pause();
    }

    public void ResumeSliding()
    {
        slideSequence?.Play();
    }

    private float GetRandomizedValue(float baseValue, float variation)
    {
        return baseValue + Random.Range(-variation, variation);
    }

    private Vector3 ClampPositionToBoundaries(Vector3 position)
    {
        if (!useBoundaries) return position;

        Vector3 relativePosition = position - startPosition;
        float clampedX = Mathf.Clamp(relativePosition.x, -leftBoundary, rightBoundary);
        return startPosition + new Vector3(clampedX, relativePosition.y, relativePosition.z);
    }

    private bool WouldExceedBoundary(Vector3 targetPosition)
    {
        if (!useBoundaries) return false;

        Vector3 relativePosition = targetPosition - startPosition;
        return relativePosition.x > rightBoundary || relativePosition.x < -leftBoundary;
    }

    private void CreateSlideSequence()
    {
        if (!isPlaying) return;
        
        slideSequence?.Kill();
        slideSequence = DOTween.Sequence();

        if (initialDelay > 0)
        {
            slideSequence.AppendInterval(initialDelay);
        }

        Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;
        currentPos = ClampPositionToBoundaries(currentPos);

        float currentSlideDistance = enableRandomization ?
            GetRandomizedValue(slideDistance, randomDistanceVariation) :
            slideDistance;

        Vector3 target = currentPos + (movingRight ? Vector3.right : Vector3.left) * currentSlideDistance;
        
        if (useBoundaries)
        {
            Vector3 clampedTarget = ClampPositionToBoundaries(target);
            
            if (WouldExceedBoundary(target))
            {
                target = clampedTarget;
                //movingRight = !movingRight;
            }
        }

        float currentDuration = enableRandomization ?
            GetRandomizedValue(slideDuration, randomDurationVariation) :
            slideDuration;

        if (useBoundaries)
        {
            float actualDistance = Vector3.Distance(currentPos, target);
            float originalDistance = currentSlideDistance;
            currentDuration *= (actualDistance / originalDistance);
        }

        float currentDelay = enableRandomization ?
            GetRandomizedValue(delayBetweenSlides, randomDelayVariation) :
            delayBetweenSlides;

        Tween moveTween = useLocalPosition ?
            transform.DOLocalMove(target, currentDuration).SetEase(easeType) :
            transform.DOMove(target, currentDuration).SetEase(easeType);

        slideSequence
            .Append(moveTween)
            .AppendInterval(currentDelay)
            .OnComplete(() => {
                if (!loopInfinitely && --numberOfCycles <= 0)
                {
                    StopSliding();
                    return;
                }
                
                if (alternateDirection || WouldExceedBoundary(target))
                {
                    movingRight = !movingRight;
                }
                CreateSlideSequence();
            });

        if (enableRandomization && directionChangeChance > 0)
        {
            slideSequence.OnUpdate(() =>
            {
                if (Time.time - lastDirectionChangeTime >= minTimeBetweenDirectionChanges)
                {
                    if (Random.value < directionChangeChance * Time.deltaTime)
                    {
                        lastDirectionChangeTime = Time.time;
                        
                        Vector3 potentialTarget = currentPos + (!movingRight ? Vector3.right : Vector3.left) * currentSlideDistance;
                        if (!WouldExceedBoundary(potentialTarget))
                        {
                            movingRight = !movingRight;
                            slideSequence.Kill();
                            CreateSlideSequence();
                        }
                    }
                }
            });
        }

        slideSequence.Play();
    }
}