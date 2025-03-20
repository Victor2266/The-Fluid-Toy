using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UIElements;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;


public class Simulation2DAoS_CPUCSort : MonoBehaviour, IFluidSimulation
{
    public event System.Action SimulationStepCompleted;

    [Header("Simulation Settings")]
    public float timeScale = 1;
    public bool fixedTimeStep; // Enable for consistent simulation steps across different framerates, (limits smoothness to 120fps)
    public int maxParticles;
    [Tooltip("Disable this to manually add obstacles to the simulation. If enabled, the obstacles will scanned via tags")]
    public bool scanForObstaclesOnStart = true;
    public bool scanForParticleSpawnersOnStart = true;
    public int iterationsPerFrame;
    public float globalEntropyRate = 1f;
    public float roomTemperature = 22f;

    public Vector2 boundsSize;
    public Vector2 obstacleSize;
    public Vector2 obstacleCentre;
    [SerializeField] private EdgeType edgeType = EdgeType.Solid;
    public uint spawnRate = 100; // How many particles that can spawn per frame

    [Header("Selected Fluid Type")] // This is used for the draw brush
    [SerializeField] private int selectedFluid;

    [Header("Brush Type")]

    [SerializeField] private BrushType brushState = BrushType.Gravity;

    [Header("Interaction Settings")]
    public float interactionRadius;
    public float minRadius = 0.25f;
    public float maxRadius = 24f;
    public float interactionStrength;
    public float minStrength = 36f;
    public float maxStrength = 720f;
    public float smoothingTime = 0.04f;
    public bool enableScrolling = false;
    public bool enableHotkeys = false;
    private float targetInteractionRadius;
    private float targetInteractionStrength;
    private float smoothRadiusVelocity;
    private float smoothStrengthVelocity;



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
    public ParticleSpawner[] spawners;
    public IParticleDisplay display;

    [Header("Obstacle Colliders")]
    public Transform[] boxColliders;
    public Transform[] circleColliders;
    private ComputeBuffer boxCollidersBuffer;
    private ComputeBuffer circleCollidersBuffer;

    [Header("Source and Drain Objects")]
    public SourceObjectInitializer[] sourceObjects;
    public Transform[] drainObjects;
    private ComputeBuffer sourceObjectBuffer;
    private ComputeBuffer drainObjectBuffer;

    [Header("Thermal Boxes (Particles in this box will be brought to the box temperature)")]
    public ThermalBoxInitializer[] thermalBoxes;
    private ComputeBuffer thermalBoxesBuffer;

    // Counter Variables
    private ComputeBuffer atomicCounterBuffer;
    private uint frameCounter; // This doesn't actually update each frame, just after it is used.

    // Private Variables 
    private OrientedBox[] boxColliderData;
    private Circle[] circleColliderData;
    private SourceObject[] sourceObjectData;
    private OrientedBox[] drainObjectData;
    private ThermalBox[] thermalBoxData;

    [Header("Particle Data")]
    // Buffers
    private Particle[] particleData;
    public ComputeBuffer particleBuffer { get; private set; }
    public ComputeBuffer sortedParticleBuffer { get; private set; }
    public ComputeBuffer sortedIndices { get; private set; }
    ComputeBuffer spatialIndices;
    ComputeBuffer spatialOffsets;
    GPUCountSort gpuSort; // Class adjust for sorting algorithm
    SpatialOffsetCalculator spatialOffsetsCalc;

    // Kernel IDs
    const int SpawnParticlesKernel = 0;
    const int externalForcesKernel = 1;
    const int spatialHashKernel = 2;
    const int reorderKernel = 3;
    const int reorderCopybackKernel = 4;
    const int densityKernel = 5;
    const int pressureKernel = 6;
    const int viscosityKernel = 7;
    const int temperatureKernel = 8;
    const int updatePositionKernel = 9;
    const int updateStateKernel = 10;
    const int mergeCPUParticlesKernel = 11; // For CPU-GPU

    // State
    bool isPaused;
    ParticleSpawner.ParticleSpawnData[] spawnDataArr;
    bool pauseNextFrame;

    public int numParticles { get; private set; }

    private float accumulatedTime = 0f;
    private const float MAX_DELTA_TIME = 1f / 30f; // Maximum allowed delta time
    private const float FIXED_TIME_STEP = 1f / 120f; // Your desired fixed time step

    [Header("CPU Computing")]
    //CPU Compute
    
    public bool toggleCPUComputing = false;
    public bool runCPUGPUEveryFrame = false;

    CPUParticleKernelAoS CPUKernelAOS;

    ComputeBuffer cpuparticlebuffer;
    ComputeBuffer keyarrbuffer;
    public uint numCPUKeys = 10;
    private bool cpuflip = false;
    void Start()
    {
        // Debug.Log(System.Runtime.InteropServices.Marshal.SizeOf(typeof(SourceObjectInitializer))); //This prints the size of the typeof(struct)
        //Debug.Log("Controls: Space = Play/Pause, R = Reset, LMB = Attract, RMB = Repel");
        Debug.Log("This level is using the CPU CSort");
        CPUKernelAOS = new CPUParticleKernelAoS();
        targetInteractionRadius = interactionRadius;
        targetInteractionStrength = interactionStrength;
        numParticles = 0;

        if (scanForParticleSpawnersOnStart){
            spawners = FindObjectsByType<ParticleSpawner>(FindObjectsSortMode.None);
        }
        spawnDataArr = new ParticleSpawner.ParticleSpawnData[spawners.Length];
        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogWarning("No particle spawners assigned. If this is unintended, please add at least one spawner in the inspector.");
            return;
        }
        for (int k = 0; k < spawners.Length; k++)
        {
            spawnDataArr[k] = spawners[k].GetSpawnData();
            numParticles += spawnDataArr[k].positions.Length;
        }

        if (!manuallySelectFluidTypes)
        {
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
                fluidData.fluidType = (FluidType)i;

                if (fluidData == null)
                {
                    Debug.LogError($"Failed to load fluid data for {fluidName}. Ensure the scriptable object exists at Resources/Fluids/{fluidName}");
                    continue;
                }

                // Assign to array at index-1 (since we skip Disabled which is 0)
                fluidDataArray[i - 1] = fluidData;
                fluidParamArr[i - 1] = fluidData.getFluidParams();
                scalingFactorsArr[i - 1] = fluidData.getScalingFactors();
            }
        }
        else
        {
            fluidParamArr = new FluidParam[fluidDataArray.Length];
            scalingFactorsArr = new ScalingFactors[fluidDataArray.Length];
            for (int i = 0; i < fluidDataArray.Length; i++)
            {
                fluidParamArr[i] = fluidDataArray[i].getFluidParams();
                // fluidParamArr[i].fluidType = (FluidType)i + 1;
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
        sortedParticleBuffer = ComputeHelper.CreateStructuredBuffer<Particle>(numParticles);

        boxColliderData = new OrientedBox[boxColliders.Length];
        circleColliderData = new Circle[circleColliders.Length];
        sourceObjectData = new SourceObject[sourceObjects.Length];
        drainObjectData = new OrientedBox[drainObjects.Length];
        if (thermalBoxes == null){
            thermalBoxes = new ThermalBoxInitializer[0]; // I think .Length wasn't working on null object types?
        }
        thermalBoxData = new ThermalBox[thermalBoxes.Length];

        boxCollidersBuffer = ComputeHelper.CreateStructuredBuffer<OrientedBox>(Mathf.Max(boxColliders.Length, 1));
        circleCollidersBuffer = ComputeHelper.CreateStructuredBuffer<Circle>(Mathf.Max(circleColliders.Length, 1));
        sourceObjectBuffer = ComputeHelper.CreateStructuredBuffer<SourceObject>(Mathf.Max(sourceObjects.Length, 1));
        drainObjectBuffer = ComputeHelper.CreateStructuredBuffer<OrientedBox>(Mathf.Max(drainObjects.Length, 1));
        thermalBoxesBuffer = ComputeHelper.CreateStructuredBuffer<ThermalBox>(Mathf.Max(thermalBoxes.Length, 1));

        atomicCounterBuffer = ComputeHelper.CreateStructuredBuffer<uint>(2);


        spatialIndices = ComputeHelper.CreateStructuredBuffer<uint2>(numParticles);
        spatialOffsets = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);
        sortedIndices = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);

        // CPU Particle Buffer
        cpuparticlebuffer = ComputeHelper.CreateStructuredBuffer<Particle>(numParticles);
        keyarrbuffer = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);

        // Set buffer data
        fluidDataBuffer.SetData(fluidParamArr);
        ScalingFactorsBuffer.SetData(scalingFactorsArr);
        SetInitialBufferData(spawnDataArr);
        uint[] atomicCounter = { 0, frameCounter++ };
        atomicCounterBuffer.SetData(atomicCounter);

        // Init compute
        ComputeHelper.SetBuffer(compute, fluidDataBuffer, "FluidDataSet", SpawnParticlesKernel, externalForcesKernel, densityKernel, pressureKernel, viscosityKernel, temperatureKernel, updatePositionKernel, updateStateKernel);
        ComputeHelper.SetBuffer(compute, ScalingFactorsBuffer, "ScalingFactorsBuffer", densityKernel, pressureKernel, viscosityKernel, temperatureKernel);
        ComputeHelper.SetBuffer(compute, particleBuffer, "Particles", SpawnParticlesKernel, externalForcesKernel, reorderKernel, reorderCopybackKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel, temperatureKernel, updatePositionKernel, mergeCPUParticlesKernel, updateStateKernel);
        ComputeHelper.SetBuffer(compute, spatialIndices, "SpatialIndices", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel, temperatureKernel);
        ComputeHelper.SetBuffer(compute, spatialOffsets, "SpatialOffsets", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel, temperatureKernel);
        ComputeHelper.SetBuffer(compute, sortedIndices, "SortedIndices", spatialHashKernel, reorderKernel, reorderCopybackKernel);
        ComputeHelper.SetBuffer(compute, sortedParticleBuffer, "SortedParticles", reorderKernel, reorderCopybackKernel);
        ComputeHelper.SetBuffer(compute, boxCollidersBuffer, "BoxColliders", updatePositionKernel);
        ComputeHelper.SetBuffer(compute, circleCollidersBuffer, "CircleColliders", updatePositionKernel);
        ComputeHelper.SetBuffer(compute, sourceObjectBuffer, "SourceObjs", SpawnParticlesKernel);
        ComputeHelper.SetBuffer(compute, drainObjectBuffer, "DrainObjs", updatePositionKernel);
        ComputeHelper.SetBuffer(compute, thermalBoxesBuffer, "ThermalBoxes", updatePositionKernel, temperatureKernel);
        ComputeHelper.SetBuffer(compute, atomicCounterBuffer, "atomicCounter", SpawnParticlesKernel, updatePositionKernel, updateStateKernel);
        ComputeHelper.SetBuffer(compute, cpuparticlebuffer, "CPUParticles", mergeCPUParticlesKernel);
        ComputeHelper.SetBuffer(compute, keyarrbuffer, "keyarr", densityKernel, pressureKernel, viscosityKernel, mergeCPUParticlesKernel);

        compute.SetInt("numParticles", numParticles);
        compute.SetInt("numFluidTypes", fluidDataArray.Length);
        compute.SetFloat("maxSmoothingRadius", maxSmoothingRadius);
        compute.SetInt("spawnRate", (int)spawnRate);
        compute.SetFloat("roomTemperature", roomTemperature);
        compute.SetFloat("globalEntropyRate", globalEntropyRate);
        compute.SetInt("numCPUKeys", (int)numCPUKeys);

        // GPU Sort Init
        gpuSort = new GPUCountSort(spatialIndices, sortedIndices, (uint)(spatialIndices.count - 1), keyarrbuffer);
        spatialOffsetsCalc = new SpatialOffsetCalculator(spatialIndices, spatialOffsets);

        // Init display
        display = GetComponent<IParticleDisplay>();
        display.Init(this);
        if (scanForObstaclesOnStart) ScanForAllObstaclesLists();

        //initialize local arrays
        initializeCPUKernelSettingsAoS();
    }

    void Update()
    {
        // Run simulation in fixed timestep mode
        // It will make number of simulation steps more consistent accross different frame rates
        // (it will be perfectly consistent down to 30fps)
        // ONLY ACTIVATE IF CONSISTENCY BETWEEN FRAMERATES IS IMPORTANT, non-fixed can be smoother looking above 120fps.
        // (skip running for first few frames as deltaTime can be disproportionaly large)
        if (fixedTimeStep && frameCounter > 10)
        {
            // Accumulate time, but cap it to prevent spiral of death
            accumulatedTime += Mathf.Min(Time.deltaTime, MAX_DELTA_TIME);

            // Run as many fixed updates as necessary to catch up
            // When the FPS is low then it will run more times to catch up
            // When the FPS is high then it will run less times
            while (accumulatedTime >= FIXED_TIME_STEP)
            {
                RunSimulationFrame(FIXED_TIME_STEP); // This way the simulation steps are consistent
                accumulatedTime -= FIXED_TIME_STEP;
            }
        }
        // In variable timestep mode, the delta time can vary, which slightly effects physics consistency across framerates
        // The number of simulation steps varies depending on the framerate 
        // Tabbing out has been fixed so it won't cause issues
        // This seems to give smoother results than fixed timestep above 120fps.
        else if (!fixedTimeStep && frameCounter > 10)
        {
            RunSimulationFrame(Time.deltaTime);
        }
        else
        {
            // Use custom frame counter because Time.frameCount does not reset on reloads
            frameCounter++;
        }

        if (pauseNextFrame)
        {
            isPaused = true;
            pauseNextFrame = false;
            UpdateSideBarIcons();
        }

        UpdateColliderData();

        if (enableScrolling)
            HandleScrollInput();

        if (enableHotkeys)
            HandleHotkeysInput();
    }

    void RunSimulationFrame(float frameTime)
    {
        // Cap the maximum deltaTime to prevent instability when tabbing out
        float cappedFrameTime = frameTime > MAX_DELTA_TIME ? MAX_DELTA_TIME : frameTime; // Cap at 30fps equivalent

        if (!isPaused)
        {
            float timeStep = cappedFrameTime / iterationsPerFrame * timeScale;

            UpdateSettings(timeStep);

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                if(toggleCPUComputing){
                    if(runCPUGPUEveryFrame){
                        cpuflip = false;
                        RunSimulationStepCPUGPUEveryFrame();
                    }else{
                        if(!cpuflip){
                            cpuflip = true;
                            RunSimulationStepGPU();
                        }else{
                            cpuflip = false;
                            RunSimulationStepCPU();
                        }
                    }
                }else{
                    cpuflip = false;
                    RunSimulationStep();
                    
                }
                SimulationStepCompleted?.Invoke();
            }
        }
    }

    void RunSimulationStepCPU()
    {
        // Init + ext forces
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: SpawnParticlesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);

        // Sort & offsets; copyback for memory coherency
        gpuSort.Run();
        spatialOffsetsCalc.Run(false);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderCopybackKernel);
        runCPUComputeTest();
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: temperatureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updateStateKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);

    }
    void RunSimulationStepGPU()
    {
        // Init + ext forces
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: SpawnParticlesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);

        // Sort & offsets; copyback for memory coherency
        gpuSort.RunKeyGen();
        spatialOffsetsCalc.Run(false);
        keyarrbuffer.GetData(CPUKernelAOS.keyarr);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderCopybackKernel);
        spatialOffsets.GetData(CPUKernelAOS.spatialOffsets);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
        spatialIndices.GetData(CPUKernelAOS.spatialIndices);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: temperatureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updateStateKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);
        particleBuffer.GetData(CPUKernelAOS.particles);
        
    }

    void RunSimulationStepCPUGPUEveryFrame()
    {
         // Init + ext forces
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: SpawnParticlesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);

        // Sort & offsets; copyback for memory coherency
        gpuSort.Run();
        spatialOffsetsCalc.Run(false);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderCopybackKernel);
        keyarrbuffer.GetData(CPUKernelAOS.keyarr);
        spatialOffsets.GetData(CPUKernelAOS.spatialOffsets);
        spatialIndices.GetData(CPUKernelAOS.spatialIndices);
        particleBuffer.GetData(CPUKernelAOS.particles);
        runCPUComputeTest();
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: temperatureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updateStateKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);
    }
    void RunSimulationStep()
    {
        // Init + ext forces
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: SpawnParticlesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);

        // Sort & offsets; copyback for memory coherency
        gpuSort.Run();
        spatialOffsetsCalc.Run(false);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderCopybackKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: temperatureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updateStateKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);
    }
    void UpdateBoxColliderData()
    {
        // Update data array
        for (int i = 0; i < boxColliders.Length; i++)
        {
            Transform collider = boxColliders[i];
            // Modify properties directly
            boxColliderData[i].pos = collider.position;
            boxColliderData[i].size = collider.localScale;
            boxColliderData[i].zLocal = (Vector2)(collider.right); // Use right vector for orientation
        }
        // Update buffer
        boxCollidersBuffer.SetData(boxColliderData);
    }

    void UpdateCircleColliderData()
    {
        // Update data array
        for (int i = 0; i < circleColliders.Length; i++)
        {
            Transform collider = circleColliders[i];
            // Modify properties directly
            circleColliderData[i].pos = collider.position;
            circleColliderData[i].radius = collider.localScale.x * 0.5f; // Assuming uniform scale
        }
        // Update buffer
        circleCollidersBuffer.SetData(circleColliderData);
    }

    void UpdateSourceObjectData()
    {
        // Update data array
        for (int i = 0; i < sourceObjects.Length; i++)
        {
            Transform source = sourceObjects[i].transform;
            // Modify properties directly
            sourceObjectData[i].pos = source.position;
            sourceObjectData[i].radius = source.localScale.x * 0.5f; // Assuming uniform scale
            sourceObjectData[i].velo = sourceObjects[i].velo;
            sourceObjectData[i].spawnRate = sourceObjects[i].spawnRate;
            sourceObjectData[i].fluidType = sourceObjects[i].fluidType;
        }
        // Update buffer
        sourceObjectBuffer.SetData(sourceObjectData);
    }

    void UpdateDrainObjectData()
    {
        // Update data array
        for (int i = 0; i < drainObjects.Length; i++)
        {
            Transform drain = drainObjects[i];
            // Modify properties directly
            drainObjectData[i].pos = drain.position;
            drainObjectData[i].size = drain.localScale;
            drainObjectData[i].zLocal = (Vector2)(drain.right); // Use right vector for orientation  
        }
        // Update buffer
        drainObjectBuffer.SetData(drainObjectData);
    }

    void UpdateThermalBoxData()
    {
        // Update data array
        for (int i = 0; i < thermalBoxes.Length; i++)
        {
            ThermalBoxInitializer tBox = thermalBoxes[i];
            Transform collider = tBox.transform;
            // Modify properties directly
            thermalBoxData[i].box.pos = collider.position;
            thermalBoxData[i].box.size = collider.localScale;
            thermalBoxData[i].box.zLocal = (Vector2)(collider.right); // Use right vector for orientation
            thermalBoxData[i].temperature = tBox.temperature;
            thermalBoxData[i].conductivity = tBox.conductivity;
        }
        // Update buffer
        thermalBoxesBuffer.SetData(thermalBoxData);
    }

    void UpdateColliderData()
    {
        UpdateBoxColliderData();
        UpdateCircleColliderData();
        UpdateSourceObjectData();
        UpdateDrainObjectData();
        UpdateThermalBoxData();
    }


    void UpdateSettings(float deltaTime)
    {
        compute.SetFloat("deltaTime", deltaTime);
        compute.SetVector("boundsSize", boundsSize);
        compute.SetInt("numBoxColliders", boxColliders.Length);
        compute.SetInt("numCircleColliders", circleColliders.Length);
        compute.SetInt("numSourceObjs", sourceObjects.Length);
        compute.SetInt("numDrainObjs", drainObjects.Length);
        compute.SetInt("numThermalBoxes", thermalBoxes.Length);
        compute.SetInt("selectedFluidType", selectedFluid);
        compute.SetInt("edgeType", (int)edgeType);
        compute.SetInt("spawnRate", (int)spawnRate);
        compute.SetFloat("roomTemperature", roomTemperature);
        compute.SetFloat("globalEntropyRate", globalEntropyRate);

        uint[] atomicCounter = { 0, frameCounter++ };
        atomicCounterBuffer.SetData(atomicCounter);
        

        // Mouse interaction settings:
        HandleMouseInput();

        // CPU compute keys
        if (toggleCPUComputing)
        {
            CPUKernelAOS.deltaTime = deltaTime;
            if(cpuflip){
                compute.SetInt("numCPUKeys", (int)numCPUKeys);
            }else{
                compute.SetInt("numCPUKeys", 0);
            }
            
        }
        else
        {
            compute.SetInt("numCPUKeys", 0);
        }
    }
    void HandleScrollInput()
    {
        ApplySmoothing();

        float scrollDelta = Input.mouseScrollDelta.y;

        if (scrollDelta != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Apply scroll input to target strength with exponential scaling
                float scaleFactor = scrollDelta > 0 ? 1.1f : 0.9f;
                targetInteractionStrength *= Mathf.Pow(scaleFactor, Mathf.Abs(scrollDelta));
                targetInteractionStrength = Mathf.Clamp(targetInteractionStrength, minStrength, maxStrength);
            }
            else{
                // Apply scroll input to target radius with exponential scaling
                float scaleFactor = scrollDelta > 0 ? 1.1f : 0.9f;
                targetInteractionRadius *= Mathf.Pow(scaleFactor, Mathf.Abs(scrollDelta));
                targetInteractionRadius = Mathf.Clamp(targetInteractionRadius, minRadius, maxRadius);
            }
        }
        
        if (Input.GetMouseButtonDown(2))
        {
            setInteractionStrengthPercent(0.5f);
            targetInteractionRadius = 3f;
        }
    }
    void ApplySmoothing()
    {
        // Smoothly interpolate to the target radius
        interactionRadius = Mathf.SmoothDamp(interactionRadius,
            targetInteractionRadius,
            ref smoothRadiusVelocity,
            smoothingTime);

        // Smoothly interpolate to the target strength
        interactionStrength = Mathf.SmoothDamp(interactionStrength,
            targetInteractionStrength,
            ref smoothStrengthVelocity,
            smoothingTime);
    }

    void HandleMouseInput()
    {
        // Mouse interaction settings:
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isPullInteraction = Input.GetMouseButton(0);
        bool isPushInteraction = Input.GetMouseButton(1);
        float currInteractStrength = 0;

        if (brushState == BrushType.Gravity)
        {
            if (isPushInteraction || isPullInteraction)
            {
                currInteractStrength = isPushInteraction ? -interactionStrength : interactionStrength;
            }
        }
        else if (brushState == BrushType.Draw)
        {
            if (isPullInteraction)
            {
                currInteractStrength = 1f;
            }
            else if (isPushInteraction)
            {
                currInteractStrength = -1f;
            }
        }
        else if (brushState == BrushType.Eraser){
            if (isPushInteraction || isPullInteraction)
            {
                currInteractStrength = -1f;
            }
        }

        compute.SetInt("brushType", (int)brushState);
        compute.SetVector("interactionInputPoint", mousePos);
        compute.SetFloat("interactionInputStrength", currInteractStrength);
        compute.SetFloat("interactionInputRadius", interactionRadius);
    }

    void SetInitialBufferData(ParticleSpawner.ParticleSpawnData[] spawnData)
    {
        for (int k = 0; k < spawners.Length; k++)
        {
            spawnDataArr[k] = spawners[k].GetSpawnData();
        }
        Particle[] allPoints = new Particle[maxParticles];
        int idx = 0;
        int spawnerIdx = 0;

        foreach (ParticleSpawner.ParticleSpawnData spawnD in spawnData)
        {
            for (int i = 0; i < spawnD.positions.Length; i++)
            {
                // Early exit if we break max particle limit
                if (idx + i >= maxParticles)
                {
                    Debug.LogWarning($"Particle Spawner: Hit max particle count! Current spawner index: {spawnerIdx}; Spawner particle offset: {i}");
                    particleBuffer.SetData(allPoints);
                    return;
                }
                Particle p = new Particle
                {
                    position = spawnD.positions[i],
                    predictedPosition = spawnD.positions[i],
                    velocity = spawnD.velocities[i],
                    density = new float2(0, 0),
                    temperature = spawnD.temperature,
                    type = spawnD.type
                };
                allPoints[idx + i] = p;
            }
            idx += spawnD.positions.Length;
            spawnerIdx++;
        }

        // Fill empty space with disabled particles
        if (idx < maxParticles)
        {
            Debug.Log($"Particle Spawner: maxParticles > numParticles to spawn; filling scene with disabled particles. maxParticles: {maxParticles}, numParticles: {idx}");
            int numFill = maxParticles - idx - 1;
            ParticleSpawner.ParticleSpawnData spawnD = ParticleSpawner.GetSceneFill(numFill, boundsSize);
            for (int i = 0; i < spawnD.positions.Length; i++)
            {
                // Early exit if we break max particle limit, shouldn't happen
                if (idx + i >= maxParticles)
                {
                    Debug.LogWarning($"Particle Spawner: Hit max particle count during fill! Spawner particle offset: {i}");
                    particleBuffer.SetData(allPoints);
                    return;
                }
                Particle p = new Particle
                {
                    position = spawnD.positions[i],
                    predictedPosition = spawnD.positions[i],
                    velocity = spawnD.velocities[i],
                    density = new float2(0, 0),
                    temperature = spawnD.temperature,
                    type = spawnD.type
                };
                allPoints[idx + i] = p;
            }
        }

        particleBuffer.SetData(allPoints);
    }

    void HandleHotkeysInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            togglePause();
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
    void UpdateSideBarIcons()
    {
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

    private void ScanForAllObstaclesLists()
    {
        UpdateBoxColliders();
        UpdateCircleColliders();
        UpdateSourceObjects();
        UpdateDrainObjects();
        UpdateThermalBoxes();
    }

    public void UpdateBoxColliders(){
        boxColliders = GameObject.FindGameObjectsWithTag("BoxCollider")
            .Select(go => go.GetComponent<Transform>())
            .Concat(GameObject.FindGameObjectsWithTag("SolidThermalBox")
                .Select(go => go.GetComponent<Transform>()))
            .ToArray();
        
        boxColliderData = new OrientedBox[boxColliders.Length];
        
        ComputeHelper.Release(boxCollidersBuffer);
        boxCollidersBuffer = ComputeHelper.CreateStructuredBuffer<OrientedBox>(Mathf.Max(boxColliders.Length, 1));
        UpdateBoxColliderData();
        ComputeHelper.SetBuffer(compute, boxCollidersBuffer, "BoxColliders", updatePositionKernel);
    }
    public void UpdateCircleColliders()
    {
        circleColliders = GameObject.FindGameObjectsWithTag("CircleCollider")
            .Select(go => go.GetComponent<Transform>())
            .ToArray();

        circleColliderData = new Circle[circleColliders.Length];

        ComputeHelper.Release(circleCollidersBuffer);
        circleCollidersBuffer = ComputeHelper.CreateStructuredBuffer<Circle>(Mathf.Max(circleColliders.Length, 1));
        UpdateCircleColliderData();
        ComputeHelper.SetBuffer(compute, circleCollidersBuffer, "CircleColliders", updatePositionKernel);
    }
    public void UpdateSourceObjects()
    {
        GameObject[] sourceObjectGameObjects = GameObject.FindGameObjectsWithTag("SourceObject");
        sourceObjects = new SourceObjectInitializer[sourceObjectGameObjects.Length];

        // Update source objects
        for (int i = 0; i < sourceObjectGameObjects.Length; i++)
        {
            sourceObjects[i] = sourceObjectGameObjects[i].GetComponent<SourceObjectInitData>().sourceInitData;
        }
        sourceObjectData = new SourceObject[sourceObjectGameObjects.Length];

        ComputeHelper.Release(sourceObjectBuffer);
        sourceObjectBuffer = ComputeHelper.CreateStructuredBuffer<SourceObject>(Mathf.Max(sourceObjects.Length, 1));
        UpdateSourceObjectData();
        ComputeHelper.SetBuffer(compute, sourceObjectBuffer, "SourceObjs", SpawnParticlesKernel);
    }
    public void UpdateDrainObjects()
    {
        drainObjects = GameObject.FindGameObjectsWithTag("DrainObject")
            .Select(go => go.GetComponent<Transform>())
            .ToArray();

        drainObjectData = new OrientedBox[drainObjects.Length];

        ComputeHelper.Release(drainObjectBuffer);
        drainObjectBuffer = ComputeHelper.CreateStructuredBuffer<OrientedBox>(Mathf.Max(drainObjects.Length, 1));
        UpdateDrainObjectData();
        ComputeHelper.SetBuffer(compute, drainObjectBuffer, "DrainObjs", updatePositionKernel);
    }
    public void UpdateThermalBoxes()
    {
        GameObject[] thermalBoxGameObjects = GameObject.FindGameObjectsWithTag("ThermalBox");
        thermalBoxes = new ThermalBoxInitializer[thermalBoxGameObjects.Length];

        // Update thermal boxes
        for (int i = 0; i < thermalBoxGameObjects.Length; i++)
        {
            thermalBoxes[i] = thermalBoxGameObjects[i].GetComponent<ThermalBoxInitData>().thermalBoxInitData;
        }

        thermalBoxData = new ThermalBox[thermalBoxGameObjects.Length];

        ComputeHelper.Release(thermalBoxesBuffer);
        thermalBoxesBuffer = ComputeHelper.CreateStructuredBuffer<ThermalBox>(Mathf.Max(thermalBoxes.Length, 1));
        UpdateThermalBoxData();
        ComputeHelper.SetBuffer(compute, thermalBoxesBuffer, "ThermalBoxes", updatePositionKernel, temperatureKernel);
    }

    void OnDestroy()
    {
        ReleaseComputeBuffers();
    }

    public void ReleaseComputeBuffers()
    {
        ComputeHelper.Release(
        fluidDataBuffer,
        ScalingFactorsBuffer,
        particleBuffer,
        sortedParticleBuffer,
        spatialIndices,
        spatialOffsets,
        sortedIndices,
        boxCollidersBuffer,
        circleCollidersBuffer,
        sourceObjectBuffer,
        drainObjectBuffer,
        thermalBoxesBuffer,
        atomicCounterBuffer,
        cpuparticlebuffer,
        keyarrbuffer
    );

        gpuSort.Release();
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

        // Draw thermal boxes
        if (thermalBoxes != null)
        {
            foreach (ThermalBoxInitializer tBox in thermalBoxes)
            {
                Transform boxCollider = tBox.transform;
                if (boxCollider != null)
                {
                    Gizmos.DrawWireCube(boxCollider.position, boxCollider.localScale);
                }
            }
        }

        // Draw spawners
        if (spawners != null)
        {
            foreach (ParticleSpawner pSpawn in spawners)
            {
                pSpawn.OnDrawGizmos();
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

    public void setEdgeType(int edgeTypeIndex)
    {
        edgeType = (EdgeType)edgeTypeIndex;
    }
    public void setGravityMode(int gravityModeIndex)
    {
        return; // Not implemented
    }
    public void setSelectedFluid(int fluidTypeIndex)
    {
        selectedFluid = fluidTypeIndex;
    }

    public void SetBrushType(int brushTypeIndex)
    {
        brushState = (BrushType)brushTypeIndex;
    }

    public void togglePause()
    {
        isPaused = !isPaused;
        UpdateSideBarIcons();
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
        SetInitialBufferData(spawnDataArr);
        RunSimulationStep();
        SetInitialBufferData(spawnDataArr);
    }

    // These functions are for the fluid detector
    public bool IsPositionBufferValid()
    {
        return particleBuffer != null;
    }
    public ComputeBuffer GetParticleBuffer()
    {

        return particleBuffer;
    }
    public float[] GetParticleTemps()
    {
        float[] temps = new float[numParticles];
        particleBuffer.GetData(particleData);

        for (int i = 0; i < numParticles; i++)
        {
            temps[i] = particleData[i].temperature;
        }
        return temps;
    }
    public FluidType[] GetParticleTypes()
    {
        FluidType[] types = new FluidType[numParticles];
        particleBuffer.GetData(particleData);

        for (int i = 0; i < numParticles; i++)
        {
            types[i] = (FluidType)particleData[i].type;
        }
        return types;
    }
    public int GetParticleCount()
    {
        return numParticles;
    }
    public float getInteractionRadius()
    {
        return interactionRadius;
    }
    public float getInteractionStrength()
    {
        return interactionStrength;
    }

    public float getBrushSizePercent()
    {
        return Mathf.Clamp01((interactionRadius - minRadius) / (maxRadius - minRadius));
    }

    public float getBrushStrengthPercent()
    {
        return Mathf.Clamp01((interactionStrength - minStrength) / (maxStrength - minStrength));
    }

    public void setInteractionRadiusPercent(float val) // This takes a value between 0 and 1
    {
        targetInteractionRadius = Mathf.Lerp(minRadius, maxRadius, Mathf.Clamp01(val));
    }

    public void setInteractionStrengthPercent(float strength) // This takes a value between 0 and 1
    {
        targetInteractionStrength = Mathf.Lerp(minStrength, maxStrength, Mathf.Clamp01(strength));
    }
    public void setBounds(Vector2 bounds)
    {
        boundsSize = bounds;
    }
    public Vector2 getBounds()
    {
        return boundsSize;
    }
    public void setMaxParticles(int newMaxParticles)
    {
        maxParticles = newMaxParticles;
    }

    void initializeCPUKernelSettingsAoS()
    {
        CPUKernelAOS.numParticles = numParticles;
        CPUKernelAOS.offsets = new int2[9];

        CPUKernelAOS.offsets[0] = new int2(-1, 1);
        CPUKernelAOS.offsets[1] = new int2(0, 1);
        CPUKernelAOS.offsets[2] = new int2(1, 1);
        CPUKernelAOS.offsets[3] = new int2(-1, 0);
        CPUKernelAOS.offsets[4] = new int2(0, 0);
        CPUKernelAOS.offsets[5] = new int2(1, 0);
        CPUKernelAOS.offsets[6] = new int2(-1, -1);
        CPUKernelAOS.offsets[7] = new int2(0, -1);
        CPUKernelAOS.offsets[8] = new int2(1, -1);

        int numFluidTypes;
        if (!manuallySelectFluidTypes)
            numFluidTypes = Enum.GetValues(typeof(FluidType)).Length - 1;
        else
            numFluidTypes = fluidDataArray.Length;

        CPUKernelAOS.fluidParams = new FluidParam[numFluidTypes];
        CPUKernelAOS.scalingFactors = new ScalingFactors[numFluidTypes];
        CPUKernelAOS.maxSmoothingRadius = maxSmoothingRadius;
        CPUKernelAOS.boxCollidersData = new OrientedBox[boxColliders.Length];
        CPUKernelAOS.circleCollidersData = new Circle[circleColliders.Length];
        CPUKernelAOS.drainData = new OrientedBox[drainObjects.Length];
        CPUKernelAOS.sourceData = new Circle[sourceObjects.Length];
        CPUKernelAOS.spatialIndices = new uint2[numParticles];
        CPUKernelAOS.spatialOffsets = new uint[numParticles];
        CPUKernelAOS.particles = new Particle[numParticles];
        fluidParamArr.CopyTo(CPUKernelAOS.fluidParams, 0);
        scalingFactorsArr.CopyTo(CPUKernelAOS.scalingFactors, 0);
        CPUKernelAOS.keyarr = new uint[numParticles];
    }

    public void runCPUComputeTest()
    {


        //Initialize all CPU buffers
        CPUKernelAOS.fluidParamBuffer = new NativeArray<FluidParam>(CPUKernelAOS.fluidParams.Length, Allocator.TempJob);
        CPUKernelAOS.scalingFactorsBuffer = new NativeArray<ScalingFactors>(CPUKernelAOS.scalingFactors.Length, Allocator.TempJob);
        CPUKernelAOS.spatialIndicesBuffer = new NativeArray<uint2>(numParticles, Allocator.TempJob);
        CPUKernelAOS.spatialOffsetsBuffer = new NativeArray<uint>(numParticles, Allocator.TempJob);
        CPUKernelAOS.particleBuffer = new NativeArray<Particle>(numParticles, Allocator.TempJob);
        CPUKernelAOS.offsets2DBuffer = new NativeArray<int2>(CPUKernelAOS.offsets.Length, Allocator.TempJob);
        CPUKernelAOS.particleResultBuffer = new NativeArray<Particle>(numParticles, Allocator.TempJob);
        CPUKernelAOS.keyarrbuffer = new NativeArray<uint>(numParticles, Allocator.TempJob);
        //copy data to CPU Buffers (need to figure out how to copy direct from compute buffer to nativearray)

        // keyarrbuffer.GetData(CPUKernelAOS.keyarr);
        CPUKernelAOS.keyarrbuffer.CopyFrom(CPUKernelAOS.keyarr);
        CPUKernelAOS.offsets2DBuffer.CopyFrom(CPUKernelAOS.offsets);
        CPUKernelAOS.fluidParamBuffer.CopyFrom(fluidParamArr);
        CPUKernelAOS.scalingFactorsBuffer.CopyFrom(scalingFactorsArr);
        // spatialIndices.GetData(CPUKernelAOS.spatialIndices);
        CPUKernelAOS.spatialIndicesBuffer.CopyFrom(CPUKernelAOS.spatialIndices);
        // spatialOffsets.GetData(CPUKernelAOS.spatialOffsets);
        CPUKernelAOS.spatialOffsetsBuffer.CopyFrom(CPUKernelAOS.spatialOffsets);
        // particleBuffer.GetData(CPUKernelAOS.particles);
        CPUKernelAOS.particleBuffer.CopyFrom(CPUKernelAOS.particles);
        CPUKernelAOS.particleResultBuffer.CopyFrom(CPUKernelAOS.particles);
        

        //create each job type and assign the buffers to the jobs
        CPUDensityCalcAoS densitycalc = new CPUDensityCalcAoS
        {
            numParticles = (uint)numParticles,
            maxSmoothingRadius = maxSmoothingRadius,
            densityOut = CPUKernelAOS.particleResultBuffer,
            particles = CPUKernelAOS.particleBuffer,
            spatialIndices = CPUKernelAOS.spatialIndicesBuffer,
            spatialOffsets = CPUKernelAOS.spatialOffsetsBuffer,
            fluidPs = CPUKernelAOS.fluidParamBuffer,
            scalingFacts = CPUKernelAOS.scalingFactorsBuffer,
            offsets2D = CPUKernelAOS.offsets2DBuffer,
            numCPUKeys = numCPUKeys,
            keyarr = CPUKernelAOS.keyarrbuffer,
        };

        CPUPressureCalcAoS pressureCalc = new CPUPressureCalcAoS
        {
            numParticles = (uint)numParticles,
            maxSmoothingRadius = maxSmoothingRadius,
            pressureOut = CPUKernelAOS.particleBuffer,
            particles = CPUKernelAOS.particleResultBuffer,
            spatialIndices = CPUKernelAOS.spatialIndicesBuffer,
            spatialOffsets = CPUKernelAOS.spatialOffsetsBuffer,
            fluidPs = CPUKernelAOS.fluidParamBuffer,
            scalingFacts = CPUKernelAOS.scalingFactorsBuffer,
            offsets2D = CPUKernelAOS.offsets2DBuffer,
            deltaTime = CPUKernelAOS.deltaTime,
            numCPUKeys = numCPUKeys,
            keyarr = CPUKernelAOS.keyarrbuffer,
        };

        CPUViscosityCalcAoS viscosityCalc = new CPUViscosityCalcAoS
        {
            numParticles = (uint)numParticles,
            maxSmoothingRadius = maxSmoothingRadius,
            viscosityOut = CPUKernelAOS.particleResultBuffer,
            particles = CPUKernelAOS.particleBuffer,
            spatialIndices = CPUKernelAOS.spatialIndicesBuffer,
            spatialOffsets = CPUKernelAOS.spatialOffsetsBuffer,
            fluidPs = CPUKernelAOS.fluidParamBuffer,
            scalingFacts = CPUKernelAOS.scalingFactorsBuffer,
            offsets2D = CPUKernelAOS.offsets2DBuffer,
            deltaTime = CPUKernelAOS.deltaTime,
            numCPUKeys = numCPUKeys,
            keyarr = CPUKernelAOS.keyarrbuffer,
        };

        //Create the threads to calculate either density, pressure, or velocity
        int workerThreads;
        workerThreads = JobsUtility.JobWorkerCount;

        JobHandle density = densitycalc.Schedule(numParticles, Mathf.Max(numParticles / (workerThreads * 2), 1));
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
        density.Complete();

        //data transfers required to merge CPU and GPU data
        cpuparticlebuffer.SetData(CPUKernelAOS.particleResultBuffer);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: mergeCPUParticlesKernel);
        // particleBuffer.GetData(CPUKernelAOS.particles);
        // CPUKernelAOS.particleResultBuffer.CopyFrom(CPUKernelAOS.particles); 
        // CPUKernelAOS.particleBuffer.CopyFrom(CPUKernelAOS.particles);

        JobHandle pressure = pressureCalc.Schedule(numParticles, Mathf.Max(numParticles / (workerThreads * 2), 1));
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
        pressure.Complete();

        cpuparticlebuffer.SetData(CPUKernelAOS.particleResultBuffer);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: mergeCPUParticlesKernel);
        // particleBuffer.GetData(CPUKernelAOS.particles);
        // CPUKernelAOS.particleResultBuffer.CopyFrom(CPUKernelAOS.particles);
        // CPUKernelAOS.particleBuffer.CopyFrom(CPUKernelAOS.particles);

        JobHandle viscosity = viscosityCalc.Schedule(numParticles, Mathf.Max(numParticles / (workerThreads * 2), 1));
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
        viscosity.Complete();


        cpuparticlebuffer.SetData(CPUKernelAOS.particleResultBuffer);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: mergeCPUParticlesKernel);


        CPUKernelAOS.fluidParamBuffer.Dispose();
        CPUKernelAOS.scalingFactorsBuffer.Dispose();
        CPUKernelAOS.spatialIndicesBuffer.Dispose();
        CPUKernelAOS.spatialOffsetsBuffer.Dispose();
        CPUKernelAOS.particleBuffer.Dispose();
        CPUKernelAOS.offsets2DBuffer.Dispose();
        CPUKernelAOS.particleResultBuffer.Dispose();
        CPUKernelAOS.keyarrbuffer.Dispose();
    }

    public SourceObjectInitializer GetFirstSourceObject()
    {
        return sourceObjects[0];
    }
    public void SetFirstSourceObject(SourceObjectInitializer source)
    {
        sourceObjects[0] = source;
    }
    public void setFixedTimestep(bool fixedTimestepVal)
    {
        fixedTimeStep = fixedTimestepVal;
    }
    public FluidData[] getFluidDataArray(){
        return fluidDataArray;
    }

}
