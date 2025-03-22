using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleLevelManager : LevelManager
{
    [Header("Level References")]
    private IFluidSimulation sim;
    private GameObject simObject;
    public FluidDetector smokeAlarm;
    public AudioSource smokeAlarmSound;
    
    [Header("Smoke alarm sound settings")]
    public float smokeSoundDuration = 5F;
    public float smokeSoundVolume = 0.2F;
    private bool soundDisabled = false;
    private float smokeDetectedTime;
    private bool smokeDetected;

    void Start()
    {
        if(smokeAlarm == null || smokeAlarmSound == null){
            soundDisabled = true;
        }
        simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();
    }

    void FixedUpdate()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        //Smoke Detector sound effect handled by level manager
        if(!soundDisabled && smokeAlarm.isFluidPresent && !smokeDetected){
            smokeDetected = true;
            smokeDetectedTime = Time.time;
            smokeAlarmSound.volume = smokeSoundVolume;
            smokeAlarmSound.Play();
        }
        //check and update smoke sound on fixed update
        toggleSmokeSound();

    }

    /// <summary>
    /// called by win button to trigger level win animation, sets bgm volume to 0.1
    /// </summary>
    public void buttonWin(){
            backgroundMusic.volume = 0.1F;
            TriggerWin();
    }

    /// <summary>
    /// Toggles smoke sound effect off based on smokeSoundDuration
    /// </summary>
    void toggleSmokeSound(){
        if(soundDisabled) return;
        if(!smokeDetected || !smokeAlarmSound.isPlaying) return;

        if(Time.time - smokeDetectedTime > smokeSoundDuration){
            smokeAlarmSound.Stop();
        }
    }
}