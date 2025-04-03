using UnityEngine;
using UnityEngine.Rendering;

public class SensorManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("How often to check temperature (overrides indiv. sensor values)")]
    public float checkInterval = 0.1f;
    [Tooltip("Automatically adds sensors in the scene to this manager")]
    public bool scanForSensorsOnStart = true;

    [Tooltip("Thermal Sensors")]
    public FluidSensor[] fluidSensors;

    private GameObject simulationGameobject;
    private IFluidSimulation fluidSimulation;
    private float nextCheckTime;

    private bool isRequestMade = false;

    void Start()
    {
        simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        // Find the fluid simulation in the scene
        fluidSimulation = simulationGameobject.GetComponent<IFluidSimulation>();
        if (fluidSimulation == null)
        {
            Debug.LogError("No Simulation2D found in the scene!");
            enabled = false;
            return;
        }
        // Update sensor list
        if (scanForSensorsOnStart)
        {
            scanForSensors();
        }
    }

    void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            //sends async data request to GPU after each check time.
            if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid())
                return;
            if (!isRequestMade)
            {
                AsyncGPUReadback.Request(fluidSimulation.GetParticleBuffer(), CheckSensors);
                isRequestMade = true;
            }
            nextCheckTime = Time.time + checkInterval;
        }
    }


    // Performs fluid check as callback to async read
    void CheckSensors(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU ASync Readback Error in Fluid Simulation Readback");
            return;
        }
        if (fluidSimulation == null || !fluidSimulation.IsPositionBufferValid() || this == null)
            return;

        // Create temporary array to get particle positions
        Particle[] particles = request.GetData<Particle>().ToArray();

        foreach (FluidSensor fSense in fluidSensors)
        {
            if (fSense.isManagedSensor)
            {
                if (fSense == null)
                {
                    continue;
                }
                fSense.CheckSensor(particles);
            }
        }
        isRequestMade = false;
    }

    public void scanForSensors()
    {
        fluidSensors = FindObjectsByType<FluidSensor>(FindObjectsSortMode.None);
        foreach (FluidSensor fSense in fluidSensors)
        {
            fSense.isManagedSensor = true;
        }
    }

    void OnDestroy()
    {
        if (isRequestMade)
        {
            AsyncGPUReadback.WaitAllRequests();
        }

    }
}