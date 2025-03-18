using System.Runtime.CompilerServices;
using UnityEngine;

public class Level3GasControlDial : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public IFluidSimulation sim;
    public int sourceIndex;
    public float minVelo = 0;
    public float maxVelo = 1F;
    public float minAngle = 0F;
    public float maxAngle = 180F;
    public float totalVelo = 1.5F;
    public float minSpawn = 0;
    public float maxSpawn = 1F;
    private GameObject simObject;
    private float currVelo = 0F;
    private float currSpawn = 0F;
    private SourceObjectInitializer source;
    public Level3GasControlDial[] dials;
    private bool pressed = false;

    void Start()
    {
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
    }

    void FixedUpdate()
    {
        if(pressed){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 toMouse = mousePos - transform.position;
            float angleToMouse = -1 * Mathf.Atan2(toMouse.x, toMouse.y) * Mathf.Rad2Deg;
            if(angleToMouse < minAngle){
                if(Mathf.Abs(angleToMouse) > 90){
                    angleToMouse = maxAngle;
                }else{
                    angleToMouse = minAngle;
                }
            }
            angleToMouse = Mathf.Clamp(angleToMouse, minAngle, maxAngle);
            transform.eulerAngles = new Vector3(0, 0, angleToMouse);
            currVelo = Remap(angleToMouse, minAngle, maxAngle, minVelo, maxVelo);
            currSpawn = Remap(angleToMouse, minAngle, maxAngle, minSpawn, maxSpawn);
            updateSource();
        }else{
            updateDial();
        }
        
    }

    void updateSource(){
        float sumVelo = 0;
        float reduction = 0;
        foreach(Level3GasControlDial dial in dials){
            sumVelo += dial.getVelo();
        }
        if (sumVelo + currVelo > totalVelo){
            reduction = (sumVelo + currVelo -totalVelo)/dials.Length;
        }
        foreach(Level3GasControlDial dial in dials){
            dial.setVelo(Mathf.Max(dial.getVelo() - reduction, minVelo));
        }
        source = sim.GetSourceObject(sourceIndex);
        source.velo.y = currVelo;
        source.spawnRate = currSpawn;
        sim.SetSourceObject(source, sourceIndex);
    }

    void OnMouseDown()
    {
        pressed = true;
        
    }

	void OnMouseUp()
	{
		pressed = false;
	}
	public float getVelo(){
        return currVelo;
    }
    public void setVelo(float velo){
        currVelo = velo;
    }
    
    void updateDial(){
        float angle = Remap(currVelo, minVelo, maxVelo, minAngle, maxAngle);
        currSpawn = Remap(currVelo, minVelo, maxVelo, minSpawn, maxSpawn);
        transform.eulerAngles = new Vector3(0, 0, angle);
        source = sim.GetSourceObject(sourceIndex);
        source.velo.y = currVelo;
        source.spawnRate = currSpawn;
        sim.SetSourceObject(source, sourceIndex);
    }
    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }
}
