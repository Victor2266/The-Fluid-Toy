using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;

public class SandboxSetupManager : MonoBehaviour
{

    public Slider sandboxSettingSlider;

    private void Start()
    {
        Initialize();
    }

    public void Initialize(){
        // Load sandbox settings and apply to the slider
        float savedSandboxPreset = PlayerPrefs.GetInt("SandboxPreset", 2);
        sandboxSettingSlider.value = savedSandboxPreset;
        sandboxSettingSlider.onValueChanged.AddListener(OnSandBoxPresetChanged);
    }

    private void OnSandBoxPresetChanged(float presetIndex)
    {
        PlayerPrefs.SetInt("SandboxPreset", (int) presetIndex);
        PlayerPrefs.Save();
    }
}