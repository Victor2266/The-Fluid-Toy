using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Manager : LevelManager
{
    [Header("Level References")]
    public FluidDetector fluidDetector;

    // Start is called before the first frame update
    void Start()
    {
        if (fluidDetector == null) // Auto-find references if not assigned in inspector on start
        {
            fluidDetector = FindObjectOfType<FluidDetector>();
            if (fluidDetector == null)
            {
                Debug.LogError("No FluidDetector found in the scene!");
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

        // Only start counting if we haven't had mouse input for at least 0.5 seconds
        if (Time.time - lastMouseInputTime < 0.5f)
        {
            ResetHoldTimer();
            return;
        }

        // Check if fluid detector is above threshold (WIN CONDITION)
        if (fluidDetector.isFluidPresent)
        {
            if (!isHolding)
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
}
