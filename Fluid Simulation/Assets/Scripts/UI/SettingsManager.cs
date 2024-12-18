using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;

public class SettingsManager : MonoBehaviour
{
    //[Header("Level Management")]
    public static int NumberOfLevels { get; private set; } = 15; //This is where you edit the total number of levels

    [Header("Resolution Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixer bgmMixer;
    [SerializeField] private AudioMixer sfxMixer;
    
    [Header("UI Controls")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;
    
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string RESOLUTION_KEY = "Resolution";

    private void Start()
    {
        InitializeResolutionDropdown();
        InitializeFullscreenToggle();
        InitializeVolumeSliders();
        LoadSavedSettings();
    }

    private void InitializeResolutionDropdown()
    {
        resolutions = Screen.resolutions
            .Select(resolution => new Resolution { 
                width = resolution.width, 
                height = resolution.height,
                refreshRateRatio = resolution.refreshRateRatio 
            })
            .Distinct()
            .OrderByDescending(r => r.width)
            .ToArray();

        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            float refreshRate = (float)resolutions[i].refreshRateRatio.value;  // Explicit cast to float
            string option = $"{resolutions[i].width}x{resolutions[i].height} @{refreshRate:F0}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void InitializeFullscreenToggle()
    {
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void InitializeVolumeSliders()
    {
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        // Initialize value texts
        UpdateBGMValueText(bgmSlider.value);
        UpdateSFXValueText(sfxSlider.value);
    }

    private void LoadSavedSettings()
    {
        // Load and apply volume settings
        float savedBGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        
        bgmSlider.value = savedBGMVolume;
        sfxSlider.value = savedSFXVolume;
        
        // Apply the saved volumes to the mixers
        SetBGMVolume(savedBGMVolume);
        SetSFXVolume(savedSFXVolume);
        
        // Update text displays
        UpdateBGMValueText(savedBGMVolume);
        UpdateSFXValueText(savedSFXVolume);
        
        // Load and apply fullscreen setting
        bool savedFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        fullscreenToggle.isOn = savedFullscreen;
        
        // Load and apply resolution
        int savedResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, currentResolutionIndex);
        resolutionDropdown.value = savedResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void OnResolutionChanged(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(RESOLUTION_KEY, index);
        PlayerPrefs.Save();
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnBGMVolumeChanged(float volume)
    {
        SetBGMVolume(volume);
        UpdateBGMValueText(volume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    private void OnSFXVolumeChanged(float volume)
    {
        SetSFXVolume(volume);
        UpdateSFXValueText(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    private void UpdateBGMValueText(float normalizedValue)
    {
        if (bgmValueText != null)
        {
            bgmValueText.text = $"{Mathf.RoundToInt(normalizedValue * 100)}%";
        }
    }

    private void UpdateSFXValueText(float normalizedValue)
    {
        if (sfxValueText != null)
        {
            sfxValueText.text = $"{Mathf.RoundToInt(normalizedValue * 100)}%";
        }
    }

    private void SetBGMVolume(float normalizedValue)
    {
        float dbValue = NormalizedToDecibels(normalizedValue);
        bgmMixer.SetFloat("BGMVolume", dbValue);//this is in the exposed parameters in the upper right of audio mizer tab
    }

    private void SetSFXVolume(float normalizedValue)
    {
        float dbValue = NormalizedToDecibels(normalizedValue);
        sfxMixer.SetFloat("SFXVolume", dbValue);
    }

    private float NormalizedToDecibels(float normalizedValue)
    {
        // Convert slider value (0 to 1) to decibels (-80dB to 0dB)
        // Using -80dB as minimum since it's essentially silence
        return normalizedValue > 0.0001f ? Mathf.Log10(normalizedValue) * 20 : -80f;
    }

    public void UnlockAllLevels()
    {
        Debug.Log("Unlocking all levels...");
        for (int i = 2; i <= NumberOfLevels; i++) // Start from 2 since level 1 is always unlocked
        {
            PlayerPrefs.SetInt($"Level_{i}_Unlocked", 1);
        }
        PlayerPrefs.Save();
    }

    public void LockAllLevels()
    {
        Debug.Log("Locking all levels...");
        for (int i = 2; i <= NumberOfLevels; i++) // Start from 2 since level 1 should remain unlocked
        {
            PlayerPrefs.SetInt($"Level_{i}_Unlocked", 0);
        }
        // Ensure level 1 stays unlocked
        PlayerPrefs.SetInt("Level_1_Unlocked", 1);
        PlayerPrefs.Save();
    }
}