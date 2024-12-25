using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro; // Add TextMeshPro namespace

public class LevelCompleteAnimation : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject overlayPanel;
    private CanvasGroup overlayCanvasGroup;
    [SerializeField] private RectTransform checkmarkTransform;
    [SerializeField] private Image checkmarkImage;
    [SerializeField] private CanvasGroup completionText;

    [SerializeField] private int currentLevel;
    
    [Header("Animation Settings")]
    [SerializeField] private float overlayFadeDuration = 0.5f;
    [SerializeField] private float checkmarkScaleDuration = 0.7f;
    [SerializeField] private float checkmarkRotationDuration = 0.5f;
    [SerializeField] private float textFadeDuration = 0.5f;
    [SerializeField] private float bounceStrength = 1.2f;

    [SerializeField] private Image FirstStarImage;    
    [SerializeField] private Image SecondStarImage;    
    [SerializeField] private Image ThirdStarImage;
    
    private void Start()
    {
        // Initialize UI elements as invisible
        overlayCanvasGroup = overlayPanel.GetComponent<CanvasGroup>();
        overlayCanvasGroup.alpha = 0f;
        checkmarkImage.color = new Color(1f, 1f, 1f, 0f);
        checkmarkTransform.localScale = Vector3.zero;
        if (completionText != null)
        {
            completionText.alpha = 0f; // Use alpha property for TextMeshPro
            completionText.transform.localPosition = new Vector3(0f, -410f, 0f);
        }
    }
    
    public void PlayLevelCompleteAnimation()
    {
        int currentLevelScore = PlayerPrefs.GetInt($"Level_{currentLevel}_Score", 0);

        // Create animation sequence
        Sequence completeSequence = DOTween.Sequence();
        DOTween.Kill(completeSequence);
        
        // Fade in overlay background
        completeSequence.Append(overlayCanvasGroup.DOFade(1f, overlayFadeDuration));
        
        // Fade in and scale up checkmark with bounce
        completeSequence.Append(checkmarkImage.DOFade(1f, checkmarkScaleDuration/2f));
        completeSequence.Join(checkmarkTransform.DOScale(Vector3.one * bounceStrength, checkmarkScaleDuration)
            .SetEase(Ease.OutBack));
        
        // Rotate checkmark
        completeSequence.Join(checkmarkTransform.DORotate(new Vector3(0f, 0f, 360f), checkmarkRotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad));
        
        // Scale back to normal size
        completeSequence.Append(checkmarkTransform.DOScale(Vector3.one, checkmarkScaleDuration/2f)
            .SetEase(Ease.OutBack));
        
        // Fade in completion score if available
        if (completionText != null)
        {
            completeSequence.Append(completionText.DOFade(1f, textFadeDuration));
            completeSequence.Join(completionText.transform.DOLocalMoveY(-300f, textFadeDuration)).SetEase(Ease.OutQuint);

            // Fade in each star image based on score
            if (currentLevelScore >= 3)
            {
                completeSequence.Append(FirstStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                completeSequence.Append(SecondStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                completeSequence.Append(ThirdStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
            }
            else if (currentLevelScore == 2)
            {
                completeSequence.Append(FirstStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                completeSequence.Append(SecondStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                completeSequence.Append(ThirdStarImage.DOFade(0.1f, textFadeDuration).SetEase(Ease.OutBounce));
            }
            else if (currentLevelScore == 1)
            {
                completeSequence.Append(FirstStarImage.DOFade(0.1f, textFadeDuration).SetEase(Ease.OutBounce));
                completeSequence.Append(SecondStarImage.DOFade(0.1f, textFadeDuration).SetEase(Ease.OutBounce));
                completeSequence.Append(ThirdStarImage.DOFade(0.1f, textFadeDuration).SetEase(Ease.OutBounce));
            }

        }
        
        // Play the sequence
        overlayPanel.SetActive(true);
        completeSequence.Play();
    }

    // Optional: Method to reset the animation
    public void ResetAnimation()
    {
        overlayPanel.SetActive(false);
        overlayCanvasGroup.alpha = 0f;
        checkmarkImage.color = new Color(1f, 1f, 1f, 0f);
        checkmarkTransform.localScale = Vector3.zero;
        checkmarkTransform.rotation = Quaternion.identity;
        if (completionText != null){
            completionText.alpha = 0f; // Reset TextMeshPro alpha
            completionText.transform.localPosition = new Vector3(0f, -410f, 0f);
        }
    }
}