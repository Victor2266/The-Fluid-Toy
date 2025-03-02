using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using DG.Tweening;

public class SideBarWrapper : MonoBehaviour
{
    [Header("This script handles the references and function calls for each of the sidebar buttons.\n This reduces the amount of drag and drops needed for managing the UI.\n")]

    [Header("Function References")]
    [SerializeField] PauseMenuManager pauseMenuManager;
    private GameObject simulation2DGameObject;
    [SerializeField] AudioSource audioSource;

    [Header("Panel References")]
    [SerializeField] GameObject simSettingsPanel;
    [SerializeField] GameObject informationPanel;
    [SerializeField] GameObject bottomBarParent;

    [Header("Tooltip and Icon References")]
    [SerializeField] TooltipTrigger HideBottomMenuTooltip;
    [SerializeField] Image PlayPauseSidebarIcon;
    [SerializeField] Image PlayPauseSidebarBG;
    [SerializeField] Image HideBottombarIcon;
    [SerializeField] Image HideBottombarBG;
    [SerializeField] Sprite PauseIconImage;
    [SerializeField] Sprite PlayIconImage;

    private IFluidSimulation simulation2DScript;

    void Awake()
    {
        // if the simulation object reference is not set, try to get it by tag
        if (simulation2DGameObject == null)
        {
            simulation2DGameObject = GameObject.FindGameObjectWithTag("Simulation");
        }
        // Get the interface implementation from the simulation object
        if (simulation2DGameObject != null)
        {
            simulation2DScript = simulation2DGameObject.GetComponent<IFluidSimulation>();
            if (simulation2DScript == null)
            {
                Debug.LogError("No IFluidSimulation implementation found on the simulation object!");
            }
        }
        else
        {
            Debug.LogError("Simulation object reference is missing!");
        }
    }

    public void PauseGame()
    {
        pauseMenuManager.PauseGame();
    }
    public void ShowSimulationSettings()
    {
        simSettingsPanel.SetActive(true);
        audioSource.Play();
    }
    public void TogglePauseFluidSimulation()
    {
        simulation2DScript.togglePause();
    }
    public void stepFluidSimulation()
    {
        simulation2DScript.stepSimulation();
    }
    public void resetFluidSimulation()
    {
        simulation2DScript.resetSimulation();
        audioSource.Play();
        UpdatePauseIcon();
    }
    public void ShowInformationPanel()
    {
        informationPanel.SetActive(true);
        audioSource.Play();
    }

    public void UpdatePauseIcon()
    {
        audioSource.Play();
        if (simulation2DScript.getPaused())
        {
            PlayPauseSidebarIcon.sprite = PlayIconImage;
            PlayPauseSidebarBG.color = new Color(0.7058824f, 0.624576f, 0.1215686f);
        }
        else
        {
            PlayPauseSidebarIcon.sprite = PauseIconImage;
            PlayPauseSidebarBG.color = new Color(0, 0, 0, 255);
        }
    }

    public void ReloadScene()
    {
        // Get current scene info
        Scene currentScene = SceneManager.GetActiveScene();
        // We should probably check to see that this async operation is done, but whatever.
        SceneManager.LoadSceneAsync(currentScene.buildIndex);
    }

    public void ToggleShowBottomBar()
    {
        // Play audio feedback
        audioSource.Play();

        if (bottomBarParent.activeSelf)
        {
            // If bottom bar is visible, slide it down and deactivate
            RectTransform bottomBarRect = bottomBarParent.GetComponent<RectTransform>();
            RectTransform HideBottombarIconRect = HideBottombarIcon.GetComponent<RectTransform>();

            // Calculate the distance to move (the height of the bottom bar)
            float slideDistance = bottomBarRect.rect.height;

            // Animate the bar sliding down
            bottomBarRect.DOAnchorPosY(-slideDistance, 0.25f)
                .SetEase(DG.Tweening.Ease.OutQuint)
                .OnComplete(() =>
                {
                    // Deactivate the bottom bar after animation completes
                    bottomBarParent.SetActive(false);
                });

            // Animate the icon rotating
            HideBottombarBG.color = new Color(0.7058824f, 0.624576f, 0.1215686f);
            HideBottombarIconRect.DORotate(new Vector3(0, 0, 180), 0.5f)
                .SetEase(DG.Tweening.Ease.OutQuint);

            HideBottomMenuTooltip.SetTooltipContent("Show Bottom Bar");
        }
        else
        {
            // If bottom bar is hidden, activate it and slide it up
            bottomBarParent.SetActive(true);

            RectTransform bottomBarRect = bottomBarParent.GetComponent<RectTransform>();
            RectTransform HideBottombarIconRect = HideBottombarIcon.GetComponent<RectTransform>();

            // Get current position
            Vector2 currentPos = bottomBarRect.anchoredPosition;

            // Calculate the target position (where the bar should end up)
            float targetY = 0f;

            // Set initial position off-screen (below view)
            bottomBarRect.anchoredPosition = new Vector2(currentPos.x, -bottomBarRect.rect.height);

            // Animate the bar sliding up
            bottomBarRect.DOAnchorPosY(targetY, 0.25f)
                .SetEase(DG.Tweening.Ease.OutBack) // Adds a slight bounce effect
                .SetDelay(0.1f); // Small delay for better feel

            // Animate the icon rotating
            HideBottombarBG.color = new Color(0f, 0f, 0f, 1f);
            HideBottombarIconRect.DORotate(new Vector3(0, 0, 0), 0.5f)
                .SetEase(DG.Tweening.Ease.OutQuint);

            HideBottomMenuTooltip.SetTooltipContent("Hide Bottom Bar");
        }
    }

    void OnDestroy()
    {
        RectTransform bottomBarRect = bottomBarParent.GetComponent<RectTransform>();
        DOTween.Kill(bottomBarRect);
        DOTween.Kill(HideBottombarIcon.GetComponent<RectTransform>());
    }
}
