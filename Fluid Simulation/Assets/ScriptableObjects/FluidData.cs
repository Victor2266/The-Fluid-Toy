using System.Runtime.InteropServices;
using UnityEngine;

public enum FluidType
{
    Disabled,
    Water,
    Steam,
    Honey,
    Lava
}

// Struct for passing to compute shader.
[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct FluidParam
{
    public FluidType fluidType;
    public float gravity;
    public float collisionDamping;
    public float smoothingRadius;
    public float targetDensity;
    public float pressureMultiplier;
    public float nearPressureMultiplier;
    public float viscosityStrength;
    public float startTemperature;
};

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 20)]
public struct ScalingFactors
{
	public float Poly6;
	public float SpikyPow3;
	public float SpikyPow2;
	public float SpikyPow3Derivative;
	public float SpikyPow2Derivative;
};

[CreateAssetMenu(fileName = "New Fluid", menuName = "Fluids/New Fluid Type")]
public class FluidData : ScriptableObject
{
    [Header("Basic Properties")]
    [Tooltip("Type of fluid")]
    public FluidType fluidType = FluidType.Water;

    [Tooltip("Gravity force applied to the fluid")]
    public float gravity = -9.81f;

    [Tooltip("Damping applied when fluid collides with surfaces")]
    [Range(0f, 1f)]
    public float collisionDamping = 0.95f;

    [Header("Particle Properties")]
    [Tooltip("Radius within which particles interact")]
    public float smoothingRadius = 0.35f;

    [Tooltip("Target density for the fluid")]
    public float targetDensity = 55f;

    [Header("Pressure Settings")]
    [Tooltip("Multiplier for pressure calculation")]
    public float pressureMultiplier = 500f;

    [Tooltip("Multiplier for near-field pressure calculation")]
    public float nearPressureMultiplier = 18f;

    [Header("Viscosity")]
    [Tooltip("Strength of the fluid's viscosity")]
    [Range(0f, 3f)]
    public float viscosityStrength = 0.06f;

    [Header("Thermal Properties")]
    [Tooltip("Starting temperature of the fluid")]
    public float startTemperature = 22f;


    [Header("Shader Properties")]
    [Tooltip("What Type of Shader to use")]
    
    public Shader shader;
    public float scale;
	public Gradient colourMap;
	public int gradientResolution;
	public float velocityDisplayMax;
    public float lavaGlowIntensity = 1.0f;
    public float lavaMinTemp = 800f;
    public float lavaMaxTemp = 1200f;
    public float steamNoiseScale = 10f;
    public float steamTimeFactor = 1f;

    // Validation method to ensure values stay within reasonable bounds
    private void OnValidate()
    {
        smoothingRadius = Mathf.Max(0.001f, smoothingRadius);
        targetDensity = Mathf.Max(0.001f, targetDensity);
        pressureMultiplier = Mathf.Max(0f, pressureMultiplier);
        nearPressureMultiplier = Mathf.Max(0f, nearPressureMultiplier);
    }

    // Returns a compressed, compute-friendly copy of this instance as a FluidParam struct
    public FluidParam getFluidParams()
    {
        FluidParam fluidParams = new FluidParam
        {
            fluidType = fluidType,
            gravity = gravity,
            collisionDamping = collisionDamping,
            smoothingRadius = smoothingRadius,
            targetDensity = targetDensity,
            pressureMultiplier = pressureMultiplier,
            nearPressureMultiplier = nearPressureMultiplier,
            viscosityStrength = viscosityStrength,
            startTemperature = startTemperature
        };
        return fluidParams;
    }
    public ScalingFactors getScalingFactors()
    {
        ScalingFactors scalingFactors = new ScalingFactors
        {
            Poly6 = 4 / (Mathf.PI * Mathf.Pow(smoothingRadius, 8)),
            SpikyPow3 = 10 / (Mathf.PI * Mathf.Pow(smoothingRadius, 5)),
            SpikyPow2 = 6 / (Mathf.PI * Mathf.Pow(smoothingRadius, 4)),
            SpikyPow3Derivative = 30 / (Mathf.Pow(smoothingRadius, 5) * Mathf.PI),
            SpikyPow2Derivative = 12 / (Mathf.Pow(smoothingRadius, 4) * Mathf.PI)
        };
        return scalingFactors;
    }
};
