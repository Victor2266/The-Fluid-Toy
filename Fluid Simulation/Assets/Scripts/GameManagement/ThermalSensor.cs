using UnityEngine;
using UnityEngine.Rendering;

// Averages fluid temperature in an area
public class ThermalSensor : FluidPropertySensor
{
    protected override void Start()
    {
        base.Start(); // Init
        // Meta settings (For printouts, debug)
        sensorName = "Thermal Sensor";
        propertyName = "Avg. Temperature";
        // Sensor values
        propertyThreshold = 0.5f;
        checkInterval = 0.1f;
        detectionRadius = 2f;
        eventTrigger = SensorEvent.Equals;
        continuousThrow = false;
        percentMargin = 0.1f;

        // Debug
        showDebugGizmos = true;
        showDebugLogs = true;
        showPropertyValue = true;
    }

    protected override bool PerformCheck(Particle[] particles)
    {
        Vector2 checkPosition = transform.position;
        float totalTemp = 0;
        int particleCount = 0;

        // Calculate density similar to the simulation's density calculation
        float sqrRadius = detectionRadius * detectionRadius;

        foreach (Particle particle in particles)
        {
            if (particle.type == FluidType.Disabled)
            {
                continue;
            }
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
        currentValue = totalTemp/particleCount;
        throwEvent = doCompare(currentValue, propertyThreshold);

        return throwEvent;
    }
}