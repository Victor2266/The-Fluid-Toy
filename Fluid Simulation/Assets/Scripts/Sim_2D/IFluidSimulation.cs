using UnityEngine;

public interface IFluidSimulation
{
    /// <summary>
    /// Sets the fluid properties for the simulation using the provided FluidData.
    /// </summary>
    /// <param name="fluidData">The fluid data containing properties to be applied to the simulation.</param>
    //void SetFluidProperties(FluidData fluidData);

    void setEdgeType(int edgeTypeIndex);
    void setGravityMode(int gravityModeIndex);
    void setFixedTimestep(bool fixedTimestep);

    FluidData[] getFluidDataArray();

    void setSelectedFluid(int fluidTypeIndex);
    /// <summary>
    /// Sets the brush type for particle interaction.
    /// </summary>
    /// <param name="brushTypeIndex">The index corresponding to the desired brush type.</param>
    void SetBrushType(int brushTypeIndex);

    /// <summary>
    /// Toggles the pause state of the simulation.
    /// </summary>
    void togglePause();

    /// <summary>
    /// Gets the current pause state of the simulation.
    /// </summary>
    /// <returns>True if the simulation is paused, false otherwise.</returns>
    bool getPaused();

    /// <summary>
    /// Steps the simulation forward by one frame.
    /// </summary>
    void stepSimulation();

    /// <summary>
    /// Resets the simulation to its initial state.
    /// </summary>
    void resetSimulation();


    // Methods for fluid detection

    /// <summary>
    /// Checks if the position buffer exists and is valid.
    /// </summary>
    bool IsPositionBufferValid();

    /// <summary>
    /// Get current particle data
    /// </summary>
    ComputeBuffer GetParticleBuffer();
    
    /// <summary>
    /// Get total number of particles
    /// </summary>
    int GetParticleCount();

    /// <summary>
    /// Get interaction radius
    /// </summary>
    float getInteractionRadius();
    float getInteractionStrength();
    float getBrushSizePercent();
    float getBrushStrengthPercent();
    void setInteractionRadiusPercent(float radius);
    void setInteractionStrengthPercent(float strength);
    void UpdateBoxColliders();
    void UpdateCircleColliders();
    void UpdateSourceObjects();
    void UpdateDrainObjects();
    void UpdateThermalBoxes();

    SourceObjectInitializer GetFirstSourceObject();

    void SetFirstSourceObject(SourceObjectInitializer source);

    /// <summary>
    /// Releases the compute buffers used by the simulation.
    /// </summary>
    void ReleaseComputeBuffers();
}