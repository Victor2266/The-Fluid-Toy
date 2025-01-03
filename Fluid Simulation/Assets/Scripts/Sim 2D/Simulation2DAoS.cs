using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UIElements;
using System.Linq;

//Defining Structs
[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 40)]
public struct Particle // 40 bytes total
{
    public float2 density; //8 bytes, density and near density
    public Vector2 velocity; //8 bytes
    public Vector2 predictedPosition; // 8
    public Vector2 position; // 8
    public float temperature; // 4
    public FluidType type; // 4 (enum is int by default)
}

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 12)]
public struct Circle //12 bytes total
{
    public Vector2 pos; //8 bytes
    public float radius; //4 bytes
}

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 24)]
public struct OrientedBox //24 bytes total
{
    public Vector2 pos; //8 bytes
    public Vector2 size;
    public Vector2 zLocal;
};

public class Simulation2DAoS : MonoBehaviour, IFluidSimulation
{
    public event System.Action SimulationStepCompleted;

    [Header("Simulation Settings")]
    public float timeScale = 1;
    public bool fixedTimeStep;
    public bool enableHotkeys = false;
    public int iterationsPerFrame;

    public Vector2 boundsSize;
    public Vector2 obstacleSize;
    public Vector2 obstacleCentre;

    public enum EdgeType
    {
        Solid,
        Void,
        Loop
    }
    [SerializeField] private EdgeType edgeType = EdgeType.Solid;

    [Header("Selected Fluid Type")] // This is used for the draw brush
    [SerializeField] private int selectedFluid;

    // Brush Settings + Enum type
    public enum BrushType
    {
        DRAW,
        GRAVITY,
        NOTHING
    }

    [Header("Brush Type")]

    [SerializeField] private BrushType brushState = BrushType.GRAVITY;

    [Header("Interaction Settings")]
    public float interactionRadius;
    public float interactionStrength;

    // Fluid data array and buffer (to serialize then pass to GPU)
    [Header("Fluid Data Types")]
    // For the spatial subdivision to work we use the largest smoothing radius for the grid
    // By manually selecting the fluid types you can finetune the grid size
    [SerializeField] private bool manuallySelectFluidTypes; 
    private float maxSmoothingRadius = 0f;
    [SerializeField] public FluidData[] fluidDataArray;
    private FluidParam[] fluidParamArr; // Compute-friendly data type
    public ComputeBuffer fluidDataBuffer { get; private set; }

    private ScalingFactors[] scalingFactorsArr;
    private ComputeBuffer ScalingFactorsBuffer;

    [Header("References")]
    public ComputeShader compute;
    public ParticleSpawner spawner;
    public ParticleDisplay2D display;

    [Header("Obstacle Colliders")]
    public Transform[] boxColliders;
    public Transform[] circleColliders;

    private ComputeBuffer boxCollidersBuffer;
    private ComputeBuffer circleCollidersBuffer;

    private ComputeBuffer atomicCounterBuffer;
    private OrientedBox[] boxColliderData;
    private Circle[] circleColliderData;
    private const int MAX_COLLIDERS = 64; // Set a reasonable maximum number of colliders

    [Header("Particle Data")]
    // Buffers
    private Particle[] particleData;
    public ComputeBuffer particleBuffer { get; private set; }
    ComputeBuffer spatialIndices;
    ComputeBuffer spatialOffsets;
    GPUSort gpuSort;

    // Kernel IDs
    const int externalForcesKernel = 0;
    const int spatialHashKernel = 1;
    const int densityKernel = 2;
    const int pressureKernel = 3;
    const int viscosityKernel = 4;
    const int updatePositionKernel = 5;

    // State
    bool isPaused;
    ParticleSpawner.ParticleSpawnData spawnData;
    bool pauseNextFrame;

    public int numParticles { get; private set; }

    void Start()
    {
        Debug.Log("Controls: Space = Play/Pause, R = Reset, LMB = Attract, RMB = Repel");

        float deltaTime = 1 / 60f;
        Time.fixedDeltaTime = deltaTime;

        spawnData = spawner.GetSpawnData();
        numParticles = spawnData.positions.Length;
        
        if (!manuallySelectFluidTypes){
            // Get the number of fluid types (excluding Disabled)
            int numFluidTypes = Enum.GetValues(typeof(FluidType)).Length - 1;
            // Initialize arrays
            fluidDataArray = new FluidData[numFluidTypes];
            fluidParamArr = new FluidParam[numFluidTypes];
            scalingFactorsArr = new ScalingFactors[numFluidTypes];

            // Load each fluid type in order
            for (int i = 1; i < numFluidTypes + 1; i++)
            {
                string fluidName = Enum.GetName(typeof(FluidType), i);
                FluidData fluidData = Resources.Load<FluidData>($"Fluids/{fluidName}");
                fluidData.fluidType = (FluidType) i;
                
                if (fluidData == null)
                {
                    Debug.LogError($"Failed to load fluid data for {fluidName}. Ensure the scriptable object exists at Resources/Fluids/{fluidName}");
                    continue;
                }

                // Assign to array at index-1 (since we skip Disabled which is 0)
                fluidDataArray[i-1] = fluidData;
                fluidParamArr[i-1] = fluidData.getFluidParams();
                scalingFactorsArr[i-1] = fluidData.getScalingFactors();
            }
        }
        else{
            fluidParamArr = new FluidParam[fluidDataArray.Length];
            scalingFactorsArr = new ScalingFactors[fluidDataArray.Length];
            for (int i = 0; i < fluidDataArray.Length; i++)
            {
                fluidParamArr[i] = fluidDataArray[i].getFluidParams();
                fluidParamArr[i].fluidType = (FluidType) i + 1;
                scalingFactorsArr[i] = fluidDataArray[i].getScalingFactors();
            }
        }

        maxSmoothingRadius = 0f;
        for (int i = 0; i < fluidDataArray.Length; i++)
        {
            if (fluidDataArray[i].smoothingRadius > maxSmoothingRadius)
            {
                maxSmoothingRadius = fluidDataArray[i].smoothingRadius;
            }
        }

        // Create buffers
        // init buffer
        fluidDataBuffer = ComputeHelper.CreateStructuredBuffer<FluidParam>(fluidDataArray.Length);
        ScalingFactorsBuffer = ComputeHelper.CreateStructuredBuffer<ScalingFactors>(fluidDataArray.Length); //why does it say this leaks?

        particleData = new Particle[numParticles];
        particleBuffer = ComputeHelper.CreateStructuredBuffer<Particle>(numParticles);
        
        boxColliderData = new OrientedBox[MAX_COLLIDERS];
        circleColliderData = new Circle[MAX_COLLIDERS];

        boxCollidersBuffer = ComputeHelper.CreateStructuredBuffer<OrientedBox>(MAX_COLLIDERS);
        circleCollidersBuffer = ComputeHelper.CreateStructuredBuffer<Circle>(MAX_COLLIDERS);
        atomicCounterBuffer =  ComputeHelper.CreateStructuredBuffer<uint>(1);

        
        spatialIndices = ComputeHelper.CreateStructuredBuffer<uint3>(numParticles);
        spatialOffsets = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);

        // Set buffer data
        fluidDataBuffer.SetData(fluidParamArr);
        ScalingFactorsBuffer.SetData(scalingFactorsArr);
        SetInitialBufferData(spawnData);
        uint[] atomicCounter = {0};
        atomicCounterBuffer.SetData(atomicCounter);
        

        // Init compute
        ComputeHelper.SetBuffer(compute, fluidDataBuffer, "FluidDataSet", externalForcesKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, ScalingFactorsBuffer, "ScalingFactorsBuffer", densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, particleBuffer, "Particles", externalForcesKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, spatialIndices, "SpatialIndices", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, spatialOffsets, "SpatialOffsets", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, boxCollidersBuffer, "BoxColliders", externalForcesKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, circleCollidersBuffer, "CircleColliders", externalForcesKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, atomicCounterBuffer, "atomicCounter", spatialHashKernel);

        compute.SetInt("numBoxColliders", boxColliders.Length);
        compute.SetInt("numCircleColliders", circleColliders.Length);
        compute.SetInt("numParticles", numParticles);
        compute.SetFloat("maxSmoothingRadius", maxSmoothingRadius);


        gpuSort = new();
        gpuSort.SetBuffers(spatialIndices, spatialOffsets);


        // Init display
        display.InitAoS(this);
    }

    void FixedUpdate()
    {
        if (fixedTimeStep)
        {
            RunSimulationFrame(Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        // Run simulation if not in fixed timestep mode
        // (skip running for first few frames as deltaTime can be disproportionaly large)
        if (!fixedTimeStep && Time.frameCount > 10)
        {
            RunSimulationFrame(Time.deltaTime);
        }

        if (pauseNextFrame)
        {
            isPaused = true;
            pauseNextFrame = false;
        }

        UpdateColliderData();
        if (enableHotkeys)
            HandleHotkeysInput();
    }

    void RunSimulationFrame(float frameTime)
    {
        if (!isPaused)
        {
            float timeStep = frameTime / iterationsPerFrame * timeScale;

            UpdateSettings(timeStep);

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                RunSimulationStep();
                SimulationStepCompleted?.Invoke();
            }
        }
    }

    void RunSimulationStep()
    {
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);
        gpuSort.SortAndCalculateOffsets();
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
        //compute the pressure and viscosity on CPU
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);

    }

    void UpdateColliderData()
    {
        // Update box colliders
        for (int i = 0; i < boxColliders.Length && i < MAX_COLLIDERS; i++)
        {
            Transform collider = boxColliders[i];
            boxColliderData[i] = new OrientedBox
            {
                pos = collider.position,
                size = collider.localScale,
                zLocal = (Vector2)(collider.right) // Use right vector for orientation
            };
        }

        // Update circle colliders
        for (int i = 0; i < circleColliders.Length && i < MAX_COLLIDERS; i++)
        {
            Transform collider = circleColliders[i];
            circleColliderData[i] = new Circle
            {
                pos = collider.position,
                radius = collider.localScale.x * 0.5f // Assuming uniform scale
            };
        }

        // Update buffers
        boxCollidersBuffer.SetData(boxColliderData);
        circleCollidersBuffer.SetData(circleColliderData);
    }

    void UpdateSettings(float deltaTime)
    {
        compute.SetFloat("deltaTime", deltaTime);
        //compute.SetFloat("gravity", gravity);
        //compute.SetFloat("collisionDamping", collisionDamping);
        //compute.SetFloat("smoothingRadius", smoothingRadius);
        //compute.SetFloat("targetDensity", targetDensity);
        //compute.SetFloat("pressureMultiplier", pressureMultiplier);
        //compute.SetFloat("nearPressureMultiplier", nearPressureMultiplier);
        //compute.SetFloat("viscosityStrength", viscosityStrength);
        compute.SetVector("boundsSize", boundsSize);
        compute.SetInt("numBoxColliders", boxColliders.Length);
        compute.SetInt("numCircleColliders", circleColliders.Length);
        compute.SetInt("selectedFluidType", selectedFluid);

        compute.SetInt("edgeType", (int) edgeType);

        //These are now computed once at the start
        /*
        compute.SetFloat("Poly6ScalingFactor", 4 / (Mathf.PI * Mathf.Pow(currentFluid.smoothingRadius, 8)));
        compute.SetFloat("SpikyPow3ScalingFactor", 10 / (Mathf.PI * Mathf.Pow(currentFluid.smoothingRadius, 5)));
        compute.SetFloat("SpikyPow2ScalingFactor", 6 / (Mathf.PI * Mathf.Pow(currentFluid.smoothingRadius, 4)));
        compute.SetFloat("SpikyPow3DerivativeScalingFactor", 30 / (Mathf.Pow(currentFluid.smoothingRadius, 5) * Mathf.PI));
        compute.SetFloat("SpikyPow2DerivativeScalingFactor", 12 / (Mathf.Pow(currentFluid.smoothingRadius, 4) * Mathf.PI));
        */
        // Mouse interaction settings:
        HandleMouseInput();

    }

    void HandleMouseInput()
    {
        // Mouse interaction settings:
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isPullInteraction = Input.GetMouseButton(0);
        bool isPushInteraction = Input.GetMouseButton(1);
        float currInteractStrength = 0;

        if(brushState == BrushType.GRAVITY){
            if (isPushInteraction || isPullInteraction)
            {
                currInteractStrength = isPushInteraction ? -interactionStrength : interactionStrength;
            }
        } else if(brushState == BrushType.DRAW){
            if (isPullInteraction)
            {
                currInteractStrength = 1f;
                uint[] atomicCounter = {0};
                atomicCounterBuffer.SetData(atomicCounter);
            }
            else if (isPushInteraction)
            {
                currInteractStrength = -1f;
            }
        }
        
        compute.SetInt("brushType", (int) brushState);
        compute.SetVector("interactionInputPoint", mousePos);
        compute.SetFloat("interactionInputStrength", currInteractStrength);
        compute.SetFloat("interactionInputRadius", interactionRadius);
    }

    void SetInitialBufferData(ParticleSpawner.ParticleSpawnData spawnData)
    {
        Particle[] allPoints = new Particle[spawnData.positions.Length];

        // FIXME defaulting some values
        for (int i = 0; i < spawnData.positions.Length; i++)
        {
            Particle p = new Particle {
                position = spawnData.positions[i],
                predictedPosition = spawnData.positions[i],
                velocity = spawnData.velocities[i],
                density = new float2(0, 0),
                temperature = 22.0f,
                type = FluidType.Water // Or whatever default type you want};
            };
            allPoints[i] = p;
        }

        particleBuffer.SetData(allPoints);
    }

    void HandleHotkeysInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            togglePause();
            GameObject sidebar = GameObject.FindGameObjectWithTag("Sidebar");
            if (sidebar != null)
            {
                SideBarWrapper sideBarWrapper = sidebar.GetComponent<SideBarWrapper>();
                if (sideBarWrapper != null)
                {
                    sideBarWrapper.UpdatePauseIcon();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            stepSimulation();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            resetSimulation();
        }
    }


    void OnDestroy()
    {
        ComputeHelper.Release(
            fluidDataBuffer,
            ScalingFactorsBuffer,
            particleBuffer,
            spatialIndices, 
            spatialOffsets,
            boxCollidersBuffer,
            circleCollidersBuffer,
            atomicCounterBuffer
        );
    }


    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawWireCube(Vector2.zero, boundsSize);
        
        // Draw all box colliders
        if (boxColliders != null)
        {
            foreach (Transform boxCollider in boxColliders)
            {
                if (boxCollider != null)
                {
                    Gizmos.DrawWireCube(boxCollider.position, boxCollider.localScale);
                }
            }
        }

        // Draw all circle colliders
        if (circleColliders != null)
        {
            foreach (Transform circleCollider in circleColliders)
            {
                if (circleCollider != null)
                {
                    Gizmos.DrawWireSphere(circleCollider.position, circleCollider.localScale.x * 0.5f);
                }
            }
        }

        if (Application.isPlaying)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool isPullInteraction = Input.GetMouseButton(0);
            bool isPushInteraction = Input.GetMouseButton(1);
            bool isInteracting = isPullInteraction || isPushInteraction;
            if (isInteracting)
            {
                Gizmos.color = isPullInteraction ? Color.green : Color.red;
                Gizmos.DrawWireSphere(mousePos, interactionRadius);
            }
        }

    }

    // These are the Interface Functions for outside game scripts:
    //
    //

    public void setEdgeType(int edgeTypeIndex){
        edgeType = (EdgeType)edgeTypeIndex;
    }

    public void setSelectedFluid(int fluidTypeIndex){
        selectedFluid = fluidTypeIndex;
    }

    public void SetBrushType(int brushTypeIndex)
    {
        brushState = (BrushType)brushTypeIndex;
    }

    public void togglePause()
    {
        isPaused = !isPaused;
    }
    public bool getPaused()
    {
        return isPaused;
    }
    public void stepSimulation()
    {
        isPaused = false;
        pauseNextFrame = true;
    }
    public void resetSimulation()
    {
        isPaused = true;
        // Reset positions, the run single frame to get density etc (for debug purposes) and then reset positions again
        SetInitialBufferData(spawnData);
        RunSimulationStep();
        SetInitialBufferData(spawnData);
    }

    // These functions are for the fluid detector
    public bool IsPositionBufferValid()
    {
        return particleBuffer != null;
    }
    public Vector2[] GetParticlePositions()
    {
        Vector2[] positions = new Vector2[numParticles];
        particleBuffer.GetData(particleData);

        for (int i = 0; i < numParticles; i++)
        {
            positions[i] = particleData[i].position;
        }
        return positions;
    }
    public int GetParticleCount()
    {
        return numParticles;
    }
}
