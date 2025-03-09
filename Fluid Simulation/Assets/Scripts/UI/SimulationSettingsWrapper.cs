using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimulationSettingsWrapper : MonoBehaviour
{
    private GameObject simulation2DGameObject;
    private IFluidSimulation simulation2DScript;

    [SerializeField] private TMP_Dropdown edgeTypeDropdown;
    [SerializeField] private TMP_Dropdown HideFPSDropdown;
    [SerializeField] private TMP_Dropdown HideMouseCircleDropdown;
    [SerializeField] private TMP_Dropdown HideBenchmarkDropdown;

    private GameObject fpsDisplayObject;
    private GameObject mouseCircleObject;
    private GameObject benchmarkObject;

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
        HideFPSDropdown.onValueChanged.AddListener(setHideFPS);
        fpsDisplayObject = FindFirstObjectByType<FPSDisplay>().gameObject;
        HideMouseCircleDropdown.onValueChanged.AddListener(setHideMouseCircle);
        mouseCircleObject = FindFirstObjectByType<InteractionRadiusVisualizer>().gameObject;
        HideBenchmarkDropdown.onValueChanged.AddListener(setHideBenchmark);
        benchmarkObject = FindFirstObjectByType<FrameTimeBenchmark>().gameObject;
    }


    public void setEdgeType(int edgeTypeIndex)
    {
        simulation2DScript.setEdgeType(edgeTypeIndex);
    }

    public void setHideFPS(int hideFPSIndex)
    {
        fpsDisplayObject.SetActive(hideFPSIndex == 0);
    }

    public void setHideMouseCircle(int hideMouseCircleIndex)
    {
        mouseCircleObject.SetActive(hideMouseCircleIndex == 0);
    }

    public void setHideBenchmark(int hideBenchmarkIndex)
    {
        benchmarkObject.SetActive(hideBenchmarkIndex == 0);
    }
}
