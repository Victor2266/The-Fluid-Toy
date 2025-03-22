using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Level3Manager : LevelManager
{
    [Header("Level References")]
    private IFluidSimulation sim;
    private GameObject simObject;

    public FluidDetector smokeAlarm;
    public AudioSource smokeAlarmSound;
    public float smokeSoundDuration = 5F;
    public float smokeSoundVolume = 0.2F;
    private bool soundDisabled = false;
    private float smokeDetectedTime;
    private bool smokeDetected;

    // Start is called before the first frame update
    void Start()
    {
        if(smokeAlarm == null || smokeAlarmSound == null){
            soundDisabled = true;
        }
        simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();
    }

    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void FixedUpdate()
    {
        if (hasWon) return;
        timer += Time.deltaTime;
        if(smokeAlarm.isFluidPresent && !smokeDetected){
            smokeDetected = true;
            smokeDetectedTime = Time.time;
            smokeAlarmSound.volume = smokeSoundVolume;
            smokeAlarmSound.Play();
        }
        toggleSmokeSound();

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
            backgroundMusic.volume = 0.1F;
            TriggerWin();
    }

    void toggleSmokeSound(){
        if(!smokeDetected || !smokeAlarmSound.isPlaying) return;

        if(Time.time - smokeDetectedTime > smokeSoundDuration){
            smokeAlarmSound.Stop();
        }
    }
    void OnDestroy()
    {
        
    }
}