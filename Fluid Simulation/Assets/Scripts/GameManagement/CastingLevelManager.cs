using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CastingLevelManager : LevelManager
{
    [Header("Level References")]

    public CastLevelSwitch castSwitch;
    public FluidDetector crucibleFailSensor;
    public CastLevelCastMovement castLeft;
    public ThermalSensor swordDetector;
    public FluidDetector[] gradingDetectors;
    public AudioSource steamSound;
    public float steamSoundVolume = 0.5F;
    public float tempSoundThreshold = 40;
    public OrthographicCameraAdjuster cameraAdjuster;
    public GameObject swordHaloEffect;

    [Header("Public Variables")]
    public bool fall = false;
    public float winDelay = 3F;

    public bool finishedCooling = false;
    public float timeToDeleteBasinLid = 2F;

    private bool steamSoundPlayed = false;

    private bool isWinSet = false;

    private float winDelayStart = 0;
    
    public bool hasFailed = false;

    // void Start()
    // {
        
    // }

    void FixedUpdate()
    {
        if(hasWon || hasFailed) return;

        if(isWinSet){
            if(winDelayStart == 0){
                winDelayStart = Time.time;
            }
            else if(Time.time - winDelayStart >= winDelay) {
                TriggerWin();
            }
            return;
        }

        if(castLeft.isOpened && !fall)
        {
            // evaluateScore();
            fall = true;
            
        }

        if(!steamSoundPlayed && swordDetector.currentTemperature >= tempSoundThreshold){
            if(steamSound != null){
                steamSound.volume = steamSoundVolume;
                steamSound.Play();
                steamSoundPlayed = true;
            }
        }
        if(fall && swordDetector.metThreshold && !finishedCooling)
        {
            finishedCooling = true;

            DOTween.Sequence().PrependInterval(3).OnComplete(() => {
                swordHaloEffect.SetActive(true);
                cameraAdjuster.enabled = false;
                Sequence cameraSequence = DOTween.Sequence();
                cameraSequence.Append(Camera.main.transform.DORotate(new Vector3(0, 0, 180+360), 2f, RotateMode.WorldAxisAdd).SetEase(Ease.InOutQuad));
                cameraSequence.Join(Camera.main.DOOrthoSize(6, 2f).SetEase(Ease.InOutBack));
                evaluateScore();
                }); // Show the halo effect after 3s
        }
    }

	public void setWin()
    {
        isWinSet = true;
    }
	void evaluateScore()
    {
        float mark = 0;
        foreach (FluidDetector grade in gradingDetectors){
            mark += grade.currentDensity;
        }
        mark = mark / gradingDetectors.Length;
        mark = 100 - mark;
        Debug.Log("Mark = " + mark);
        timer = mark;
    }

    /// <summary>
    /// Remaps input value with range sourceFrom to SourceTo, to a value in range targetFrom to targetTo.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceFrom"></param>
    /// <param name="sourceTo"></param>
    /// <param name="targetFrom"></param>
    /// <param name="targetTo"></param>
    /// <returns>source value remapped to new limits.</returns>
    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }
}