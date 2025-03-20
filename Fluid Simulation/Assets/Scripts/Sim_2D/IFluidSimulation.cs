using UnityEngine;

public interface IFluidSimulation
{
    // Fluid simulation control
    void setEdgeType(int edgeTypeIndex);
    void setGravityMode(int gravityModeIndex);
    void setFixedTimestep(bool fixedTimestep);
    void togglePause();
    bool getPaused();
    void stepSimulation();
    void resetSimulation();
    void setBounds(Vector2 bounds);
    Vector2 getBounds();
    void setMaxParticles(int maxParticles);
    int GetParticleCount();

    // Fluid data
    FluidData[] getFluidDataArray();
    void setSelectedFluid(int fluidTypeIndex);

    // Brush control
    void SetBrushType(int brushTypeIndex);
    void setInteractionRadiusPercent(float radius);
    void setInteractionStrengthPercent(float strength);
    float getInteractionRadius();
    float getInteractionStrength();
    float getBrushSizePercent();
    float getBrushStrengthPercent();

    // Fluid detector
    bool IsPositionBufferValid();
    ComputeBuffer GetParticleBuffer();

    // Obstacle management
    void UpdateBoxColliders();
    void UpdateCircleColliders();
    void UpdateSourceObjects();
    void UpdateDrainObjects();
    void UpdateThermalBoxes();
    SourceObjectInitializer GetFirstSourceObject();
    void SetFirstSourceObject(SourceObjectInitializer source);

    // Cleanup
    void ReleaseComputeBuffers();
}