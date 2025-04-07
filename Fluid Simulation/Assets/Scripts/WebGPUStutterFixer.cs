using UnityEngine;

public class WebGPUStutterFixer : MonoBehaviour
{
#if UNITY_WEBGL
    void Update()
    {
        // Calling this each frame fixes the stuttering that occurs on lower FPS
        // It does not improve FPS, if anything it decreases it
        // This is a quirk of WebGPU
        // See github issue here: https://github.com/users/Victor2266/projects/1/views/1?pane=issue&itemId=105225582&issue=Victor2266%7CThe-Fluid-Toy%7C231
        UnityEngine.FrameTimingManager.CaptureFrameTimings();
    }
#endif
}
