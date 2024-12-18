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
    private int numberOfLevels;

    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    
    private void Start()
    {
        numberOfLevels = SettingsManager.NumberOfLevels;

        // Initialize the menu state
        ShowMainMenu();
        
        // Generate level select buttons
        GenerateLevelButtons();
        
        levelButtonContainer.GetComponent<LevelSelectButtonAnimator>().InitializeButtons();

        // Load and apply saved settings
        LoadSettings();
    }

    private List<Button> levelButtons = new List<Button>(); // Add this field to store references to buttons

    private void GenerateLevelButtons()
    {
        for (int i = 1; i <= numberOfLevels; i++)
        {
            int levelNumber = i;
            Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
            levelButtons.Add(levelButton); // Store reference to button

            TextMeshProUGUI buttonText = levelButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = $"Level {levelNumber}";

            levelButton.onClick.AddListener(() => LoadLevel(levelNumber));

            UpdateButtonLockStatus(levelButton, buttonText, levelNumber);
        }
    }

    private void UpdateButtonLockStatus(Button button, TextMeshProUGUI buttonText, int levelNumber)
    {
        bool isUnlocked = PlayerPrefs.GetInt($"Level_{levelNumber}_Unlocked", levelNumber == 1 ? 1 : 0) == 1;
        button.interactable = isUnlocked;

        if (!isUnlocked)
        {
            buttonText.text = "LOCKED";
            RectTransform rectTransform = buttonText.GetComponent<RectTransform>();
            float currentLeft = rectTransform.offsetMin.x;
            float currentRight = rectTransform.offsetMax.x;
            rectTransform.offsetMin = new Vector2(currentLeft, -10);
            rectTransform.offsetMax = new Vector2(currentRight, -10);
        }
        else
        {
            buttonText.text = $"Level {levelNumber}";
            RectTransform rectTransform = buttonText.GetComponent<RectTransform>();
            float currentLeft = rectTransform.offsetMin.x;
            float currentRight = rectTransform.offsetMax.x;
            rectTransform.offsetMin = new Vector2(currentLeft, 0);
            rectTransform.offsetMax = new Vector2(currentRight, 0);
        }
        Transform lockIcon = button.transform.Find("Lock Icon");
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(!isUnlocked);
    }

    private void RefreshLevelButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (levelButtons[i] != null)
            {
                TextMeshProUGUI buttonText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                UpdateButtonLockStatus(levelButtons[i], buttonText, i + 1);
            }
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
        RefreshLevelButtons(); // Refresh level buttons lock status
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

    public void LoadSandbox()
    {
        PlayButtonSound();
        // Save current game state if needed
        PlayerPrefs.Save();
        // Load the level scene
        SceneManager.LoadScene($"Sandbox");
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