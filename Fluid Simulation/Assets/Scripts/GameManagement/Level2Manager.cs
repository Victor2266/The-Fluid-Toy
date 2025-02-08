using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Level2Manager : LevelManager
{
    [Header("Level References")]
    public FluidDetector fluidDetector;

    private IFluidSimulation sim;

    private GameObject simObject;

    public int totalTargetHitsNeeded = 10;

    public float sourcePlayerModulationStrength = 11;

    public float sourceAcceleration = 0.02F;
    public float sourceVelocity = 1;

    public float maxSourceJitter;

    public float maxSourceOffset;
    public float sourceOffset;
    private SourceObjectInitializer source;

    public int targetHits = 0;

    private float timeOfLastHit = 0;

    // Start is called before the first frame update
    void Start()
    {

        simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();
        if (fluidDetector == null) // Auto-find references if not assigned in inspector on start
        {
            fluidDetector = FindObjectOfType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in the scene!");
                enabled = false;
                return;
            }
        }
    }
    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        updateSourceStream();

        // Check if fluid detector is above threshold (WIN CONDITION)
        if (fluidDetector.isFluidPresent)
        {
            if(Time.time - timeOfLastHit > 0.2F){
                targetHits += 1;
                timeOfLastHit = Time.time;
            }
            

            // Update background music volume
            if (backgroundMusic != null)
            {
                float fadeStartThreshold = totalTargetHitsNeeded * fadeOutStartTime;
                if (holdTimer >= fadeStartThreshold)
                {
                    float fadeProgress = (targetHits - fadeStartThreshold) / (totalTargetHitsNeeded - fadeStartThreshold);
                    fadeProgress = Mathf.Clamp01(fadeProgress);
                    backgroundMusic.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
                }
            }

            
        }
        else
        {
            ResetHoldTimer();
        }

        // Check if we've held for long enough
        if (targetHits >= totalTargetHitsNeeded)
        {
            TriggerWin();
        }
    }

    void updateSourceStream(){
        source = sim.GetFirstSourceObject();
        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePos = Camera.main.ViewportToWorldPoint(mousePos);
        Vector3 dirToMouse = mousePos - source.transform.position;
        float rand = UnityEngine.Random.Range(-maxSourceJitter, maxSourceJitter);
        source.velo.y = dirToMouse.y * 1.5F;
        source.velo.x = dirToMouse.x;
        
        sourceVelocity += -1 * Math.Sign(sourceOffset) * sourceAcceleration;
        sourceVelocity += -1 * Math.Sign(sourceOffset) * rand;

        sourceOffset += sourceVelocity;

        if(Math.Abs(sourceOffset) > maxSourceOffset){
            sourceOffset = Math.Sign(sourceOffset) * maxSourceOffset;
        }

        source.velo.x += sourceOffset;

        sim.SetFirstSourceObject(source);
    }


}
