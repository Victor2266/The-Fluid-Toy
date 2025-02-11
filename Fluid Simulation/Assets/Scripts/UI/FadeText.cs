using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class FadeText : MonoBehaviour
{
    public TMP_Text tmpText;       // Reference to the TMP_Text element (for Text Mesh Pro)
    public Image fadeBG;
    public float fadeDuration = 2.0f; // Duration of the fade-in/out
    public float displayTime = 2.0f;  // Time the text is fully visible
    
    private Color originalColor;      // To store the initial color
    private Color originalBGColor;      // To store the initial color
    private float timer = 0f;         // Track time for fading
    private bool fadeStarted = false; // Track if fade has started
    public bool fadeImmediately = false;

    void Start()
    {
        // Store the original color of the text
        originalColor = tmpText.color;

        if(fadeBG != null)
        {
            originalBGColor = fadeBG.color;
        }

        if(fadeImmediately)
        {
            fadeStarted = true;
            StartCoroutine(FadeInOut());
        }
    }

    void Update()
    {
        // Check for any input if fade hasn't started yet
        if (!fadeStarted && !fadeImmediately && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            fadeStarted = true;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeInOut()
    {
        // Set initial transparency to 0 (invisible)
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        if(fadeBG != null)
        {
            fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, 0);
        }

        // Fade in
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alphaValue = Mathf.Lerp(0, 1, timer / fadeDuration);
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);

            if(fadeBG != null)
            {
                fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, alphaValue);
            }
            yield return null;
        }

        // Ensure it's fully visible
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        if(fadeBG != null)
        {
            fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, 1);
        }
        yield return new WaitForSeconds(displayTime);

        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alphaValue = Mathf.Lerp(1, 0, timer / fadeDuration);
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);
            if(fadeBG != null)
            {
                fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, alphaValue);
            }
            yield return null;
        }

        // Ensure it's fully invisible
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        if(fadeBG != null)
        {
            fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, 0);
        }
        Destroy(gameObject);
        Destroy(fadeBG.gameObject);
    }
        IEnumerator FadeOut()
    {
        // Ensure it's fully visible
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);

        yield return new WaitForSeconds(displayTime);

        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alphaValue = Mathf.Lerp(1, 0, timer / fadeDuration);
            float BGalphaValue = Mathf.Lerp(originalBGColor.a, 0, timer / fadeDuration);
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);
            if(fadeBG != null)
            {
                fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, BGalphaValue);
            }
            yield return null;
        }

        // Ensure it's fully invisible
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        if(fadeBG != null)
        {
            fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, 0);
        }
        Destroy(gameObject);
        Destroy(fadeBG.gameObject);
    }
}