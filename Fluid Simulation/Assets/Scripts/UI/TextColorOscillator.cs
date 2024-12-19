using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextColorOscillator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Color Settings")]
    [SerializeField] private Color firstColor = Color.red;
    [SerializeField] private Color secondColor = Color.blue;
    
    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private bool playOnAwake = true;

    private Sequence colorSequence;

    private void Start()
    {
        // If no text component assigned, try to get it from this GameObject
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
            if (targetText == null)
            {
                Debug.LogError("No TextMeshProUGUI component found!");
                return;
            }
        }

        if (playOnAwake)
        {
            StartColorOscillation();
        }
    }

    public void StartColorOscillation()
    {
        // Kill any existing sequence
        if (colorSequence != null && colorSequence.IsPlaying())
        {
            colorSequence.Kill();
        }

        // Create a new sequence
        colorSequence = DOTween.Sequence();

        // Add the color transitions
        colorSequence.Append(targetText.DOColor(firstColor, transitionDuration).SetEase(easeType))
                    .Append(targetText.DOColor(secondColor, transitionDuration).SetEase(easeType));

        // Make it loop infinitely
        colorSequence.SetLoops(-1, LoopType.Yoyo);
    }

    public void StopColorOscillation()
    {
        if (colorSequence != null && colorSequence.IsPlaying())
        {
            colorSequence.Kill();
        }
    }

    private void OnDestroy()
    {
        // Clean up the sequence when the object is destroyed
        if (colorSequence != null)
        {
            colorSequence.Kill();
        }
    }
}