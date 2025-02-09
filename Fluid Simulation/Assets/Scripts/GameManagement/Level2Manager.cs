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

    public float nozzleStrength = 25F;

    public float nozzleSpawnRate = 0.1F;
    private SourceObjectInitializer source;

    public int targetHits = 0;

    private float timeOfLastHit = 0;

    private float timeOfLastDecrease = 0;

    public float decreaseAcceleration = 1;
    public float decreaseSpeed = 1;

    public float maxDecreaseSpeed = 0.1F;

    public float minDecreaseSpeed = 1F;

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

        // Check if fluid detector is above threshold (HIT CONDITION)
        if (fluidDetector.isFluidPresent)
        {
            if(Time.time - timeOfLastHit > 0.2F){
                targetHits += 1;
                timeOfLastHit = Time.time;
                timeOfLastDecrease = 0;
                decreaseSpeed = minDecreaseSpeed;
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

        timeOfLastDecrease += Time.deltaTime;

        if(timeOfLastDecrease > decreaseSpeed){
            decreaseSpeed -= decreaseAcceleration;
            targetHits -= 1;
            timeOfLastDecrease = 0;
            if (targetHits < 0){
                targetHits = 0;
            }
            if(decreaseSpeed < maxDecreaseSpeed){
                decreaseSpeed = maxDecreaseSpeed;
            }
                
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
        
        sourceVelocity += -1 * Math.Sign(sourceOffset) * sourceAcceleration;
        sourceVelocity += -1 * Math.Sign(sourceOffset) * rand;

        sourceOffset += sourceVelocity;


        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;

        if(Math.Abs(sourceOffset) > maxSourceOffset){
            sourceOffset = Math.Sign(sourceOffset) * maxSourceOffset;
        }

        angle += sourceOffset;

        if(Input.GetMouseButton(0))
            source.spawnRate = nozzleSpawnRate;
        else
            source.spawnRate = 0;

        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        direction *= nozzleStrength;
        source.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90F);

        source.velo.x = direction.x;
        source.velo.y = direction.y;
        sim.SetFirstSourceObject(source);
    }


}
