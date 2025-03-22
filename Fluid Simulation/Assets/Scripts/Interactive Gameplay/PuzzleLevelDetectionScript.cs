using System.Runtime.CompilerServices;
using UnityEngine;

public class PuzzleLevelDetectionScript : MonoBehaviour
{
    [Header("Level References")]
    public IFluidSimulation sim;
    public FluidDetector fluidDetector;

    [Header("Activation sound settings")]
    public AudioSource sound;
    public float targetVolume = 0.3F;

    [Header("Source control settings")]
    public bool disableSourceUpdate = false;
    public int sourceIndex;
    public float activationValue = 0.1F;
    public float activationTime;
    
    private float toggledTime;
    private GameObject simObject;
    private bool activated;
    private SourceObjectInitializer source;
    private bool soundDisabled = false;


    void Start()
    {
        if (sound == null){
            soundDisabled = true;
        }
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
        if(fluidDetector == null && disableSourceUpdate == false){
            Debug.LogError("No fluid detector connected to Detection script");
        }
    }

    /// <summary>
    /// Activates or deactivates fluid source based on fluid detector value.
    /// Deactivation can occur after <param name="activationTime"></param> or never if activationTime set to 0
    /// </summary>
    void FixedUpdate()
    {
        if(!activated){
            if(fluidDetector.isFluidPresent){
                activateSource();
            }
        }else{
            if(activationTime != 0){
                if(Time.time - toggledTime > activationTime){
                    deactivateSource();
                    if(!fluidDetector.isFluidPresent){
                        activated = false;
                    }
                }
                
            }
            
        }
        

    }

    /// <summary>
    /// If disableSourceUpdate is false, acctivates source indexed by sourceIndex with specified spawnrate and starts sound effect.
    /// Else only starts sound effect.
    /// </summary>
    void activateSource(){
        if(disableSourceUpdate){
            startSound();
            return;
        }
        source = sim.GetSourceObject(sourceIndex);
        source.spawnRate = activationValue;
        sim.SetSourceObject(source, sourceIndex);
        activated = true;
        toggledTime = Time.time;
        startSound();
    }

    /// <summary>
    /// If disableSourceUpdate is false, deactivates source indexed by sourceIndex with specified spawnrate and stops sound effect.
    /// Else only stops sound effect.
    /// </summary>
    void deactivateSource(){
        if(disableSourceUpdate){
            stopSound();
            return;
        }
        source = sim.GetSourceObject(sourceIndex);
        source.spawnRate = 0;
        sim.SetSourceObject(source, sourceIndex);
        toggledTime = Time.time;
        stopSound();
    }

    /// <summary>
    /// If soundDisabled is false, starts sound playback at specified volume.
    /// </summary>
    void startSound(){
        if(soundDisabled) return;

        sound.volume = targetVolume;
        sound.Play();
    }

    /// <summary>
    /// If soundDisabled is false, stops sound playback and sets volume to 0.
    /// </summary>
    void stopSound(){
        if(soundDisabled) return;
        sound.volume = 0;
        sound.Stop();
    }
}
