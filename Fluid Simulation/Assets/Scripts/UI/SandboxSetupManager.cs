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
    public TMP_Text sandboxSettingText;

    private String[] sandboxSettings = {"(4k)", "Low (8k)", "Medium (16k)", "High (32k)", "(64k)"}; //{"Very Low (4k)", "Low (8k)", "Medium (16k)", "High (32k)", "Ultra High (64k)"};

    private void Start()
    {
        Initialize();
    }

    public void Initialize(){
        // Load sandbox settings and apply to the slider
        int savedSandboxPreset = PlayerPrefs.GetInt("SandboxPreset", 2);
        sandboxSettingSlider.value = savedSandboxPreset;
        sandboxSettingText.text = sandboxSettings[savedSandboxPreset];
        sandboxSettingSlider.onValueChanged.AddListener(OnSandBoxPresetChanged);
    }

    private void OnSandBoxPresetChanged(float presetIndex)
    {
        PlayerPrefs.SetInt("SandboxPreset", Mathf.RoundToInt(presetIndex));
        sandboxSettingText.text = sandboxSettings[Mathf.RoundToInt(presetIndex)];
        PlayerPrefs.Save();
    }
}