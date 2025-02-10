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
    public GameObject sourceObjectParent;

    public GameObject tableObject;

    private IFluidSimulation sim;

    private GameObject simObject;

    [Header("score total needed for victory")]
    public int totalTargetHitsNeeded = 10;
    public int targetHits = 0;

    [Header("Source nozzle control")]
    public float sourcePlayerModulationStrength = 11;
    public float sourceAcceleration = 0.02F;
    public float sourceVelocity = 1;
    public float maxSourceJitter;
    public float maxSourceOffset;
    public float sourceOffset;
    public float nozzleStrength = 25F;
    public float nozzleSpawnRate = 0.1F;
    private SourceObjectInitializer source;

    [Header("Source Decay control")]
    [Tooltip("The length of time that must occur between two hits")]
    public float hitTimeOffset = 0.2F;
    public float DecayAcceleration = 1;
    public float DecaySpeed = 1;
    public float maxDecaySpeed = 0.1F;
    public float minDecaySpeed = 1F;
    
    private float timeOfLastHit = 0;
    private float timeOfLastDecay = 0;
    

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

        //update source nozzle
        updateSourceStream();

        //increase time since last decrease
        timeOfLastDecay += Time.deltaTime;

        // Check if fluid detector is above threshold (HIT CONDITION)
        if (fluidDetector.isFluidPresent)
        {
            // only update hit count after hitTimeOffset delay from last hit
            if(Time.time - timeOfLastHit > hitTimeOffset){
                targetHits += 1;
                timeOfLastHit = Time.time;
                timeOfLastDecay = 0;
                DecaySpeed = minDecaySpeed;
            }
            

            // Update background music volume (couldnt get this working so hopefully someone could fix)
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

        
        // if time since last decrease is longer than decayspeed, then decrease targetHits varaible.
        if(timeOfLastDecay > DecaySpeed){
            DecaySpeed -= DecayAcceleration;
            targetHits -= 1;
            timeOfLastDecay = 0;
            if (targetHits < 0){
                targetHits = 0;
            }
            if(DecaySpeed < maxDecaySpeed){
                DecaySpeed = maxDecaySpeed;
            }
                
        }

        // Check if target total reached
        if (targetHits >= totalTargetHitsNeeded)
        {
            TriggerWin();
        }
    }

    void updateSourceStream(){

        //get source object
        source = sim.GetFirstSourceObject();

        //calculate vector from source to mouse position
        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePos = Camera.main.ViewportToWorldPoint(mousePos);
        Vector3 dirToMouse = mousePos - source.transform.position;

        //can apply random jitter to source velocity if maxSourceJitter is set above 0
        float rand = UnityEngine.Random.Range(-maxSourceJitter, maxSourceJitter);
        
        //apply acceleration to source modulation
        sourceVelocity += -1 * Math.Sign(sourceOffset) * sourceAcceleration;
        sourceVelocity += -1 * Math.Sign(sourceOffset) * rand;

        //update source offset amount
        sourceOffset += sourceVelocity;

        //enforce max offset rule
        if(Math.Abs(sourceOffset) > maxSourceOffset){
            sourceOffset = Math.Sign(sourceOffset) * maxSourceOffset;
        }

        //calculate nozzle angle from vector and apply modulation offset
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, -55f +90f, 55f +90f);
        sourceObjectParent.transform.rotation = Quaternion.Euler(0f, 0f, (angle - 90F)*0.75f);
        tableObject.transform.rotation = Quaternion.Euler(0f, 0f, (angle - 90F)*0.25f);
        angle += sourceOffset;

        //nozzle enable only on left mouse down
        if(Input.GetMouseButton(0))
            source.spawnRate = nozzleSpawnRate;
        else
            source.spawnRate = 0;

        // convert back to vector and apply nozzle strength
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        direction *= nozzleStrength;

        //update source rotation and nozzle velocity
        source.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90F);
        source.velo.x = direction.x;
        source.velo.y = direction.y;

        //set source object
        sim.SetFirstSourceObject(source);
    }


}
