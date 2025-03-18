using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class Level3ThermalControl : MonoBehaviour
{
    public Level3GasControlDial[] dials;

    public int tBoxIndex;

    public float minThermal;

    public float maxThermal;

    public float minHeating;

    public float maxHeating;

    public float dialThreshold;

    private GameObject simObject;
    public IFluidSimulation sim;

    void Start()
    {
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
        if(dials.Length == 0){
            Debug.LogError("Error: no dials attached to thermal box control");
            return;
        }
    }

	void FixedUpdate() // called every 0.02s, heating rate is adjusted to this update rate
	{
        float heating = 0;
		foreach(Level3GasControlDial dial in dials){
            if(dial.getVelo() > dialThreshold)
            {
                heating += Remap(dial.getVelo() - dialThreshold, dial.minVelo, dial.maxVelo, minHeating, maxHeating) * 0.02F;
            }else{
                heating -= Remap(dialThreshold - dial.getVelo(), dial.minVelo, dial.maxVelo, minHeating, maxHeating) * 0.02F;
            }
            
        }
        ThermalBoxInitializer tBox = sim.GetThermalBox(tBoxIndex);
        tBox.temperature = Mathf.Clamp(tBox.temperature + heating, minThermal, maxThermal);
        sim.SetThermalBox(tBox, tBoxIndex);
	}

    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }
}