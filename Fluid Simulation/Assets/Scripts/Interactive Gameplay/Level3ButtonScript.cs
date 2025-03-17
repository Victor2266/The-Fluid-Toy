using System.Runtime.CompilerServices;
using UnityEngine;

public class Level3ButtonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public IFluidSimulation sim;
    public int sourceIndex;
    public float activationValue = 0.1F;
    public float activationTime;
    private GameObject simObject;
    private float clickedTime;
    private bool clicked;

    private SourceObjectInitializer source;
    void Start()
    {
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
    }

    void FixedUpdate()
    {
        if(clicked){
            if(activationTime != 0){
                if (Time.time - clickedTime >= activationTime){
                    deactivateSource();
                }
            }
            
        }
    }

    void OnMouseOver()
    {
        //add some animation for when hovering over
        if (Input.GetMouseButtonDown(0)){
            Debug.Log("Pressed");
            activateSource();
        }
    }

    void activateSource(){
        source = sim.GetSourceObject(sourceIndex);
        source.spawnRate = activationValue;
        clicked = true;
        clickedTime = Time.time;
        sim.SetSourceObject(source, sourceIndex);
    }

    void deactivateSource(){
        source = sim.GetSourceObject(sourceIndex);
        source.spawnRate = 0;
        clicked = true;
        clickedTime = Time.time;
        sim.SetSourceObject(source, sourceIndex);
    }
}
