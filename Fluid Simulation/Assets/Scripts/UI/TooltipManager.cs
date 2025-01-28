using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
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

    public void ShowTooltip(string message)
    {
        tooltipPanel.gameObject.SetActive(true);
        tooltipText.text = message;
    }

    public void HideTooltip()
    {
        tooltipPanel.gameObject.SetActive(false);
    }
}