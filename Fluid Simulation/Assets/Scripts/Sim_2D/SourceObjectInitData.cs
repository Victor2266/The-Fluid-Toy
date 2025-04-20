using UnityEngine;

public class SourceObjectInitData : MonoBehaviour
{
    public SourceObjectInitializer sourceInitData;

    public Vector2 velo{
        get{
            return sourceInitData.velo;
        }
        set{
            sourceInitData.velo = value;
        }
    }

    public float spawnRate{
        get{
            return sourceInitData.spawnRate;
        }
        set{
            sourceInitData.spawnRate = Mathf.Clamp01(value);
        }
    }

    public FluidType fluidType{
        get{
            return sourceInitData.fluidType;
        }
        set{
            sourceInitData.fluidType = value;
        }
    }
}
