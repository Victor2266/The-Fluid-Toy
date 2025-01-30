using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private float fadeTime = 0.25f;
    [SerializeField] private float moveDistance = 8f; // Distance to move upwards
    
    private Vector2 targetPosition;
    private CanvasGroup canvasGroup;
    private Sequence currentSequence;

    [SerializeField] private Image background;
    [SerializeField] private Vector2 padding = new Vector2(16, 2);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
            Instance = this;
        }
    }
    private void Start()
    {
        canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        tooltipPanel.gameObject.SetActive(false);

        targetPosition = tooltipPanel.anchoredPosition;
    }

    public void ShowTooltip(string message)
    {
        // Kill any ongoing animations
        currentSequence?.Kill();
        canvasGroup.DOKill();

        if(!tooltipPanel.gameObject.activeSelf){
            // Set starting position
            tooltipPanel.anchoredPosition = targetPosition - new Vector2(0, moveDistance);
        }

        tooltipPanel.gameObject.SetActive(true);
        tooltipText.text = message;

        // Wait for text layout to update
        Canvas.ForceUpdateCanvases();
        
        // Calculate background size
        Vector2 textSize = new Vector2(
            tooltipText.preferredWidth,
            tooltipText.preferredHeight
        );
        
        // Set background size with padding
        background.rectTransform.sizeDelta = textSize + padding;

        // Create animation sequence
        currentSequence = DOTween.Sequence()
            .Join(canvasGroup.DOFade(1, fadeTime).SetEase(Ease.OutCubic))
            .Join(tooltipPanel.DOAnchorPos(targetPosition, fadeTime).SetEase(Ease.OutCubic));
    }

    public void HideTooltip()
    {
        // Kill any ongoing animations
        currentSequence?.Kill();
        canvasGroup.DOKill();

        currentSequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(0, fadeTime).SetEase(Ease.OutCubic))
            .OnComplete(() => tooltipPanel.gameObject.SetActive(false));
    }

    void OnDestroy(){
        // Kill any ongoing animations
        currentSequence?.Kill();
        canvasGroup.DOKill();
    }
}