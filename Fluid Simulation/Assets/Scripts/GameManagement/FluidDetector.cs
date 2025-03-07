using UnityEngine;
using UnityEngine.Rendering;

public class FluidDetector : FluidPropertySensor
{
    protected override void Start()
    {
        base.Start();
        // Meta settings (For printouts, debug)
        sensorName = "Fluid Detector";
        propertyName = "Density";
        // Sensor settings
        propertyThreshold = 0.5f;
        checkInterval = 0.1f;
        detectionRadius = 2f;
        eventTrigger = SensorEvent.GreaterThan;
        continuousThrow = false; // If true, will continously execute "throwEvent" on event trigger
        percentMargin = 0f; // For threshold; only useful for "Equals"

        // Debug
        showDebugGizmos = true;
        showDebugLogs = true;
        showPropertyValue = true;
    }

    // Function executes asynchronously, contents must be threadsafe or program may crash
    protected override bool PerformCheck(Particle[] particles)
    {
        Vector2 checkPosition = transform.position;
        float totalDensity = 0f;

        // Calculate density similar to the simulation's density calculation
        float sqrRadius = detectionRadius * detectionRadius;

        foreach (Particle particle in particles)
        {
            if(particle.type == FluidType.Disabled){
                continue;
            }
            Vector2 particlePos = particle.position;
            Vector2 offsetToParticle = particlePos - checkPosition;
            float sqrDstToParticle = Vector2.Dot(offsetToParticle, offsetToParticle);

            if (sqrDstToParticle < sqrRadius)
            {
                float dst = Mathf.Sqrt(sqrDstToParticle);
                // Using a simplified density kernel for detection
                totalDensity += (1 - (dst / detectionRadius)) * (1 - (dst / detectionRadius));
            }
        }

        // Update fluid presence flag
        currentValue = totalDensity;
        throwEvent = doCompare(totalDensity, propertyThreshold);

        return throwEvent;
    }

    // Function executes asynchronously, contents must be threadsafe or program may crash
    protected override void ThrowEvent()
    {
        // You can add custom events or UnityEvents here to notify other scripts
        if (showDebugLogs)
            Debug.Log($"Fluid presence changed to: {throwEvent} at {gameObject.name}");
    }
}