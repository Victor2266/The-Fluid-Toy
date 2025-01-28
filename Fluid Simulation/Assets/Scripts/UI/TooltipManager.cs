using TMPro;
using DG.Tweening;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private float fadeTime = 0.2f;
    private CanvasGroup canvasGroup;
    private Sequence currentSequence;


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

    }

    public void ShowTooltip(string message)
    {
        // Kill any ongoing animations
        currentSequence?.Kill();
        canvasGroup.DOKill();

        tooltipPanel.gameObject.SetActive(true);
        tooltipText.text = message;
        currentSequence = DOTween.Sequence().Append(canvasGroup.DOFade(1, fadeTime).SetEase(Ease.OutCubic));
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
}