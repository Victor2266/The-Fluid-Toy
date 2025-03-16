using System.Runtime.CompilerServices;
using UnityEngine;

public class Level3DetectionScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public IFluidSimulation sim;
    public FluidDetector fluidDetector;
    public int sourceIndex;
    public float activationValue = 0.1F;
    public float activationTime;
    private float toggledTime;
    private GameObject simObject;
    private bool activated;
    private SourceObjectInitializer source;
    void Start()
    {
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
        if(fluidDetector == null){
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
        source = sim.GetSourceObject(sourceIndex);
        source.spawnRate = activationValue;
        sim.SetSourceObject(source, sourceIndex);
        activated = true;
        toggledTime = Time.time;
    }

    void deactivateSource(){
        source = sim.GetSourceObject(sourceIndex);
        source.spawnRate = 0;
        sim.SetSourceObject(source, sourceIndex);
        toggledTime = Time.time;
    }
}
