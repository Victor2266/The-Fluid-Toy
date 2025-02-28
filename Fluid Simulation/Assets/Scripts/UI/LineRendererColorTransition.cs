using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererColorTransition : MonoBehaviour
{
    [Tooltip("Gradient representing the color transition.")]
    public Gradient colorGradient;

    [Tooltip("Duration of the color cycle in seconds.")]
    public float cycleDuration = 5f;

    private LineRenderer lineRenderer;
    private float cycleProgress = 0f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (colorGradient == null)
        {
            Debug.LogWarning("Please assign a color gradient for the transition to work.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // Update the cycle progress
        cycleProgress = Mathf.Repeat(cycleProgress + Time.deltaTime / cycleDuration, 1f);

        // Evaluate the color from the gradient
        Color currentColor = colorGradient.Evaluate(cycleProgress);

        // Apply the color to the LineRenderer
        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;
    }
}
