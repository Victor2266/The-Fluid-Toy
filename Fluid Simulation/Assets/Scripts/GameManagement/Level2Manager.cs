using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Level2Manager : LevelManager
{
    [Header("Level References")]
    public FluidDetector fluidDetector;
    public GameObject sourceObjectParent;

    public GameObject tableObject;

    private IFluidSimulation sim;

    private GameObject simObject;

    [Header("Audio Source")]
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private AudioSource barAudioSource;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private AudioSource ambientSFXAudioSource;

    [Header("Left Click Sounds")]
    [SerializeField] private List<AudioClip> leftClickSounds = new List<AudioClip>();
    [SerializeField] private AudioClip refreshSound;
    [SerializeField] private AudioClip overheatSound;
    [SerializeField] private float leftClickMinPitch = 0.95f;
    [SerializeField] private float leftClickMaxPitch = 1.05f;
    [SerializeField] private float leftClickVolume = 1f;
    private AudioClip currentLeftClickSound;

    [Header("Overheat System")]
    public float maxHeatLevel = 5f;        // Maximum time in seconds before overheating
    public float currentHeatLevel = 5f;     // Current heat level
    public float coolingTime = 3f;         // Time needed to cool down when fully overheated
    public float heatingRate = 1f;         // How fast the source heats up when firing
    public float coolingRate = 1f;         // How fast the source cools down when not firing
    private bool isOverheated = false;     // Track if the source is currently overheated
    private float overheatedTimer = 0f;    // Timer for tracking cooldown period

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

    public float rotationSpeed = 5f;  // Controls how fast the rotation occurs
    private float currentAngle = 0f;  // Store the current rotation angle, initialize to vertical
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
            if (Time.time - timeOfLastHit > hitTimeOffset)
            {
                targetHits += 1;
                targetAudioSource.PlayOneShot(targetAudioSource.clip, 1f);
                timeOfLastHit = Time.time;
                timeOfLastDecay = 0;
                DecaySpeed = minDecaySpeed;
            }


            // Update background music volume (fixed)
            if (backgroundMusic != null)
            {
                float percentageComplete = (float)targetHits / (float)totalTargetHitsNeeded;
                float fadeStartThreshold = 0.75f;

                if (percentageComplete >= fadeStartThreshold)
                {
                    float fadeProgress = (percentageComplete - fadeStartThreshold) / (1f - fadeStartThreshold);
                    fadeProgress = Mathf.Clamp01(fadeProgress);
                    backgroundMusic.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
                    ambientSFXAudioSource.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
                }
            }
        }


        // if time since last decrease is longer than decayspeed, then decrease targetHits variable.
        if (timeOfLastDecay > DecaySpeed)
        {
            DecaySpeed = Mathf.Max(DecaySpeed - DecayAcceleration, maxDecaySpeed);
            targetHits = Mathf.Max(targetHits - 1, 0);
            timeOfLastDecay = 0;

        }

        // Check if target total reached
        if (targetHits >= totalTargetHitsNeeded)
        {
            TriggerWin();
            barAudioSource.Stop();
            gunAudioSource.Stop();
        }

        // Handle left click sound
        if (isOverheated || Input.GetMouseButtonUp(0) && currentLeftClickSound != null)
        {
            if (gunAudioSource.clip == currentLeftClickSound)
            {
                gunAudioSource.Stop();
                // Play overheated sound
                if (isOverheated && overheatSound != null && barAudioSource != null)
                {
                    barAudioSource.PlayOneShot(overheatSound);
                }
                gunAudioSource.loop = false;
                currentLeftClickSound = null;
            }
        }
    }

    void updateSourceStream()
    {

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
        if (Math.Abs(sourceOffset) > maxSourceOffset)
        {
            sourceOffset = Math.Sign(sourceOffset) * maxSourceOffset;
        }

        // Calculate target angle from mouse position
        float targetAngle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        targetAngle = Mathf.Clamp(targetAngle, -55f + 90f, 55f + 90f);

        // Smoothly interpolate current angle towards target angle
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

        // Set the rotation of the gun and table
        sourceObjectParent.transform.rotation = Quaternion.Euler(0f, 0f, (currentAngle - 90F) * 0.75f);
        tableObject.transform.rotation = Quaternion.Euler(0f, 0f, (currentAngle - 90F) * 0.35f);

        //Handle overheat and nozzle control
        if (Input.GetMouseButton(0) && !isOverheated)
        {
            if (gunAudioSource.isPlaying == false)
            {
                // Start continuous sound
                currentLeftClickSound = GetRandomSound(leftClickSounds);
                if (currentLeftClickSound != null)
                {
                    gunAudioSource.loop = true;
                    gunAudioSource.clip = currentLeftClickSound;
                    gunAudioSource.pitch = Random.Range(leftClickMinPitch, leftClickMaxPitch);
                    gunAudioSource.volume = leftClickVolume;
                    gunAudioSource.Play();
                }
            }
            // Decrease heat level while firing
            currentHeatLevel -= Time.deltaTime * heatingRate;
            source.spawnRate = nozzleSpawnRate;

            // Check if fully overheated
            if (currentHeatLevel < 0)
            {
                currentHeatLevel = 0;
                isOverheated = true;

                overheatedTimer = 0f;
                source.spawnRate = 0;
            }
        }
        else
        {
            source.spawnRate = 0;

            // Handle cooling
            if (isOverheated)
            {
                // Track cooldown period
                overheatedTimer += Time.deltaTime;
                if (overheatedTimer >= coolingTime)
                {
                    // Reset after full cooldown
                    isOverheated = false;
                    // Play refreshSound sound
                    if (refreshSound != null && barAudioSource != null)
                    {
                        barAudioSource.PlayOneShot(refreshSound);
                    }
                    currentHeatLevel = maxHeatLevel;
                }
            }
            else
            {
                // Normal cooling when not overheated
                currentHeatLevel = Mathf.Min(currentHeatLevel + Time.deltaTime * coolingRate, maxHeatLevel);
            }
        }

        // Add source offset to target angle (not current angle)
        targetAngle = currentAngle + sourceOffset;
        // convert back to vector and apply nozzle strength
        Vector3 direction = new Vector3(Mathf.Cos(targetAngle * Mathf.Deg2Rad), Mathf.Sin(targetAngle * Mathf.Deg2Rad), 0);
        direction *= nozzleStrength;

        //update source rotation and nozzle velocity
        source.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle - 90F); // This doesn't do anything since the rotation of the source isn't used by any script
        source.velo.x = direction.x;
        source.velo.y = direction.y;

        //set source object
        sim.SetFirstSourceObject(source);
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
}