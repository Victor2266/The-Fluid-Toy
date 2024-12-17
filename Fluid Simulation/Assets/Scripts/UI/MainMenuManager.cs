// MenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Level Select")]
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private int numberOfLevels = 10;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    
    private void Start()
    {
        // Initialize the menu state
        ShowMainMenu();
        
        // Generate level select buttons
        GenerateLevelButtons();
        
        levelButtonContainer.GetComponent<LevelSelectButtonAnimator>().InitializeButtons();

        // Load and apply saved settings
        LoadSettings();
    }
    
    private void GenerateLevelButtons()
    {
        for (int i = 1; i <= numberOfLevels; i++)
        {
            int levelNumber = i; // Capture the level number for the button callback
            Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = levelButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = $"Level {levelNumber}";
            
            // Add click listener
            levelButton.onClick.AddListener(() => LoadLevel(levelNumber));
            
            // Check if level is unlocked
            bool isUnlocked = PlayerPrefs.GetInt($"Level_{levelNumber}_Unlocked", levelNumber == 1 ? 1 : 0) == 1;
            levelButton.interactable = isUnlocked;

            if (!isUnlocked)
                buttonText.text = $"LOCKED";
        }
    }
    
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(false);
        PlayButtonSound();
    }
    
    public void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        settingsPanel.SetActive(false);
        PlayButtonSound();
    }
    
    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(true);
        PlayButtonSound();
    }
    
    public void LoadLevel(int levelNumber)
    {
        PlayButtonSound();
        // Save current game state if needed
        PlayerPrefs.Save();
        // Load the level scene
        SceneManager.LoadScene($"Level_{levelNumber}");
    }
    
    public void QuitGame()
    {
        PlayButtonSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }
    
    private void LoadSettings()
    {
        // Load saved settings (volume, graphics, etc.)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        // Apply settings...
    }
}

// SaveManager.cs
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void UnlockLevel(int levelNumber)
    {
        PlayerPrefs.SetInt($"Level_{levelNumber}_Unlocked", 1);
        PlayerPrefs.Save();
    }
    
    public bool IsLevelUnlocked(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Unlocked", levelNumber == 1 ? 1 : 0) == 1;
    }
}