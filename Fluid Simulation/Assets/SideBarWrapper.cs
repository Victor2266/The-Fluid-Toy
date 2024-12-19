using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBarWrapper : MonoBehaviour
{
    [Header("This script handles the references and function calls for each of the sidebar buttons.\n This reduces the amount of drag and drops needed for managing the UI.\n")]

    [SerializeField] PauseMenuManager pauseMenuManager;
    [SerializeField] GameObject simSettingsPanel;
    [SerializeField] Simulation2D simulation2DScript;
    [SerializeField] GameObject informationPanel;

    [SerializeField] AudioSource audioSource;

    public void PauseGame()
    {
        pauseMenuManager.PauseGame();
    }
    public void ShowSimulationSettings(){
        simSettingsPanel.SetActive(true);
        audioSource.Play();
    }
    public void TogglePauseFluidSimulation(){
        simulation2DScript.togglePause();
        audioSource.Play();
    }
    public void stepFluidSimulation(){
        simulation2DScript.stepSimulation();
        audioSource.Play();
    }
    public void resetFluidSimulation(){
        simulation2DScript.resetSimulation();
        audioSource.Play();
    }
    public void ShowInformationPanel(){
        informationPanel.SetActive(true);
        audioSource.Play();
    }


}
