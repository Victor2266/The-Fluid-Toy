using UnityEngine;
using UnityEngine.Rendering;

public abstract class FluidSensor : MonoBehaviour
{
    [Header("Management Flag (do not change while running)")]
    public bool isManagedSensor = false;

    protected abstract void ParticleRequest(AsyncGPUReadbackRequest request);
    public abstract void CheckSensor(Particle[] particles);

    public enum DetectionType
    {
        Disabled,
        GreaterThan,
        LessThan,
        Equals
    }
}