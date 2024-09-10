using System;
using UnityEngine;
using UnityEngine.Audio;

public class ConfigPanel : MonoBehaviour
{
    public AudioMixer audioMixer;

    // Master volume control
    public void OnMasterChange(float change)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(change) * 20);
    }

    // SFX volume control
    public void OnSFXChange(float change)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(change) * 20);
    }

    // Music volume control
    public void OnMusicChange(float change)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(change) * 20);
    }

    // Screen type change (0 for Windowed, 1 for Fullscreen)
    public void OnScreenTypeChange(int type)
    {
        switch (type)
        {
            default:
                Debug.LogWarning("Unknown screen type selected");
                break;
            case 0: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 1: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }

    // Resolution change (implement specific resolutions as needed)
    public void OnResolutionChange(int type)
    {
        switch (type)
        {
            default:
                Debug.LogWarning("Unknown resolution type selected");
                break;
            case 0:
                Screen.SetResolution(1920, 1080, Screen.fullScreenMode);
                break;
            case 1:
                Screen.SetResolution(1280, 720, Screen.fullScreenMode);
                break;
            case 2:
                Screen.SetResolution(1366, 768, Screen.fullScreenMode);
                break;
            case 3:
                Screen.SetResolution(2560, 1440, Screen.fullScreenMode);
                break;
            case 4:
                Screen.SetResolution(3840, 2160, Screen.fullScreenMode);
                break;
                // Add more cases for other resolutions if needed
        }
    }
}
