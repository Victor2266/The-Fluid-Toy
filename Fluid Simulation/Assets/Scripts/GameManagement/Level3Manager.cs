using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Level3Manager : LevelManager
{
    [Header("Level References")]
    public FluidDetector fluidDetector1;
    public FluidDetector fluidDetector2;
    private IFluidSimulation sim;
    private GameObject simObject;

    [Header("Audio Source")]
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private AudioSource barAudioSource;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private AudioSource ambientSFXAudioSource;
    public bool buttonEnabled = false;
    public float timeToEnable = 10.0F;
    public bool enableWin = false;
    private float TTL;

    // Start is called before the first frame update
    void Start()
    {
        simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();
        if (fluidDetector1 == null || fluidDetector2 == null) // Auto-find references if not assigned in inspector on start
        {
            
            Debug.LogError("No FluidDetector connected to level manager");
            enabled = false;
            return;
        }
        TTL = timeToEnable;
    }

    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void FixedUpdate()
    {
        if (hasWon) return;
        timer += Time.deltaTime;
        holdTimer = 0;
        if (!buttonEnabled){
            if (fluidDetector1.isFluidPresent){
                if(TTL <= 0){
                    buttonEnabled = true;
                }else{
                    TTL -= Time.deltaTime;
                }
            }else{
                TTL = timeToEnable;
            }
        }else{
            if(!fluidDetector2.isFluidPresent){
                enableWin = true;
            }
        }


            // // Update background music volume (fixed)
            // if (backgroundMusic != null)
            // {
            //     float percentageComplete = (float)targetHits / (float)totalTargetHitsNeeded;
            //     float fadeStartThreshold = 0.75f;

            //     if (percentageComplete >= fadeStartThreshold)
            //     {
            //         float fadeProgress = (percentageComplete - fadeStartThreshold) / (1f - fadeStartThreshold);
            //         fadeProgress = Mathf.Clamp01(fadeProgress);
            //         backgroundMusic.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
            //         ambientSFXAudioSource.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
            //     }
            // }
    }

    public void buttonWin(){
        if(enableWin){
            hasWon = true;
        }
    }

	private AudioClip GetRandomSound(List<AudioClip> soundList)
    {
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("No sound clips assigned to the list!");
            return null;
        }

        int randomIndex = Random.Range(0, soundList.Count);
        AudioClip randomClip = soundList[randomIndex];

        if (randomClip == null)
        {
            Debug.LogWarning("Null audio clip found in the list!");
        }

        return randomClip;
    }

    void OnDestroy()
    {
        
    }
}