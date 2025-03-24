using UnityEngine;
using UnityEngine.Rendering;

public class VirtualParticleManager : MonoBehaviour
{
    [Header("Virtual Particle Settings")]
    [Tooltip("Number of particles to use for each body")]
    public int particlesPerObject = 100; // Number of virtual particles per Rigidbody
    [Tooltip("Particle spacing")]
    public float particleSpacing = 0.1f; // Spacing between virtual particles
    [Tooltip("Automatically adds rigidbodies to this manager on start")]
    public bool scanOnStart = true;

    [Header("Managed entities")]
    public Rigidbody2D[] rigidbodyObjects; // Array of Rigidbody objects
    private Particle[][] virtualParticles; // Array to store virtual particles for each Rigidbody

    private GameObject simulationGameobject;
    private IFluidSimulation fluidSimulation;
    private Vector2[][] virtualParticleForces; // Virtual forces

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
        if (scanOnStart)
        {
            // FIXME ADD FILTERING FOR NON-BUOYANT OBJECTS
            rigidbodyObjects = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
            foreach (Rigidbody2D rBody in rigidbodyObjects)
            {
                //fSense.isManagedSensor = true;
            }
        }

        // Initialize virtual particles for each Rigidbody
        virtualParticles = new Particle[rigidbodyObjects.Length][];
        virtualParticleForces = new Vector2[rigidbodyObjects.Length][];
        for (int i = 0; i < rigidbodyObjects.Length; i++)
        {
            virtualParticles[i] = GenerateVirtualParticles(rigidbodyObjects[i], particlesPerObject, particleSpacing);
            virtualParticleForces[i] = new Vector2[particlesPerObject];
        }
    }
    void FixedUpdate()
    {
        // Update virtual particle positions based on Rigidbody transforms
        for (int i = 0; i < rigidbodyObjects.Length; i++)
        {
            UpdateVirtualParticles(rigidbodyObjects[i], virtualParticles[i]);
        }

        // Pass virtual particle data to your HLSL-based fluid simulation
        SendVirtualParticlesToFluidSimulation(virtualParticles);

        // Retrieve forces from the fluid simulation (this is just a placeholder)
        RetrieveForcesFromFluidSimulation(virtualParticleForces);

        // Apply forces to the Rigidbody
        for (int i = 0; i < rigidbodyObjects.Length; i++)
        {
            ApplyForcesToRigidbody(rigidbodyObjects[i], virtualParticles[i], virtualParticleForces[i]);
        }
    }

    // Generate virtual particles for a Rigidbody
    private Particle[] GenerateVirtualParticles(Rigidbody2D rb, int count, float spacing)
    {
        Particle[] particles = new Particle[count];
        Bounds bounds = rb.GetComponent<Collider>().bounds; // Use the Collider bounds to generate particles

        for (int i = 0; i < count; i++)
        {
            // Generate particles within the bounds of the Rigidbody's Collider
            Vector2 position = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            // Create a new Particle and set its position
            particles[i] = new Particle
            {
                type = FluidType.VirtualParticle,
                temperature = 22f,
                position = position
            };
        }

        return particles;
    }

    // Update virtual particle positions based on the Rigidbody's transform
    private void UpdateVirtualParticles(Rigidbody2D rb, Particle[] particles)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            // Transform the particle position from local space to world space
            particles[i].position = rb.transform.TransformPoint(particles[i].position);
        }
    }

    // Send virtual particle data to the fluid simulation
    private void SendVirtualParticlesToFluidSimulation(Particle[][] particles)
    {
        // Flatten the particle array for easier data transfer
        int totalParticles = 0;
        foreach (var pArray in particles)
        {
            totalParticles += pArray.Length;
        }

        Vector2[] flattenedParticlePositions = new Vector2[totalParticles];
        int index = 0;
        foreach (var pArray in particles)
        {
            foreach (var p in pArray)
            {
                flattenedParticlePositions[index++] = p.position;
            }
        }

        // Pass the flattened particle array to your HLSL-based fluid simulation
        // (e.g., using a compute buffer or texture)
        // Example:
        // ComputeBuffer particleBuffer = new ComputeBuffer(totalParticles, sizeof(float) * 3);
        // particleBuffer.SetData(flattenedParticlePositions);
        // fluidSimulationShader.SetBuffer("_VirtualParticles", particleBuffer);
    }

    // Retrieve forces from the fluid simulation (placeholder)
    private void RetrieveForcesFromFluidSimulation(Vector2[][] forces)
    {
        // This is a placeholder for retrieving forces from your HLSL-based fluid simulation
        // In practice, you would read the forces from a ComputeBuffer or texture
        for (int i = 0; i < forces.Length; i++)
        {
            for (int j = 0; j < forces[i].Length; j++)
            {
                // Example: Assign random forces for demonstration
                forces[i][j] = new Vector2(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                );
            }
        }
    }

    // Apply forces to the Rigidbody
    private void ApplyForcesToRigidbody(Rigidbody2D rb, Particle[] particles, Vector2[] forces)
    {
        Vector2 totalForce = Vector2.zero;
        float totalTorque = 0f;

        for (int i = 0; i < particles.Length; i++)
        {
            // Sum up the forces
            totalForce += forces[i];

            // Calculate torque (optional)
            Vector2 r = particles[i].position - rb.worldCenterOfMass;
            totalTorque += r.x * forces[i].y - r.y * forces[i].x; // Cross product in 2D
        }

        // Apply the total force and torque to the Rigidbody
        rb.AddForce(totalForce);
        rb.AddTorque(totalTorque);
    }
}