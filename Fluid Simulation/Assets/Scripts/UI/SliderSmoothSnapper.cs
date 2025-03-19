using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SliderSnapper : MonoBehaviour, IPointerUpHandler
{
    [Tooltip("The slider to snap to whole numbers")]
    public Slider slider;
    
    [Tooltip("How long the snap animation should take (in seconds)")]
    [Range(0.1f, 1.0f)]
    public float snapDuration = 0.25f;
    
    [Tooltip("The easing function to use for the snap animation")]
    public Ease snapEase = Ease.OutBack;
    
    [Tooltip("The threshold at which to snap to the next whole number while dragging (0.0-0.5)")]
    [Range(0.0f, 0.5f)]
    public float snapThreshold = 0.25f;
    public bool snapWhileDragging = false;

    public AudioSource audioSource;
    
    private float lastSnapValue = 0f;
    private Tweener currentTween;
    private bool isAnimating = false;
    
    private void Start()
    {
        if (slider == null)
        {
            // Try to get the slider component from this GameObject
            slider = GetComponent<Slider>();
            
            if (slider == null)
            {
                Debug.LogError("SliderSnapper: No slider assigned or found on this GameObject.");
                enabled = false;
                return;
            }
        }
        
        // Round the initial value
        lastSnapValue = Mathf.Round(slider.value);
        slider.value = lastSnapValue;
        
        // Add event listeners
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    // This handles snapping while dragging
    private void OnSliderValueChanged(float value)
    {
        if (!snapWhileDragging)
            return;
        // Skip if we're currently animating a snap
        if (isAnimating)
            return;
        
        float currentValue = slider.value;
        float decimalPart = currentValue - Mathf.Floor(currentValue);
        
        // Determine if we need to snap
        bool shouldSnapUp = decimalPart >= (1.0f - snapThreshold);
        bool shouldSnapDown = decimalPart <= snapThreshold;
        
        if (shouldSnapUp || shouldSnapDown)
        {
            float targetValue;
            
            if (shouldSnapUp)
                targetValue = Mathf.Ceil(currentValue);
            else
                targetValue = Mathf.Floor(currentValue);
            
            // Only snap if we're moving to a new value
            if (targetValue != lastSnapValue)
            {
                // Kill any existing tween
                if (currentTween != null && currentTween.IsActive())
                    currentTween.Kill();
                
                // Create a new tween
                isAnimating = true;
                currentTween = DOTween.To(() => slider.value, x => slider.value = x, targetValue, snapDuration)
                    .SetEase(snapEase)
                    .OnComplete(() => {
                        isAnimating = false;
                        lastSnapValue = targetValue;
                    });
            }
        }
    }
    
    // This handles snapping when the slider is released
    public void OnPointerUp(PointerEventData eventData)
    {
        // Kill any existing tween
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();
        
        // Always snap to nearest whole number on release
        float targetValue = Mathf.Round(slider.value);
        
        // Only animate if we're not already at the target
        if (!Mathf.Approximately(slider.value, targetValue))
        {
            isAnimating = true;
            currentTween = DOTween.To(() => slider.value, x => slider.value = x, targetValue, snapDuration)
                .SetEase(snapEase)
                .OnComplete(() => {
                    isAnimating = false;
                    lastSnapValue = targetValue;
                    audioSource.Play();
                });
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listeners when the component is destroyed
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
        }
        
        // Kill any active tweens
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();
    }
}