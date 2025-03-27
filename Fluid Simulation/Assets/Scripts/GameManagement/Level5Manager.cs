using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Level5Manager : LevelManager
{
    [Header("Level References")]
    public FluidDetector fluidDetector;
    public ThermalSensor thermalSensor;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI planetStatusReportText;

    public Button TwinIonCannonButton;
    public Button DeathRayButton;
    public Button TractorBeamButton;
    public Button NeutronBombButton;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        if (fluidDetector == null) // Auto-find references if not assigned in inspector on start
        {
            fluidDetector = FindFirstObjectByType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in the scene!");
                enabled = false;
                return;
            }
        }

        if (thermalSensor == null) // Auto-find references if not assigned in inspector on start
        {
            thermalSensor = FindFirstObjectByType<ThermalSensor>();
            if (thermalSensor == null)
            {
                Debug.LogError("No ThermalSensor found in the scene!");
                enabled = false;
                return;
            }
        }
    }

    // Update is called once per frame
    // This script will check for the win conditions
    // this can be customized for each level
    void Update()
    {
        if (hasWon) return;
        timer += Time.deltaTime;

        // Update timer text
        timerText.text = $"TIME WASTED ON TASK: <size=16>{timer:F4}s</size>";

        // Update Planet Status Report
        planetStatusReportText.text = $"REMOVAL STATUS: <color=red>{Mathf.FloorToInt((1 - fluidDetector.currentDensity / 3000f) * 100f)}%</color>\n" +
                                      $"CLIMATE STATUS: <color=red>{(thermalSensor.currentTemperature > 550 ? "HOSTILE" : "HOSPITABLE")}</color>\n" +
                                      $"PLANET DENSITY: {fluidDetector.currentDensity:F0}g/cm³\n" +
                                      $"PLANET TEMPERATURE: {thermalSensor.currentTemperature:F0}C";

        // Check for any mouse input
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            lastMouseInputTime = Time.time;
            ResetHoldTimer();
            return;
        }

        // Only start counting after 0.5 seconds have past since last mouse input
        if (Time.time - lastMouseInputTime < 0.5f)
        {
            ResetHoldTimer();
            return;
        }

        // Check if fluid detector is above threshold (WIN CONDITION)
        if (!fluidDetector.isFluidPresent)
        {
            if (!isHolding) // This is used to show the holding timer at the top once fluid is detected
            {
                isHolding = true;
                holdTimer = 0f;
            }

            holdTimer += Time.deltaTime;

            // Update background music volume
            if (backgroundMusic != null)
            {
                float fadeStartThreshold = requiredHoldTime * fadeOutStartTime;
                if (holdTimer >= fadeStartThreshold)
                {
                    float fadeProgress = (holdTimer - fadeStartThreshold) / (requiredHoldTime - fadeStartThreshold);
                    fadeProgress = Mathf.Clamp01(fadeProgress);
                    backgroundMusic.volume = Mathf.Lerp(initialMusicVolume, 0f, fadeProgress);
                }
            }

            // Check if we've held for long enough
            if (holdTimer >= requiredHoldTime)
            {
                TriggerWin();
            }
        }
        else
        {
            ResetHoldTimer();
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}
