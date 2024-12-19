using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelectedButton; // Assign this in inspector

    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    private bool isPaused = false;

    private void Start()
    {
        // Ensure menus are closed at start
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                // If settings panel is open, close it and show pause menu
                ShowPauseMenu();
            }
            else
            {
                // Toggle pause state
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;
        PlayButtonSound();

         // Select the default button when pause menu opens
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f; // Unfreeze the game
        isPaused = false;
        PlayButtonSound();

        // Deselect all UI elements
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShowSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        PlayButtonSound();
    }

    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        PlayButtonSound();
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        PlayButtonSound();
        SceneManager.LoadScene("Main Menu"); // Make sure your main menu scene is named "MainMenu"
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void OnDestroy()
    {
        // Ensure time scale is reset when script is destroyed
        Time.timeScale = 1f;
    }
}