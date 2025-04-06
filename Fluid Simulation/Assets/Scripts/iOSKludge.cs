using UnityEngine;

public class iOSKludge : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        UnityEngine.FrameTimingManager.CaptureFrameTimings();
    }
}
