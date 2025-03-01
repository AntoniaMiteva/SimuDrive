using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlides;
    [SerializeField] private Slider SFXSlides;
    [SerializeField] private Slider soundSlides;
    [SerializeField] public TMP_Dropdown resolutionDropdown;
    [SerializeField] public TMP_Dropdown qualityDropdown;
    [SerializeField] public Toggle fullscreenToggle;

    Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        LoadAllPreferences();
    }

    private void LoadAllPreferences()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            musicSlides.value = PlayerPrefs.GetFloat("musicVolume");
            SFXSlides.value = PlayerPrefs.GetFloat("SFXVolume");
            soundSlides.value = PlayerPrefs.GetFloat("soundVolume");

            SetMusicVolume();
            SetSFXVolume();
            SetSoundVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetSoundVolume();
        }

        if (PlayerPrefs.HasKey("qualityLevel"))
        {
            int qualityLevel = PlayerPrefs.GetInt("qualityLevel");
            qualityDropdown.value = qualityLevel;
            QualitySettings.SetQualityLevel(qualityLevel);
        }

        if (PlayerPrefs.HasKey("resolutionIndex"))
        {
            int resIndex = PlayerPrefs.GetInt("resolutionIndex");
            if (resIndex < resolutions.Length)
            {
                resolutionDropdown.value = resIndex;
                Resolution storedResolution = resolutions[resIndex];
                Screen.SetResolution(storedResolution.width, storedResolution.height, Screen.fullScreen);
            }
        }
        else
        {
            int currentResIndex = FindCurrentResolutionIndex();
            resolutionDropdown.value = currentResIndex;
        }

        if (PlayerPrefs.HasKey("fullscreen"))
        {
            bool isFullscreen = PlayerPrefs.GetInt("fullscreen") == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        resolutionDropdown.RefreshShownValue();
        if (qualityDropdown != null)
            qualityDropdown.RefreshShownValue();
    }

    private int FindCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0; 
    }

    public void SetResolution(int resolutionIndex)
    {
        PlayerPrefs.SetInt("resolutionIndex", resolutionIndex);

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetMusicVolume()
    {
        float volume = musicSlides.value;
        myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlides.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetSoundVolume()
    {
        float volume = soundSlides.value;
        myMixer.SetFloat("sound", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("soundVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        PlayerPrefs.SetInt("qualityLevel", qualityIndex);
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void BackButton()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main Menu");
    }
}