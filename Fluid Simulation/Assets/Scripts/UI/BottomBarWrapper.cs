using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBarWrapper : MonoBehaviour
{
    [SerializeField] GameObject simulation2DGameObject;
    private IFluidSimulation simulation2DScript;

    void Awake()
    {
        // if the simulation object reference is not set, try to get it by tag
        if (simulation2DGameObject == null)
        {
            simulation2DGameObject = GameObject.FindGameObjectWithTag("Simulation");
        }
        // Get the interface implementation from the simulation object
        if (simulation2DGameObject != null)
        {
            simulation2DScript = simulation2DGameObject.GetComponent<IFluidSimulation>();
            if (simulation2DScript == null)
            {
                Debug.LogError("No IFluidSimulation implementation found on the simulation object!");
            }
        }
        else
        {
            Debug.LogError("Simulation object reference is missing!");
        }
    }

    public void setSelectedFluid(int fluidTypeIndex)
    {
        simulation2DScript.setSelectedFluid(fluidTypeIndex);
        simulation2DScript.SetBrushType(0);
    }

    public void SetBrushType(int brushTypeIndex)
    {
        simulation2DScript.SetBrushType(brushTypeIndex);
    }
}
