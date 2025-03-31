using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class DrinkMixLevelManager : LevelManager
{
    [Header("Level References")]
    private IFluidSimulation sim;
    private GameObject simObject;
    
    [Header("Smoke alarm sound settings")]
    public float smokeSoundDuration = 5F;
    public float smokeSoundVolume = 0.2F;
    private bool soundDisabled = false;
    private float smokeDetectedTime;
    private bool smokeDetected;

    void Start()
    {
        simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();
    }

    void FixedUpdate()
    {
        if (hasWon) return;
        timer += Time.deltaTime;
    }

    public void buttonWin(){
            backgroundMusic.volume = 0.1F;
            TriggerWin();
    }
}