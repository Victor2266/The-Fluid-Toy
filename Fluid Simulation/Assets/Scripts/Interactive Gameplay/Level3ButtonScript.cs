using System.Runtime.CompilerServices;
using UnityEngine;

public class Level3ButtonScript : MonoBehaviour
{
    public FluidDetector fluidDetector1;
    public FluidDetector fluidDetector2;
    public Gradient gradient;
    public Color disabledColor;
    public SpriteRenderer spriteRenderer;
    public Level3Manager manager;

    public bool buttonEnabled = false;
    public float timeToEnable = 10.0F;
    public bool enableWin = false;
    private float TTL;
    void Start()
    {
        if(fluidDetector1 == null || fluidDetector2 == null){
            Debug.LogError("Error fluid detectors not connected to button script");
        }
        if(spriteRenderer == null){
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (manager == null){
            Debug.LogError("Error no manager connected to button script");
        }
    }

    void OnMouseDown()
    {
        if(enableWin)
            manager.buttonWin();
        
    }
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
        }

        gradientUpdate();
    }


    void gradientUpdate()
    {
        if(!buttonEnabled){
            spriteRenderer.color = disabledColor;
        }else{
            float t = Remap(fluidDetector2.currentDensity, 0, 70, 0, 1);
            spriteRenderer.color = gradient.Evaluate(t);
        }
    }

    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
	    return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
    }
}
