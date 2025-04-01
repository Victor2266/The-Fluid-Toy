using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaLevelManager : LevelManager
{
    [Header("Level References")]
    public ThermalSensor thermalSensor;
    
    [Header("Debug Settings")]
    public bool showTemperatureDebug = true;
    public bool forceWinOnKeyPress = false;
    public KeyCode winKey = KeyCode.F5;

    // Start is called before the first frame update
    void Start()
    {        
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

        // Check if thermal sensor has reached the temperature threshold (WIN CONDITION)
        if (thermalSensor.metThreshold)
        {
            if (!isHolding) // This is used to show the holding timer at the top once temperature threshold is met
            {
                isHolding = true;
                holdTimer = 0f;
                Debug.Log("Temperature threshold met, starting hold timer");
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
                Debug.Log("Hold timer complete, triggering win");
                TriggerWin();
            }
        }
        else
        {
            ResetHoldTimer();
        }
    }
    
    // Override OnGUI to add temperature debug info
    protected override void OnGUI()
    {
        // Call base implementation to show hold timer
        base.OnGUI();
        
        // Add temperature debug info
        if (showTemperatureDebug && thermalSensor != null)
        {
            GUILayout.BeginArea(new Rect(10, 40, 300, 100));
            GUILayout.Label($"Current Temperature: {thermalSensor.currentTemperature:F1}");
            GUILayout.Label($"Threshold: {thermalSensor.temperatureThreshold:F1}");
            GUILayout.Label($"Threshold Met: {thermalSensor.metThreshold}");
            GUILayout.Label($"Level Timer: {timer:F1}s");
            GUILayout.EndArea();
        }
    }
}
