using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public float requiredHoldTime = 5f;
    
    [Header("References")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioSource backgroundMusic;
    
    [Header("Audio Settings")]
    [SerializeField] protected float initialMusicVolume = 0.5f;
    [Range(0f, 1f)]
    public float fadeOutStartTime = 0f; // When to start fading (as percentage of hold time)
    
    [Header("Debug")]
    public bool showDebugTimer = true;
    
    protected float holdTimer = 0f;
    protected bool isHolding = false;
    protected bool hasWon = false;
    protected float lastMouseInputTime;

    public LevelCompleteAnimation levelCompleteAnim;

    [SerializeField] protected float timer = 0;
    [SerializeField] protected float threeStarTime = 0;
    [SerializeField] protected float twoStarTime = 0;

    void Start()
    {
        // Store initial background music volume
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = initialMusicVolume;
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

    // Extend this class and implement the update loop for each individual level
    /*
    void Update()
    {

    }*/

    protected void ResetHoldTimer()
    {
        holdTimer = 0f;
        isHolding = false;
        
        // Reset background music volume
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = initialMusicVolume;
        }
    }

    protected void TriggerWin()
    {
        if (hasWon) return;
        
        hasWon = true;

        // Save level completion
        PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);

        // Save level score
        int savedScore = PlayerPrefs.GetInt($"Level_{currentLevel}_Score", 0);

        if (timer < threeStarTime)
        {
            int setScore = Mathf.Max(savedScore, 3);
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", setScore);
            levelCompleteAnim.setCurrentScore(3);
        }
        else if (timer < twoStarTime)
        {
            int setScore = Mathf.Max(savedScore, 2);
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", setScore);
            levelCompleteAnim.setCurrentScore(2);
        }
        else
        {
            int setScore = Mathf.Max(savedScore, 1);
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", setScore);
            levelCompleteAnim.setCurrentScore(1);
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

    protected IEnumerator WinSequence()
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
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single); // Uncomment and modify as needed
    }

    protected void OnGUI()
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