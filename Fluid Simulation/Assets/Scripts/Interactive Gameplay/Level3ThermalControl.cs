using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class Level3ThermalControl : MonoBehaviour
{
    public Level3GasControlDial[] dials;
    public SpriteRenderer spriteRenderer;

    public Gradient thermalGradient;

    public float currentTemp;

    public int tBoxIndex;

    public float minThermal;

    public float maxThermal;

    public float heatingSpeed;
    public float dialThreshold;

    private GameObject simObject;
    public IFluidSimulation sim;

    void Start()
    {
        if(spriteRenderer == null){
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
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
        bool thresholdReached = true;
		foreach(Level3GasControlDial dial in dials){
            if(dial.getVelo() < dialThreshold){
                thresholdReached = false;
            }
        }
        if(thresholdReached){
            ThermalBoxInitializer tBox = sim.GetThermalBox(tBoxIndex);
            tBox.temperature = Mathf.Clamp(tBox.temperature + (heatingSpeed * 0.02F), minThermal, maxThermal);
            currentTemp = tBox.temperature;
            sim.SetThermalBox(tBox, tBoxIndex);
            updateGradient();
        }else{
            ThermalBoxInitializer tBox = sim.GetThermalBox(tBoxIndex);
            tBox.temperature = Mathf.Clamp(tBox.temperature - (heatingSpeed * 0.02F), minThermal, maxThermal);
            currentTemp = tBox.temperature;
            sim.SetThermalBox(tBox, tBoxIndex);
            updateGradient();
        }

	}

    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }


    void updateGradient()
    {
        float t = Remap(currentTemp, minThermal, maxThermal, 0, 1);
        spriteRenderer.color = thermalGradient.Evaluate(t);
    }
}