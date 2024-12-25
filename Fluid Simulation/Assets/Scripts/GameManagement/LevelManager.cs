using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public float requiredHoldTime = 5f;
    
    [Header("References")]
    public FluidDetector fluidDetector;
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioSource backgroundMusic;
    
    [Header("Audio Settings")]
    [SerializeField] private float initialMusicVolume = 0.5f;
    [Range(0f, 1f)]
    public float fadeOutStartTime = 0f; // When to start fading (as percentage of hold time)
    
    [Header("Debug")]
    public bool showDebugTimer = true;
    
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool hasWon = false;
    private float lastMouseInputTime;

    public LevelCompleteAnimation levelCompleteAnim;

    [SerializeField] private float timer = 0;
    [SerializeField] private float threeStarTime = 0;
    [SerializeField] private float twoStarTime = 0;
    [SerializeField] private float oneStarTime = 0;

    void Start()
    {
        // Store initial background music volume
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = initialMusicVolume;
        }
        
        if (fluidDetector == null)
        {
            fluidDetector = FindObjectOfType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in the scene!");
                enabled = false;
                return;
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Initialize the last mouse input time
        lastMouseInputTime = Time.time;
    }

    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        // Check for any mouse input
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            lastMouseInputTime = Time.time;
            ResetHoldTimer();
            return;
        }

        // Only start counting if we haven't had mouse input for at least 0.5 seconds
        if (Time.time - lastMouseInputTime < 0.5f)
        {
            ResetHoldTimer();
            return;
        }

        // Check if fluid detector is above threshold
        if (fluidDetector.isFluidPresent)
        {
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;
            }

            holdTimer += Time.deltaTime;

            // Update background music volume
            if (backgroundMusic != null)
            {
                float fadeStartThreshold = requiredHoldTime * fadeOutStartTime;
                if (holdTimer >= fadeStartThreshold)
                {
                    float fadeProgress = (holdTimer - fadeStartThreshold) / (requiredHoldTime - fadeStartThreshold);
                    fadeProgress = Mathf.Clamp01(fadeProgress);
                    backgroundMusic.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
                }
            }

            // Check if we've held for long enough
            if (holdTimer >= requiredHoldTime)
            {
                TriggerWin();
            }
        }
        else
        {
            ResetHoldTimer();
        }
    }

    void ResetHoldTimer()
    {
        holdTimer = 0f;
        isHolding = false;
        
        // Reset background music volume
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = initialMusicVolume;
        }
    }

    void TriggerWin()
    {
        if (hasWon) return;
        
        hasWon = true;

        // Save level completion
        PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);

        if (timer < threeStarTime)
        {
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", 3);
        }
        else if (timer < twoStarTime)
        {
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", 2);
        }
        else if (timer < oneStarTime)
        {
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", 1);
        }

        PlayerPrefs.Save();

        // Play win sound
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        // Optional: Start win sequence
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        Debug.Log($"Level {currentLevel} Complete!");

        // Play Win Animation
        levelCompleteAnim.PlayLevelCompleteAnimation();
        
        // Wait for sound to finish if there is one
        if (winSound != null)
        {
            yield return new WaitForSeconds(winSound.length);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // You can add level transition logic here
        SceneManager.LoadScene("Main Menu"); // Uncomment and modify as needed
    }

    void OnGUI()
    {
        if (!showDebugTimer || hasWon) return;

        // Display hold timer when active
        if (isHolding)
        {
            float remainingTime = requiredHoldTime - holdTimer;
            string timerText = $"Wait for: {remainingTime:F1}s";
            GUI.Label(new Rect(Screen.width / 2 - 50, 20, 100, 20), timerText);
        }
    }
}