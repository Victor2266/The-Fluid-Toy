using System.Runtime.CompilerServices;
using UnityEngine;

public class Level3GasControlDial : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public IFluidSimulation sim;
    public FluidDetector fluidDetector;
    public int sourceIndex;
    public float minVelo = 0;
    public float maxVelo = 1F;
    public float minAngle = 0F;
    public float maxAngle = 180F;
    private GameObject simObject;
    private float currVelo = 0F;
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

    // void FixedUpdate()
    // {
        
        

    // }

    void updateSource(){
        source = sim.GetSourceObject(sourceIndex);
        source.velo.y = currVelo;
        sim.SetSourceObject(source, sourceIndex);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 toMouse = mousePos - transform.position;
            float angleToMouse = Mathf.Atan2(toMouse.x, toMouse.y);
            transform.eulerAngles = new Vector3(0, 0, angleToMouse);
        }
    }
    void UpdateAngle(){

    }

    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }
}
