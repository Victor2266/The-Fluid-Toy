using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

public class TitleAnimation : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TMP_Text[] titleTexts;
    
    [Header("Animation Settings")]
    [SerializeField] private float startOffsetY = -100f;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float delayBetweenTexts = 0.2f;
    [SerializeField] private Ease easeType = Ease.OutBack;
    
    private Vector3[] originalPositions;
    
    private void Awake()
    {
        // Store original positions
        originalPositions = new Vector3[titleTexts.Length];
        for (int i = 0; i < titleTexts.Length; i++)
        {
            originalPositions[i] = titleTexts[i].transform.position;
        }
    }
    
    private void OnEnable()
    {
        // Reset and start animation whenever the object is enabled
        ResetPositions();
        StartCoroutine(AnimateTitleSequence());
    }
    
    private void ResetPositions()
    {
        // Kill any existing animations
        DOTween.Kill(gameObject);
        
        for (int i = 0; i < titleTexts.Length; i++)
        {
            // Reset alpha
            Color textColor = titleTexts[i].color;
            textColor.a = 0f;
            titleTexts[i].color = textColor;
            
            // Reset position
            Vector3 startPos = originalPositions[i];
            startPos.y += startOffsetY;
            titleTexts[i].transform.position = startPos;
        }
    }
    
    private IEnumerator AnimateTitleSequence()
    {
        for (int i = 0; i < titleTexts.Length; i++)
        {
            // Start animation for current text
            AnimateText(titleTexts[i], originalPositions[i], i);
            
            // Wait before starting next text animation
            yield return new WaitForSeconds(delayBetweenTexts);
        }
    }
    
    private void AnimateText(TMP_Text text, Vector3 targetPosition, int index)
    {
        // Animate position
        text.transform.DOMove(targetPosition, animationDuration)
            .SetEase(easeType);
        
        // Animate opacity
        text.DOFade(1f, animationDuration * 0.8f)
            .SetEase(Ease.OutQuad);
    }
}