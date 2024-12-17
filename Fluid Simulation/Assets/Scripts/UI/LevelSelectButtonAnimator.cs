using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectButtonAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Time in seconds between each button's animation start")]
    [SerializeField] private float delayBetweenButtons = 0.1f;
    
    [Tooltip("Duration of each button's fade animation")]
    [SerializeField] private float fadeInDuration = 0.3f;
    
    [Tooltip("Optional easing curve for the fade animation")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private List<ButtonData> buttonDataList = new List<ButtonData>();
    private bool isAnimating = false;
    private bool hasInitialized = false;

    private class ButtonData
    {
        public CanvasGroup canvasGroup;
        public Animator animator;
        public Button button;
        public bool wasInteractable;
    }

    private void OnEnable()
    {
        // Reset all buttons to invisible immediately
        foreach (var buttonData in buttonDataList)
        {
            if (buttonData.canvasGroup != null)
            {
                buttonData.canvasGroup.alpha = 0f;
                buttonData.canvasGroup.interactable = false;
                buttonData.canvasGroup.blocksRaycasts = false;
            }
            if (buttonData.animator != null)
            {
                // Set the correct interactable state before enabling animator
                buttonData.button.interactable = buttonData.wasInteractable;
                buttonData.animator.enabled = true;
            }
        }

        // Start animation if we have buttons
        if (hasInitialized && buttonDataList.Count > 0 && !isAnimating)
        {
            StartCoroutine(AnimateButtons());
        }
    }

    public void InitializeButtons()
    {
        if (hasInitialized)
        {
            return;
        }
        
        hasInitialized = true;
        buttonDataList.Clear();

        foreach (Transform child in transform)
        {
            ButtonData buttonData = new ButtonData();
            
            buttonData.button = child.gameObject.GetComponent<Button>();
            if (buttonData.button == null)
            {
                Debug.LogWarning($"Object {child.name} is missing a Button component!");
                continue;
            }

            // Store the initial interactable state
            buttonData.wasInteractable = buttonData.button.interactable;
            
            buttonData.canvasGroup = child.gameObject.GetComponent<CanvasGroup>();
            if (buttonData.canvasGroup == null)
            {
                buttonData.canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            }
            
            buttonData.animator = child.gameObject.GetComponent<Animator>();
            
            // Set initial states
            buttonData.canvasGroup.alpha = 0f;
            buttonData.canvasGroup.interactable = false;
            buttonData.canvasGroup.blocksRaycasts = false;
            
            if (buttonData.animator != null)
            {
                // Set the correct state before any animations
                buttonData.button.interactable = buttonData.wasInteractable;
                buttonData.animator.enabled = true;
            }
            
            buttonDataList.Add(buttonData);
        }

        // If the object is active, start animation
        if (gameObject.activeInHierarchy && buttonDataList.Count > 0)
        {
            StartCoroutine(AnimateButtons());
        }
    }

    private IEnumerator AnimateButtons()
    {
        isAnimating = true;

        // Ensure all buttons are properly set up before starting animations
        foreach (var buttonData in buttonDataList)
        {
            buttonData.canvasGroup.alpha = 0f;
            buttonData.canvasGroup.interactable = false;
            buttonData.canvasGroup.blocksRaycasts = false;
            
            if (buttonData.animator != null)
            {
                buttonData.button.interactable = buttonData.wasInteractable;
            }
        }

        // Wait a frame to ensure all states are properly set
        yield return null;

        // Now animate each button
        foreach (var buttonData in buttonDataList)
        {
            StartCoroutine(FadeInButton(buttonData));
            yield return new WaitForSeconds(delayBetweenButtons);
        }

        isAnimating = false;
    }

    private IEnumerator FadeInButton(ButtonData buttonData)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeInDuration;
            
            float alpha = fadeCurve.Evaluate(normalizedTime);
            buttonData.canvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        buttonData.canvasGroup.alpha = 1f;
        buttonData.canvasGroup.interactable = true;
        buttonData.canvasGroup.blocksRaycasts = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAnimating = false;

        // Restore all buttons to their proper states
        foreach (var buttonData in buttonDataList)
        {
            if (buttonData.animator != null)
            {
                buttonData.animator.enabled = true;
            }
            buttonData.canvasGroup.interactable = true;
            buttonData.canvasGroup.blocksRaycasts = true;
            buttonData.canvasGroup.alpha = 1f;
            buttonData.button.interactable = buttonData.wasInteractable;
        }
    }
}