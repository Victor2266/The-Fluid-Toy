using System.Runtime.CompilerServices;
using UnityEngine;

public class Level3DetectionScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool disableSourceUpdate = false;
    public IFluidSimulation sim;
    public FluidDetector fluidDetector;
    public AudioSource sound;
    public float targetVolume = 0.3F;
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
            Debug.LogError("No fluid detector connected");
        }
    }

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

    void startSound(){
        if(soundDisabled) return;

        sound.volume = targetVolume;
        sound.Play();
    }

    void stopSound(){
        if(soundDisabled) return;
        sound.volume = 0;
        sound.Stop();
    }
}
