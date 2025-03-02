using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BottomBarWrapper : MonoBehaviour
{
    [SerializeField] GameObject simulation2DGameObject;
    [SerializeField] AudioSource audioSource;
    private IFluidSimulation simulation2DScript;

    [SerializeField] Button[] FluidTypebuttons;



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

        FluidTypebuttons = gameObject.GetComponentsInChildren<Button>(true)
            .Where(b => b.gameObject.tag == "FluidTypeButton")
            .ToArray();
    }

    void OnEnable()
    {
        FluidTypebuttons = gameObject.GetComponentsInChildren<Button>(true)
            .Where(b => b.gameObject.tag == "FluidTypeButton")
            .ToArray();

        resetButtonStates();
    }

    public void setSelectedFluid(int fluidTypeIndex)
    {
        simulation2DScript.setSelectedFluid(fluidTypeIndex);
        simulation2DScript.SetBrushType(0);
        audioSource.Play();
        TooltipManager.Instance.SetLastSelectedFluid(((FluidType)fluidTypeIndex).ToString());
        resetButtonStates();
    }

    public void SetBrushType(int brushTypeIndex)
    {
        simulation2DScript.SetBrushType(brushTypeIndex);
        audioSource.Play();
        TooltipManager.Instance.SetLastSelectedFluid(((BrushType)brushTypeIndex).ToString());
        resetButtonStates();
    }

    private void resetButtonStates()
    {
        foreach (Button button in FluidTypebuttons)
        {
            if (TooltipManager.Instance.GetLastSelectedFluid() == null){ // At the start of level the last selected fluid is null
                button.interactable = true;
                return;
            }

            if (button.name == TooltipManager.Instance.GetLastSelectedFluid().Replace('_', ' '))
            {
                button.interactable = false;
            }
            else
            {
                button.interactable = true;
            }
        }
    }
}
