using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Level4Manager : LevelManager
{
    [Header("Level References")]
    public FluidDetector[] fluidDetectors;
    public GameObject engineBlockParent;
    public GameObject pistonObject;
    public GameObject sparkPlugObject;
    public GameObject valveIntakeObject;
    public GameObject valveExhaustObject;

    private IFluidSimulation sim;
    private GameObject simObject;

    [Header("Audio Source")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private AudioSource ignitionAudioSource;
    [SerializeField] private AudioSource ambientSFXAudioSource;

    [Header("Engine Sounds")]
    [SerializeField] private List<AudioClip> engineSounds = new List<AudioClip>();
    [SerializeField] private AudioClip ignitionSound;
    [SerializeField] private float engineMinPitch = 0.8f;
    [SerializeField] private float engineMaxPitch = 1.2f;
    [SerializeField] private float engineVolume = 1f;
    private AudioClip currentEngineSound;

    [Header("Engine Cycle")]
    public float cycleSpeed = 1.0f;
    public float pistonTravelDistance = 2.0f;
    public float valveOpenAmount = 0.5f;
    private float cycleTimer = 0f;
    private int currentCycleStep = 0;
    private bool isRunning = false;
    private Vector3 pistonStartPosition;
    private Vector3 intakeValveStartPosition;
    private Vector3 exhaustValveStartPosition;
    private Vector3 sparkPlugStartPosition;

    [Header("Fluid Control")]
    public float fuelSpawnRate = 0.1f;
    public float exhaustRate = 0.05f;
    public float sparkPlugTemperature = 1800f;
    private SourceObjectInitializer fuelSource;
    private SourceObjectInitializer exhaustSource;

    [Header("Win Conditions")]
    public int totalCyclesNeeded = 5;
    public int completedCycles = 0;

    // Start is called before the first frame update
    void Start()
    {
        simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();

        // Store initial positions
        if (pistonObject != null)
            pistonStartPosition = pistonObject.transform.position;

        if (valveIntakeObject != null)
            intakeValveStartPosition = valveIntakeObject.transform.position;

        if (valveExhaustObject != null)
            exhaustValveStartPosition = valveExhaustObject.transform.position;

        if (sparkPlugObject != null)
            sparkPlugStartPosition = sparkPlugObject.transform.position;

        // Auto-find references if not assigned in inspector
        if (fluidDetectors == null || fluidDetectors.Length == 0)
        {
            fluidDetectors = FindObjectsOfType<FluidDetector>();
            if (fluidDetectors == null || fluidDetectors.Length == 0)
            {
                Debug.LogError("No FluidDetectors found in the scene!");
                enabled = false;
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        // Engine cycle logic
        if (Input.GetKeyDown(KeyCode.Space) && !isRunning)
        {
            StartEngine();
        }

        if (isRunning)
        {
            UpdateEngineCycle();
        }

        // Check win condition
        if (completedCycles >= totalCyclesNeeded)
        {
            TriggerWin();
            if (engineAudioSource != null)
                engineAudioSource.Stop();
        }
    }

    void StartEngine()
    {
        isRunning = true;
        cycleTimer = 0f;
        currentCycleStep = 0;

        // Play engine sound
        if (engineAudioSource != null && engineSounds.Count > 0)
        {
            currentEngineSound = GetRandomSound(engineSounds);
            if (currentEngineSound != null)
            {
                engineAudioSource.loop = true;
                engineAudioSource.clip = currentEngineSound;
                engineAudioSource.pitch = Random.Range(engineMinPitch, engineMaxPitch);
                engineAudioSource.volume = engineVolume;
                engineAudioSource.Play();
            }
        }
    }

    void UpdateEngineCycle()
    {
        cycleTimer += Time.deltaTime * cycleSpeed;

        // Four-stroke engine cycle: Intake, Compression, Power, Exhaust
        switch (currentCycleStep)
        {
            case 0: // Intake stroke
                // Move piston down, open intake valve
                if (pistonObject != null)
                {
                    float t = Mathf.Clamp01(cycleTimer / 1.0f);
                    pistonObject.transform.position = pistonStartPosition + Vector3.down * pistonTravelDistance * t;
                }

                if (valveIntakeObject != null)
                {
                    float t = Mathf.Clamp01(cycleTimer / 0.5f);
                    if (t <= 0.5f)
                        valveIntakeObject.transform.position = intakeValveStartPosition + Vector3.up * valveOpenAmount * (t * 2);
                    else
                        valveIntakeObject.transform.position = intakeValveStartPosition + Vector3.up * valveOpenAmount * (2 - t * 2);
                }

                // Spawn fuel
                SpawnFuel(cycleTimer < 0.5f);

                if (cycleTimer >= 1.0f)
                {
                    cycleTimer = 0f;
                    currentCycleStep = 1;
                }
                break;

            case 1: // Compression stroke
                // Move piston up, both valves closed
                if (pistonObject != null)
                {
                    float t = Mathf.Clamp01(cycleTimer / 1.0f);
                    pistonObject.transform.position = pistonStartPosition + Vector3.down * pistonTravelDistance * (1 - t);
                }

                if (cycleTimer >= 1.0f)
                {
                    cycleTimer = 0f;
                    currentCycleStep = 2;

                    // Trigger ignition
                    if (ignitionAudioSource != null && ignitionSound != null)
                    {
                        ignitionAudioSource.PlayOneShot(ignitionSound);
                    }

                    // Heat up spark plug to ignite fuel
                    if (sparkPlugObject != null)
                    {
                        sparkPlugObject.transform.DOShakePosition(0.2f, 0.1f, fadeOut: true);
                    }
                }
                break;

            case 2: // Power stroke
                // Move piston down, ignite fuel
                if (pistonObject != null)
                {
                    float t = Mathf.Clamp01(cycleTimer / 1.0f);
                    pistonObject.transform.position = pistonStartPosition + Vector3.down * pistonTravelDistance * t;

                    // Apply force to piston based on explosion
                    if (t < 0.2f)
                    {
                        pistonObject.transform.DOShakePosition(0.1f, 0.05f * (1 - t * 5), fadeOut: true);
                    }
                }

                if (cycleTimer >= 1.0f)
                {
                    cycleTimer = 0f;
                    currentCycleStep = 3;
                }
                break;

            case 3: // Exhaust stroke
                // Move piston up, open exhaust valve
                if (pistonObject != null)
                {
                    float t = Mathf.Clamp01(cycleTimer / 1.0f);
                    pistonObject.transform.position = pistonStartPosition + Vector3.down * pistonTravelDistance * (1 - t);
                }

                if (valveExhaustObject != null)
                {
                    float t = Mathf.Clamp01(cycleTimer / 0.5f);
                    if (t <= 0.5f)
                        valveExhaustObject.transform.position = exhaustValveStartPosition + Vector3.up * valveOpenAmount * (t * 2);
                    else
                        valveExhaustObject.transform.position = exhaustValveStartPosition + Vector3.up * valveOpenAmount * (2 - t * 2);
                }

                // Spawn exhaust
                SpawnExhaust(cycleTimer < 0.7f);

                if (cycleTimer >= 1.0f)
                {
                    cycleTimer = 0f;
                    currentCycleStep = 0;
                    completedCycles++;

                    // Update background music volume
                    if (backgroundMusic != null)
                    {
                        float percentageComplete = (float)completedCycles / (float)totalCyclesNeeded;
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
                break;
        }
    }

    void SpawnFuel(bool isActive)
    {
        // Get fuel source object
        fuelSource = sim.GetFirstSourceObject(); // Using the first source object for fuel

        if (fuelSource.transform != null)
        {
            // Set spawn rate based on active state
            fuelSource.spawnRate = isActive ? fuelSpawnRate : 0f;

            // Update source in simulation
            sim.SetFirstSourceObject(fuelSource);
        }
    }

    void SpawnExhaust(bool isActive)
    {
        // For exhaust, we'll modify the same source object
        // In a real implementation, you would need to create multiple source objects in the scene
        // and access them through other means (like finding them by name or tag)
        fuelSource = sim.GetFirstSourceObject();

        if (fuelSource.transform != null && !isActive)
        {
            // When fuel is not active but exhaust is, change the source to emit exhaust
            float originalSpawnRate = fuelSource.spawnRate;
            fuelSource.spawnRate = isActive ? exhaustRate : 0f;

            // Update source in simulation
            sim.SetFirstSourceObject(fuelSource);
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
        if (pistonObject != null)
            pistonObject.transform.DOKill();

        if (sparkPlugObject != null)
            sparkPlugObject.transform.DOKill();
    }
}
