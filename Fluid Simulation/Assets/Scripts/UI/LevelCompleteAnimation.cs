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
    [SerializeField] private CanvasGroup completionScore;

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

    public AudioClip starSound;
    public AudioSource audioSource;

    private int currentLevelScore = 0;
    
    private void Start()
    {
        // Initialize UI elements as invisible
        overlayCanvasGroup = overlayPanel.GetComponent<CanvasGroup>();
        overlayCanvasGroup.alpha = 0f;
        checkmarkImage.color = new Color(1f, 1f, 1f, 0f);
        checkmarkTransform.localScale = Vector3.zero;
        if (completionScore != null)
        {
            completionScore.alpha = 0f; // Use alpha property for TextMeshPro
            completionScore.transform.localPosition = new Vector3(0f, -410f, 0f);
        }
    }
    
    public void PlayLevelCompleteAnimation()
    {
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
        if (completionScore != null)
        {
            completeSequence.Append(completionScore.DOFade(1f, textFadeDuration));
            completeSequence.Join(completionScore.transform.DOLocalMoveY(-300f+32f, textFadeDuration)).SetEase(Ease.OutQuint);

            // Fade in each star image based on score
            if (currentLevelScore >= 3)
            {
                completeSequence.AppendCallback(() => audioSource.pitch = 0.8f);
                completeSequence.AppendCallback(() => audioSource.PlayOneShot(starSound));
                completeSequence.Append(FirstStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                // completeSequence.Join(checkmarkTransform.DOScale(Vector3.one * 0.8f, textFadeDuration/3f)
                //     .SetEase(Ease.OutExpo));
                // completeSequence.Append(checkmarkTransform.DOScale(Vector3.one, textFadeDuration/3f)
                //     .SetEase(Ease.OutBack));

                completeSequence.AppendCallback(() => audioSource.pitch = 0.9f);
                completeSequence.AppendCallback(() => audioSource.PlayOneShot(starSound));
                completeSequence.Append(SecondStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                // completeSequence.Join(checkmarkTransform.DOScale(Vector3.one * 0.8f, textFadeDuration/3f)
                //     .SetEase(Ease.OutExpo));
                // completeSequence.Append(checkmarkTransform.DOScale(Vector3.one, textFadeDuration/3f)
                //     .SetEase(Ease.OutBack));

                completeSequence.AppendCallback(() => audioSource.pitch = 1f);
                completeSequence.AppendCallback(() => audioSource.PlayOneShot(starSound));
                completeSequence.Append(ThirdStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                // completeSequence.Join(checkmarkTransform.DOScale(Vector3.one * 0.8f, textFadeDuration/3f)
                //     .SetEase(Ease.OutExpo));
                // completeSequence.Append(checkmarkTransform.DOScale(Vector3.one, textFadeDuration/3f)
                //     .SetEase(Ease.OutBack));
            }
            else if (currentLevelScore == 2)
            {
                completeSequence.AppendCallback(() => audioSource.pitch = 0.8f);
                completeSequence.AppendCallback(() => audioSource.PlayOneShot(starSound));
                completeSequence.Append(FirstStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));

                completeSequence.AppendCallback(() => audioSource.pitch = 0.9f);
                completeSequence.AppendCallback(() => audioSource.PlayOneShot(starSound));
                completeSequence.Append(SecondStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));
                
                completeSequence.Append(ThirdStarImage.DOFade(0.1f, textFadeDuration).SetEase(Ease.OutBounce));
            }
            else if (currentLevelScore == 1)
            {
                completeSequence.AppendCallback(() => audioSource.pitch = 0.8f);
                completeSequence.AppendCallback(() => audioSource.PlayOneShot(starSound));
                completeSequence.Append(FirstStarImage.DOFade(1f, textFadeDuration).SetEase(Ease.OutBounce));

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
        if (completionScore != null){
            completionScore.alpha = 0f; // Reset TextMeshPro alpha
            completionScore.transform.localPosition = new Vector3(0f, -410f, 0f);
        }
    }

    public void setCurrentScore(int score)
    {
        currentLevelScore = score;
    }

    void OnDestroy()
    {
        DOTween.KillAll();
    }
}