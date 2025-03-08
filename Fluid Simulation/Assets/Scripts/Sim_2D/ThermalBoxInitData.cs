using UnityEngine;

public class ThermalBoxInitData : MonoBehaviour
{
    public ThermalBoxInitializer thermalBoxInitData;

    public Transform transform
    {
        get { return thermalBoxInitData.transform; }
        set { thermalBoxInitData.transform = value; }
    }
    public float temperature
    {
        get { return thermalBoxInitData.temperature; }
        set { thermalBoxInitData.temperature = value; }
    }
    public float conductivity
    {
        get { return thermalBoxInitData.conductivity; }
        set { thermalBoxInitData.conductivity = value; }
    }
}
