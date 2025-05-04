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
    private string[] sandboxSettings = { "(4k)", "Low (8k)", "Medium (16k)", "High (32k)", "(64k)", "(128k)", "(256k)", "(512k)", "(1.024M)", "(2.048M)" }; //{"Very Low (4k)", "Low (8k)", "Medium (16k)", "High (32k)", "Ultra High (64k)"};


    // Higher Particle Count Settings
    private bool highParticleCountOptions = false;
    public AudioSource audioSource;
    public AudioClip showHiddenMenuSound;
    public TMP_Text subtitle1Text;
    private string subtitle1original;
    private string subtitle1alt = "<color=red>WARNING!";
    public TMP_Text subtitle2Text;
    private string subtitle2original;
    private string subtitle2alt = "\nTHESE SETTINGS CAN ABSOLUTELY OBLITERATE YOUR COMPUTER!\nPress [G] to return to lower settings";


    private void Start()
    {
        subtitle1original = subtitle1Text.text;
        subtitle2original = subtitle2Text.text;
        Initialize();
    }

    public void Initialize()
    {
        // Load sandbox settings and apply to the slider
        int savedSandboxPreset = PlayerPrefs.GetInt("SandboxPreset", 2);
        if (savedSandboxPreset >= 5)
        {
            ToggleHighParticleCounts();
            sandboxSettingSlider.value = savedSandboxPreset - 5;
        }
        else
        {
            highParticleCountOptions = false;
            sandboxSettingSlider.value = savedSandboxPreset;
        }

        sandboxSettingText.text = sandboxSettings[savedSandboxPreset];
        sandboxSettingSlider.onValueChanged.AddListener(OnSandBoxPresetChanged);
    }

    private void OnSandBoxPresetChanged(float presetIndex)
    {
        int presetIndexInt = Mathf.RoundToInt(presetIndex);
        if (highParticleCountOptions)
        {
            presetIndexInt += 5;
        }
        PlayerPrefs.SetInt("SandboxPreset", presetIndexInt);
        sandboxSettingText.text = sandboxSettings[presetIndexInt];
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleHighParticleCounts();
        }
    }

    void ToggleHighParticleCounts()
    {
        audioSource.PlayOneShot(showHiddenMenuSound, 0.5f);
        if (highParticleCountOptions)
        {
            subtitle1Text.text = subtitle1original;
            subtitle2Text.text = subtitle2original;
            highParticleCountOptions = false;
        }
        else
        {
            subtitle1Text.text = subtitle1alt;
            subtitle2Text.text = subtitle2alt;
            highParticleCountOptions = true;
        }
        OnSandBoxPresetChanged(sandboxSettingSlider.value);
    }
}