using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FadeText : MonoBehaviour
{
    public TMP_Text tmpText;           // Reference to the TMP_Text element (for Text Mesh Pro)
    public Image fadeBG;               // Background image
    public float fadeDuration = 2.0f;  // Duration of the fade-in/out
    public float displayTime = 2.0f;   // Time the text is fully visible
    
    public bool fadeInOutImmediately = false;
    public bool FadeInOnStart = false; // New bool to control DOTween fade-in with movement

    public bool moveBGOnStart = false;
    public float moveDistance = 50f;   // How far the text should move upward
    
    private Color originalColor;       // To store the initial text color
    private Color originalBGColor;     // To store the initial background color
    private float timer = 0f;          // Track time for fading
    private bool fadeStarted = false;  // Track if fade has started
    private Vector3 originalPosition;  // Store the initial position
    private Vector3 originalBGPosition;  // Store the initial position

    void Start()
    {
        // Store the original color of the text
        originalColor = tmpText.color;
        
        // Store the original position
        originalPosition = transform.position;

        if(fadeBG != null)
        {
            originalBGPosition = fadeBG.transform.position;
            originalBGColor = fadeBG.color;
        }

        if(FadeInOnStart)
        {
            // Setup initial state
            tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
            if(fadeBG != null)
            {
                fadeBG.color = new Color(originalBGColor.r, originalBGColor.g, originalBGColor.b, 0);
                if (moveBGOnStart)
                    fadeBG.transform.position = new Vector3(originalBGPosition.x, originalBGPosition.y - moveDistance, originalBGPosition.z);
            }
            
            // Use DOTween to fade in and move upward
            transform.position = new Vector3(originalPosition.x, originalPosition.y - moveDistance, originalPosition.z);
            
            // Animate text
            tmpText.DOFade(1f, fadeDuration);
            transform.DOMoveY(originalPosition.y, fadeDuration).SetEase(Ease.OutBack);
            
            // Animate background if available
            if(fadeBG != null)
            {
                fadeBG.DOFade(originalBGColor.a, fadeDuration);
                if (moveBGOnStart)
                    fadeBG.transform.DOMoveY(originalBGPosition.y, fadeDuration).SetEase(Ease.OutBack);
            }
            
            // Schedule fade out after display time
            //Sequence sequence = DOTween.Sequence();
            //sequence.AppendInterval(fadeDuration + displayTime);
            //sequence.AppendCallback(() => StartFadeOut());
            
        }
        else if(fadeInOutImmediately)
        {
            fadeStarted = true;
            StartCoroutine(FadeInOut());
        }
    }

    void Update()
    {
        // Check for any input if fade hasn't started yet
        if (!fadeStarted && !fadeInOutImmediately && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            fadeStarted = true;
            StartCoroutine(FadeOut());
        }
    }
    
    private void StartFadeOut()
    {
        // Fade out with DOTween
        tmpText.DOFade(0f, fadeDuration);
        transform.DOMoveY(originalPosition.y + moveDistance, fadeDuration).SetEase(Ease.InQuad);
        
        if(fadeBG != null)
        {
            fadeBG.DOFade(0f, fadeDuration);
        }
        
        // Destroy objects after fade out
        Destroy(gameObject, fadeDuration + 0.1f);
        if (fadeBG != null)
            Destroy(fadeBG.gameObject, fadeDuration + 0.1f);
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
        if (fadeBG != null)
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
        if (fadeBG != null)
            Destroy(fadeBG.gameObject);
    }
    void OnDestroy()
    {
        DOTween.Kill(transform);
        DOTween.Kill(tmpText);
        if(fadeBG != null){
            DOTween.Kill(fadeBG);
            DOTween.Kill(fadeBG.transform);
        }
    }
}