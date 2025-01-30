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
    
    private Vector3[] originalLocalPositions;
    
    private void Awake()
    {
        // Store original local positions
        originalLocalPositions = new Vector3[titleTexts.Length];
        for (int i = 0; i < titleTexts.Length; i++)
        {
            originalLocalPositions[i] = titleTexts[i].transform.localPosition;
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
            
            // Reset local position
            Vector3 startPos = originalLocalPositions[i];
            startPos.y += startOffsetY;
            titleTexts[i].transform.localPosition = startPos;
        }
    }
    
    private IEnumerator AnimateTitleSequence()
    {
        for (int i = 0; i < titleTexts.Length; i++)
        {
            // Start animation for current text
            AnimateText(titleTexts[i], originalLocalPositions[i], i);
            
            // Wait before starting next text animation
            yield return new WaitForSeconds(delayBetweenTexts);
        }
    }
    
    private void AnimateText(TMP_Text text, Vector3 targetLocalPosition, int index)
    {
        // Animate local position
        text.transform.DOLocalMove(targetLocalPosition, animationDuration)
            .SetEase(easeType);
        
        // Animate opacity
        text.DOFade(1f, animationDuration * 0.8f)
            .SetEase(Ease.OutQuad);
    }

    void OnDestroy(){
        // Kill any existing animations
        DOTween.KillAll();
    }
}