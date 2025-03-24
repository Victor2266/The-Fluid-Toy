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
    [Tooltip("Starting temperature of virtual particles")]
    public float startTemp = 22f;

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
            // Generate particles as well as force vectors for each particle
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

    private Particle[] GenerateVirtualParticles(Rigidbody2D rb, int count, float spacing)
    {
        // Generate virtual particles for a Rigidbody
        Collider2D collider = rb.GetComponent<Collider2D>();

        if (collider == null) return null; // Null return on null collider

        // Handle different colliders
        if (collider is PolygonCollider2D polyCollider)
        {
            return GenerateForPolygonCollider(polyCollider, count);
        }
        else if (collider is BoxCollider2D boxCollider)
        {
            return GenerateForBoxCollider(boxCollider, count);
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            return GenerateForCircleCollider(circleCollider, count);
        }


        return null; // Fall-thru, null return
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

    // ==============================
    // Collider type generators below
    // ==============================
    private Particle[] GenerateForPolygonCollider(PolygonCollider2D collider, int count)
    {
        Particle[] particles = new Particle[count];
        Vector2[] points = collider.points; // Get local space points
        float totalLength = 0f;

        // Calculate total perimeter length
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Length];
            totalLength += Vector2.Distance(current, next);
        }

        float spacing = totalLength / count;
        float currentDistance = 0f;
        int edgeIndex = 0;
        float edgeProgress = 0f;

        for (int i = 0; i < count; i++)
        {
            float targetDistance = i * spacing;

            // Find which edge this particle should be on
            while (currentDistance < targetDistance && edgeIndex < points.Length)
            {
                Vector2 current = points[edgeIndex];
                Vector2 next = points[(edgeIndex + 1) % points.Length];
                float edgeLength = Vector2.Distance(current, next);

                if (currentDistance + edgeLength >= targetDistance)
                {
                    edgeProgress = (targetDistance - currentDistance) / edgeLength;
                    break;
                }

                currentDistance += edgeLength;
                edgeIndex++;
            }

            // Get the two points of the current edge
            Vector2 p1 = points[edgeIndex];
            Vector2 p2 = points[(edgeIndex + 1) % points.Length];

            // Interpolate between them
            Vector2 localPosition = Vector2.Lerp(p1, p2, edgeProgress);
            Vector2 worldPosition = collider.transform.TransformPoint(localPosition);

            particles[i] = new Particle
            {
                type = FluidType.VirtualParticle,
                temperature = this.startTemp,
                position = worldPosition
            };
        }

        return particles;
    }

    private Particle[] GenerateForBoxCollider(BoxCollider2D collider, int count)
    {
        Particle[] particles = new Particle[count];
        Vector2 size = collider.size;
        Vector2 offset = collider.offset;
        Transform transform = collider.transform;

        // Calculate perimeter
        float perimeter = 2 * (size.x + size.y);
        float spacing = perimeter / count;

        for (int i = 0; i < count; i++)
        {
            float distance = i * spacing;
            Vector2 localPosition;

            // Top edge
            if (distance < size.x)
            {
                localPosition = new Vector2(-size.x / 2 + distance, size.y / 2);
            }
            // Right edge
            else if (distance < size.x + size.y)
            {
                localPosition = new Vector2(size.x / 2, size.y / 2 - (distance - size.x));
            }
            // Bottom edge
            else if (distance < 2 * size.x + size.y)
            {
                localPosition = new Vector2(size.x / 2 - (distance - size.x - size.y), -size.y / 2);
            }
            // Left edge
            else
            {
                localPosition = new Vector2(-size.x / 2, -size.y / 2 + (distance - 2 * size.x - size.y));
            }

            localPosition += offset;
            Vector2 worldPosition = transform.TransformPoint(localPosition);

            particles[i] = new Particle
            {
                type = FluidType.VirtualParticle,
                temperature = this.startTemp,
                position = worldPosition
            };
        }

        return particles;
    }

    private Particle[] GenerateForCircleCollider(CircleCollider2D collider, int count)
    {
        Particle[] particles = new Particle[count];
        float radius = collider.radius;
        Vector2 offset = collider.offset;
        Transform transform = collider.transform;

        for (int i = 0; i < count; i++)
        {
            float angle = 2 * Mathf.PI * i / count;
            Vector2 localPosition = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            localPosition += offset;
            Vector2 worldPosition = transform.TransformPoint(localPosition);

            particles[i] = new Particle
            {
                type = FluidType.VirtualParticle,
                temperature = this.startTemp,
                position = worldPosition
            };
        }

        return particles;
    }
}