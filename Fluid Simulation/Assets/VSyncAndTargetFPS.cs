using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSyncAndTargetFPS : MonoBehaviour
{
    [SerializeField] private int vsyncCount = 0;
    [SerializeField] private int targetFrameRate = 144;

    // Start is called before the first frame update
    void Start()
    {   
        //  VSyncCount Documentation:
        //      If vSyncCount > 0, then the field Application.targetFrameRate is ignored, and the effective frame rate is the native refresh rate of the display divided by vSyncCount.
        //If vSyncCount == 1, rendering is synchronized to the vertical refresh rate of the display.
        //      If vSyncCount is set to 0, Unity does not synchronize rendering to vertical sync, and the field Application.targetFrameRate is instead used to pace the rendered frames.
        //      For example, if you're running the Editor on a 60 Hz display and vSyncCount == 2, then the target frame rate is 30 frames per second.


        //  Application.targetFrameRate Documentation:
        //      When QualitySettings.vSyncCount = 0 and Application.targetFrameRate = -1:
        //          Desktop: Content is rendered unsynchronized as fast as possible.
        //          Web: Content is rendered at the native display refresh rate.
        //          Android and iOS: Content is rendered at fixed 30 fps to conserve battery power, independent of the native refresh rate of the display.

        QualitySettings.vSyncCount = vsyncCount; // Set vSyncCount to 0 so that using .targetFrameRate is enabled.
        Application.targetFrameRate = targetFrameRate;
    }
}
