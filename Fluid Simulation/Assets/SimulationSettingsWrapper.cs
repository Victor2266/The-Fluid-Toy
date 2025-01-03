using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimulationSettingsWrapper : MonoBehaviour
{
    [SerializeField] GameObject simulation2DGameObject;
    private IFluidSimulation simulation2DScript;

    [SerializeField] private TMP_Dropdown edgeTypeDropdown;

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

    // Start is called before the first frame update
    void Start()
    {
        edgeTypeDropdown.onValueChanged.AddListener(setEdgeType);
    }


    public void setEdgeType(int edgeTypeIndex){
        simulation2DScript.setEdgeType(edgeTypeIndex);
    }
}
