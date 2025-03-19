using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderSmoothSnapper : MonoBehaviour, IPointerUpHandler
{
    [Tooltip("The slider to snap to whole numbers")]
    public Slider slider;
    
    [Tooltip("How quickly the slider snaps to the nearest whole number")]
    public float snapSpeed = 5.0f;
    
    [Tooltip("Whether to snap only when the user releases the slider")]
    public bool snapOnRelease = true;
    
    private bool isSnapping = false;
    private Coroutine snapCoroutine;
    
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
        
        // Add listener for slider value change
        if (!snapOnRelease)
        {
            slider.onValueChanged.AddListener(OnSliderValueChangedAutoSnap);
        }
        else
        {
            // We'll use IPointerUpHandler instead for release detection
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }
    
    // This is called when the pointer is released from the slider
    public void OnPointerUp(PointerEventData eventData)
    {
        if (snapOnRelease && slider != null)
        {
            SnapToNearestWholeNumber();
        }
    }
    
    private void OnSliderValueChanged(float value)
    {
        // Do nothing while the slider is being dragged
        // The snap will happen on release
    }
    
    private void OnSliderValueChangedAutoSnap(float value)
    {
        // Snap to nearest whole number whenever the value changes
        SnapToNearestWholeNumber();
    }
    
    private void SnapToNearestWholeNumber()
    {
        // Don't start a new coroutine if one is already running
        if (isSnapping)
            return;
        
        // Calculate the nearest whole number
        float targetValue = Mathf.Round(slider.value);
        
        // If we're already at a whole number, no need to snap
        if (Mathf.Approximately(slider.value, targetValue))
            return;
        
        // Start the smooth snapping coroutine
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
            
        snapCoroutine = StartCoroutine(SmoothSnapCoroutine(targetValue));
    }
    
    private IEnumerator SmoothSnapCoroutine(float targetValue)
    {
        isSnapping = true;
        
        float startValue = slider.value;
        float time = 0f;
        
        // Calculate the duration based on the distance and speed
        float duration = Mathf.Abs(targetValue - startValue) / snapSpeed;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            
            // Use smooth step for a more natural easing
            float smoothT = t * t * (3f - 2f * t);
            
            // Update the slider value
            slider.value = Mathf.Lerp(startValue, targetValue, smoothT);
            
            yield return null;
        }
        
        // Ensure we end exactly at the target value
        slider.value = targetValue;
        isSnapping = false;
    }
    
    private void OnDestroy()
    {
        // Clean up listeners when the component is destroyed
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
        }
    }
}