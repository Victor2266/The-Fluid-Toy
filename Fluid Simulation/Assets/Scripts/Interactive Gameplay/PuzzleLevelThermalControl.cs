using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class PuzzleLevelThermalControl : MonoBehaviour
{
    [Header("Level references")]
    public IFluidSimulation sim;
    public PuzzleLevelGasControlDial[] dials;
    public SpriteRenderer spriteRenderer;

    [Header("Gradient for color change with thermal change")]
    public Gradient thermalGradient;

    [Header("Thermal box thresholds and settings")]
    public float currentTemp;
    public int tBoxIndex;
    public float minThermal;
    public float maxThermal;
    public float heatingSpeed;
    public float dialThreshold;

    private GameObject simObject;
    

    void Start()
    {
        // sprite renderer obtained from components if not specified
        if(spriteRenderer == null){
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (sim == null){
            simObject = GameObject.FindGameObjectWithTag("Simulation");
            sim = simObject.GetComponent<IFluidSimulation>();
        }
        // must connect dials
        if(dials.Length == 0){
            Debug.LogError("Error: no dials attached to thermal box control");
            return;
        }
    }

    /// <summary>
    /// Controls thermal box heating or cooling based on whether each dial in dials is set to a value above the dialThreshold value.
    /// If all dials are set above the threshold, then the thermal box heats up at a rate of heatingSpeed degrees per second, otherwise it cools at the same rate.
    /// </summary>
	void FixedUpdate() // called every 0.02s, heating rate is adjusted to this update rate
	{
        bool thresholdReached = true;
		foreach(PuzzleLevelGasControlDial dial in dials){
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
    /// <summary>
    /// Updates the sprite renderer color based on the gradient specified and the current temp value.
    /// </summary>
    void updateGradient()
    {
        float t = Remap(currentTemp, minThermal, maxThermal, 0, 1);
        spriteRenderer.color = thermalGradient.Evaluate(t);
    }

    /// <summary>
    /// Remaps input value with range sourceFrom to SourceTo, to a value in range targetFrom to targetTo.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceFrom"></param>
    /// <param name="sourceTo"></param>
    /// <param name="targetFrom"></param>
    /// <param name="targetTo"></param>
    /// <returns>source value remapped to new limits.</returns>
    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }


    
}