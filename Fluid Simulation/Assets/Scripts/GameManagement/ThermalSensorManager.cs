using UnityEngine;
using UnityEngine.Rendering;

public class ThermalSensorManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Thermal Sensors")]
    public ThermalSensor[] thermalSensors;

    [Tooltip("How often to check temperature (overrides sensors)")]
    public float checkInterval = 0.1f;
    public bool scanForSensorsOnStart = true;

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
        if (scanForSensorsOnStart)
        {
            thermalSensors = FindObjectsByType<ThermalSensor>(FindObjectsSortMode.None);
            foreach (ThermalSensor tSense in thermalSensors)
            {
                tSense.isManagedSensor = true;
            }
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
                AsyncGPUReadback.Request(fluidSimulation.GetParticleBuffer(), CheckTemperatures);
                isRequestMade = true;
                foreach (ThermalSensor tSensor in thermalSensors)
                {
                    if (tSensor.isManagedSensor)
                    {
                        tSensor.isRequestMade = true;
                    }
                }
            }
            nextCheckTime = Time.time + checkInterval;
        }
    }


    // Performs fluid check as callback to async read
    void CheckTemperatures(AsyncGPUReadbackRequest request)
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

        foreach (ThermalSensor tSensor in thermalSensors)
        {
            if (!tSensor.isManagedSensor) continue; // Skip unmanaged sensors

            float sqrRadius = tSensor.detectionRadius * tSensor.detectionRadius;
            Vector2 checkPosition = tSensor.transform.position;
            float totalTemp = 0f;
            int particleCount = 0;

            foreach (Particle particle in particles)
            {
                if (particle.type == FluidType.Disabled) continue;

                Vector2 particlePos = particle.position;
                Vector2 offsetToParticle = particlePos - checkPosition;
                float sqrDstToParticle = Vector2.Dot(offsetToParticle, offsetToParticle);

                if (sqrDstToParticle < sqrRadius)
                {
                    totalTemp += particle.temperature;
                    particleCount++;
                }
            }

            // Update fluid presence flag
            bool previousState = tSensor.metThreshold;
            tSensor.currentTemperature = totalTemp / particleCount;
            tSensor.metThreshold = tSensor.doCompare(tSensor.currentTemperature);

            // Notify if state changed
            if (previousState != tSensor.metThreshold)
            {
                tSensor.OnTempThresholdMet();
            }

            tSensor.isRequestMade = false;
        }

        isRequestMade = false;
    }

    void OnDestroy()
    {
        if (isRequestMade)
        {
            AsyncGPUReadback.WaitAllRequests();
        }

    }
}