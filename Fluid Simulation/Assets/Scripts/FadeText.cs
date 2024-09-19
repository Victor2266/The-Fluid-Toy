using UnityEngine;
using TMPro;  // Add this to use Text Mesh Pro
using System.Collections;

public class FadeText : MonoBehaviour
{
    public TMP_Text tmpText;       // Reference to the TMP_Text element (for Text Mesh Pro)
    public float fadeDuration = 2.0f; // Duration of the fade-in/out
    public float displayTime = 2.0f;  // Time the text is fully visible

    private Color originalColor;      // To store the initial color
    private float timer = 0f;         // Track time for fading

    void Start()
    {
        // Store the original color of the text
        originalColor = tmpText.color;

        // Set initial transparency to 0 (invisible)
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        // Start the fading process
        StartCoroutine(FadeInOut());
    }

    IEnumerator FadeInOut()
    {
        // Fade in
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alphaValue = Mathf.Lerp(0, 1, timer / fadeDuration);
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);
            yield return null;
        }

        // Ensure it's fully visible
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        yield return new WaitForSeconds(displayTime);

        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alphaValue = Mathf.Lerp(1, 0, timer / fadeDuration);
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);
            yield return null;
        }

        // Ensure it's fully invisible
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
    }
}
