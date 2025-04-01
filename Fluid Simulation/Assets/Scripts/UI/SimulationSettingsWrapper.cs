using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimulationSettingsWrapper : MonoBehaviour
{
    private GameObject simulation2DGameObject;
    private IFluidSimulation simulation2DScript;

    [Header("Simulation Settings")]
    [SerializeField] private TMP_Dropdown edgeTypeDropdown;
    [SerializeField] private TMP_Dropdown gravityModeDropdown;
    [SerializeField] private TMP_Dropdown fixedTimestepDropdown;

    [Header("UI Settings")]
    [SerializeField] private TMP_Dropdown HideFPSDropdown;
    [SerializeField] private TMP_Dropdown HideMouseCircleDropdown;
    [SerializeField] private TMP_Dropdown HideBenchmarkDropdown;

    private GameObject fpsDisplayObject;
    private GameObject mouseCircleObject;
    [SerializeField] private GameObject benchmarkObject;

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
        // Edge Type
        edgeTypeDropdown.onValueChanged.AddListener(setEdgeType);

        // Gravity Mode
        gravityModeDropdown.onValueChanged.AddListener(setGravityMode);

        // Fixed Timestep
        fixedTimestepDropdown.onValueChanged.AddListener(setTimestampMode);

        // FPS Display
        HideFPSDropdown.onValueChanged.AddListener(setHideFPS);
        fpsDisplayObject = FindFirstObjectByType<FPSDisplay>().gameObject;

        // Mouse Circle
        HideMouseCircleDropdown.onValueChanged.AddListener(setHideMouseCircle);
        mouseCircleObject = FindFirstObjectByType<InteractionRadiusVisualizer>().gameObject;

        // Benchmark Script
        HideBenchmarkDropdown.onValueChanged.AddListener(setHideBenchmark);
    }


    public void setEdgeType(int edgeTypeIndex)
    {
        simulation2DScript.setEdgeType(edgeTypeIndex);
    }

    public void setGravityMode(int gravityModeIndex)
    {
        simulation2DScript.setGravityMode(gravityModeIndex);
    }

    public void setHideFPS(int hideFPSIndex)
    {
        fpsDisplayObject.SetActive(hideFPSIndex == 0);
    }

    public void setHideMouseCircle(int hideMouseCircleIndex)
    {
        if (hideMouseCircleIndex == 2){
            mouseCircleObject.SetActive(true);
            mouseCircleObject.GetComponent<InteractionRadiusVisualizer>().alwaysShowNeutral = false;
            return;
        }
        
        mouseCircleObject.SetActive(hideMouseCircleIndex == 0);
        mouseCircleObject.GetComponent<InteractionRadiusVisualizer>().alwaysShowNeutral = true;
    }

    public void setHideBenchmark(int hideBenchmarkIndex)
    {
        benchmarkObject.SetActive(hideBenchmarkIndex != 0);
    }

    public void setTimestampMode(int timestampModeIndex)
    {
        simulation2DScript.setFixedTimestep(timestampModeIndex == 1);
    }
}
