using UnityEngine;
public interface IParticleDisplay
{
    void Init(Simulation2DAoSCounting sim, FluidData[] fluidDataArray);

	void Init(Simulation2DAoS_CPUCSort sim, FluidData[] fluidDataArray);
}

