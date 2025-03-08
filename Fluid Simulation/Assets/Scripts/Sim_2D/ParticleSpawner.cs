using UnityEngine;
using Unity.Mathematics;

public class ParticleSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public FluidType type;
    public int particleCount;
    public Vector2 initialVelocity;
    public Vector2 spawnCentre;
    public Vector2 spawnSize;
    public float jitterStr;
    public float temperature;

    [Header("Debug")]
    public bool showSpawnBoundsGizmos;
    public Color wireFrameColor = new(1, 1, 0, 0.5f);

    public ParticleSpawnData GetSpawnData()
    {
        ParticleSpawnData data = new ParticleSpawnData(type, particleCount, temperature);
        var rng = new Unity.Mathematics.Random(42);

        float2 s = spawnSize;
        int numX = Mathf.CeilToInt(Mathf.Sqrt(s.x / s.y * particleCount + (s.x - s.y) * (s.x - s.y) / (4 * s.y * s.y)) - (s.x - s.y) / (2 * s.y));
        int numY = Mathf.CeilToInt(particleCount / (float)numX);
        int i = 0;

        for (int y = 0; y < numY; y++)
        {
            for (int x = 0; x < numX; x++)
            {
                if (i >= particleCount) break;

                float tx = numX <= 1 ? 0.5f : x / (numX - 1f);
                float ty = numY <= 1 ? 0.5f : y / (numY - 1f);

                float angle = (float)rng.NextDouble() * 3.14f * 2;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 jitter = dir * jitterStr * ((float)rng.NextDouble() - 0.5f);
                data.positions[i] = new Vector2((tx - 0.5f) * spawnSize.x, (ty - 0.5f) * spawnSize.y) + jitter + spawnCentre;
                data.velocities[i] = initialVelocity;
                i++;
            }
        }

        return data;
    }

    // Only defines fluid data. Spawn position and size of spawn area determined by ParticleSpawner.
    public struct ParticleSpawnData
    {
        public FluidType type;
        public float2[] positions;
        public float2[] velocities;
        public float temperature;

        public ParticleSpawnData(int num) // Old call, for compatibility
        {
            this.type = FluidType.Water;
            positions = new float2[num];
            velocities = new float2[num];
            this.temperature = 22f;
        }

        public ParticleSpawnData(FluidType type, int num, float temperature)
        {
            this.type = type;
            positions = new float2[num];
            velocities = new float2[num];
            this.temperature = temperature;
        }
    }

    // Gets ParticleSpawnData to fill scene with Disabled particles
    static public ParticleSpawnData GetSceneFill(int pCount, Vector2 sceneSize)
    {
        ParticleSpawnData data = new ParticleSpawnData(FluidType.Disabled, pCount, 0);

        var rng = new Unity.Mathematics.Random(42);

        float2 s = sceneSize;
        int numX = Mathf.CeilToInt(Mathf.Sqrt(s.x / s.y * pCount + (s.x - s.y) * (s.x - s.y) / (4 * s.y * s.y)) - (s.x - s.y) / (2 * s.y));
        int numY = Mathf.CeilToInt(pCount / (float)numX);
        int i = 0;

        for (int y = 0; y < numY; y++)
        {
            for (int x = 0; x < numX; x++)
            {
                if (i >= pCount) break;

                float tx = numX <= 1 ? 0.5f : x / (numX - 1f);
                float ty = numY <= 1 ? 0.5f : y / (numY - 1f);

                float angle = (float)rng.NextDouble() * 3.14f * 2;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 jitter = dir * 0;
                data.positions[i] = new Vector2((tx - 0.5f) * sceneSize.x, (ty - 0.5f) * sceneSize.y) + jitter;
                data.velocities[i] = 0;
                i++;
            }
        }

        return data;
    }

    public void OnDrawGizmos()
    {
        if (!showSpawnBoundsGizmos || Application.isPlaying) return;

        Gizmos.color = this.wireFrameColor;
        Gizmos.DrawWireCube(spawnCentre, Vector2.one * spawnSize);
    }
}
