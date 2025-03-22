using System.Runtime.CompilerServices;
using UnityEngine;

public class PuzzleLevelButtonScript : MonoBehaviour
{
    [Header("Fluid Detectors for determining win eligibility")]
    public FluidDetector fluidDetector1;
    public FluidDetector fluidDetector2;

    [Header("Color Gradient for visual effect on button")]
    public Gradient gradient;
    public Color disabledColor;

    [Header("Sprite Reference")]
    public SpriteRenderer spriteRenderer;

    [Header("Manager Reference")]
    public PuzzleLevelManager manager;

    [Header("Button status flags and settings")]
    public bool buttonEnabled = false;
    public float timeToEnable = 10.0F;
    public bool enableWin = false;
    private float TTL;

    void Start()
    {
        if(fluidDetector1 == null || fluidDetector2 == null){
            Debug.LogError("Error fluid detectors not connected to button script");
        }
        // Pulls sprite renderer from object components if not specified
        if(spriteRenderer == null){
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (manager == null){
            Debug.LogError("Error no manager connected to button script");
        }
        //set sprite color
        gradientUpdate();
    }

    /// <summary>
    /// if eligible for win, calls the buttonWin() function in manager to toggle victory
    /// </summary>
    void OnMouseDown()
    {
        if(enableWin)
            manager.buttonWin();
        
    }

    /// <summary>
    /// Checks if smoke alarm has been triggered (buttonEnabled), if not triggered checks the smoke alarm detector to determine if fluid is present.
    /// If fluid is present, wait TTL time before enabling the button to prevent quick win.
    /// If all water has been boiled away from the button (fluiddetector 2 does not detect any fluid at all), then enableWin is set true.
    /// sprite color is updated every fixed update once buttonEnabled is true.
    /// </summary>
    void FixedUpdate()
    {
        if (!buttonEnabled){
            if (fluidDetector1.isFluidPresent){
                if(TTL <= 0){
                    buttonEnabled = true;
                }else{
                    TTL -= Time.deltaTime;
                }
            }else{
                TTL = timeToEnable;
            }
        }else{
            if(!fluidDetector2.isFluidPresent){
                enableWin = true;
            }
            gradientUpdate();
        }

        
    }

    /// <summary>
    /// Updates sprite renderer color depending on status of buttonEnabled and the current density near the button.
    /// </summary>
    void gradientUpdate()
    {
        if(!buttonEnabled){
            spriteRenderer.color = disabledColor;
        }else{
            float t = Remap(fluidDetector2.currentDensity, 0, 70, 0, 1);
            spriteRenderer.color = gradient.Evaluate(t);
        }
    }

    /// <summary>
    /// Remaps input value with range sourceFrom to SourceTo, to a value in range targetFrom to targetTo.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceFrom"></param>
    /// <param name="sourceTo"></param>
    /// <param name="targetFrom"></param>
    /// <param name="targetTo"></param>
    /// <returns>source value remapped to new limits.</returns>
    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }
}
