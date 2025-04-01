using UnityEngine;
public interface IParticleDisplay
{
    void Init(IFluidSimulation sim);

    void ReleaseBuffers();
}

