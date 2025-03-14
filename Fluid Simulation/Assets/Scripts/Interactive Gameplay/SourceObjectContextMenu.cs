using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SourceObjectContextMenu : EditableObject
{
    // Source Object Properties
    private SourceObjectInitData sourceObjectInitData;

    protected override void Awake()
    {
        base.Awake();
        sourceObjectInitData = GetComponent<SourceObjectInitData>();
    }

    protected override void SetupMenuControls()
    {
        // Call base class implementation first
        base.SetupMenuControls();
        
        // Add source-specific controls
        Transform velocityXInput = content.transform.Find("VelocityXInput");
        Transform velocityYInput = content.transform.Find("VelocityYInput");
        Transform fluidTypeInput = content.transform.Find("FluidTypeInput");
        Transform spawnRateInput = content.transform.Find("SpawnRateInput");
        
        if (velocityXInput != null && velocityYInput != null && fluidTypeInput != null && spawnRateInput != null)
        {
            // Set initial values
            TMP_InputField velocityXField = velocityXInput.GetComponentInChildren<TMP_InputField>();
            TMP_InputField velocityYField = velocityYInput.GetComponentInChildren<TMP_InputField>();
            TMP_Dropdown fluidTypeField = fluidTypeInput.GetComponentInChildren<TMP_Dropdown>();
            TMP_InputField spawnRateField = spawnRateInput.GetComponentInChildren<TMP_InputField>();
            
            velocityXField.text = sourceObjectInitData.velo.x.ToString("F2");
            velocityYField.text = sourceObjectInitData.velo.y.ToString("F2");
            // fluidTypeField.value = sourceObjectInitData.fluidType;
            spawnRateField.text = sourceObjectInitData.spawnRate.ToString("F2");

            // Clear existing dropdown options
            fluidTypeField.ClearOptions();

            PopulateFluidTypeDropdown(fluidTypeField);
            
            // Add listeners
            velocityXField.onEndEdit.AddListener((value) => {
                if (float.TryParse(value, out float newVelocityX))
                {
                    Vector2 newVelocity = new Vector2(newVelocityX, sourceObjectInitData.velo.y);
                    sourceObjectInitData.velo = newVelocity;
                    RescanForObstacles();
                }
            });
            
            velocityYField.onEndEdit.AddListener((value) => {
                if (float.TryParse(value, out float newVelocityY))
                {
                    Vector2 newVelocity = new Vector2(sourceObjectInitData.velo.x, newVelocityY);
                    sourceObjectInitData.velo = newVelocity;
                    RescanForObstacles();
                }
            });
            
            fluidTypeField.onValueChanged.AddListener((value) => {
                FluidData[] fluidDataArray = fluidSimulationScript.getFluidDataArray();
                sourceObjectInitData.fluidType = (int) fluidDataArray[value].fluidType;
                RescanForObstacles();
            });
            
            spawnRateField.onEndEdit.AddListener((value) => {
                if (float.TryParse(value, out float newSpawnRate))
                {
                    sourceObjectInitData.spawnRate = newSpawnRate;
                    RescanForObstacles();
                }
            });
            
        }
    }

    private void PopulateFluidTypeDropdown(TMP_Dropdown fluidTypeField){
            // Populate the dropdown with fluid types from fluidDataArray
            FluidData[] fluidDataArray = fluidSimulationScript.getFluidDataArray();
            if (fluidSimulationScript != null && fluidDataArray != null)
            {
                // Create a list to hold our dropdown options
                List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                
                // Add each fluid type from the fluidDataArray
                for (int i = 0; i < fluidDataArray.Length; i++)
                {
                    FluidData fluidData = fluidDataArray[i];
                    // Add option with the name of the fluid type
                    options.Add(new TMP_Dropdown.OptionData(fluidData.fluidType.ToString()));
                }
                
                // Add the options to the dropdown
                fluidTypeField.AddOptions(options);
                
                // Set the current value based on the source's fluid type
                // Make sure to find the correct index that matches the current fluid type
                int currentFluidTypeIndex = 0;
                for (int i = 0; i < fluidDataArray.Length; i++)
                {
                    if ((int)fluidDataArray[i].fluidType == sourceObjectInitData.fluidType)
                    {
                        currentFluidTypeIndex = i;
                        break;
                    }
                }
                fluidTypeField.value = currentFluidTypeIndex;
            }
    }
}
