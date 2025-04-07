using UnityEngine;

public class WebGPUStutterFixer : MonoBehaviour
{
#if UNITY_WEBGPU
    void Update()
    {
        // Calling this each frame fixes the stuttering that occurs on lower FPS
        // It does not improve FPS, if anything it decreases it
        // This is a quirk of WebGPU
        UnityEngine.FrameTimingManager.CaptureFrameTimings();
    }
#endif
}
