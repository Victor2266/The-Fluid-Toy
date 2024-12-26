using UnityEngine;

public enum FluidType
{
    Disabled,
    Water,
    Steam,
    Honey
}

// Struct for passing to compute shader
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
    [Range(0f, 1f)]
    public float viscosityStrength = 0.06f;


    [Header("Shader Properties")]
    [Tooltip("What Type of Shader to use")]
    public Shader shader;
    public float scale;
	public Gradient colourMap;
	public int gradientResolution;
	public float velocityDisplayMax;

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
            viscosityStrength = viscosityStrength
        };
        return fluidParams;
    }
};
