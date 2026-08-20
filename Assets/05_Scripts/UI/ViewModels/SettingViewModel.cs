using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingViewModel
{
    UIManager ui;
    SoundManager soundManager;

    public float Master { get; set; } = 1f;
    public float Sfx { get; set; } = 1f;
    public float Bgm { get; set; } = 1f;
    public float Sensitivity { get; set; } = 1f;
    public event Action OnChanged;

    private List<Resolution> resolutions = new();
    private List<FullScreenMode> screenModes = new();

    public SettingViewModel(UIManager ui, SoundManager soundManager)
    {
        this.ui = ui;
        this.soundManager = soundManager;
    }

    public void SetMaster(float v)
    {
        this.Master = v;
        soundManager.MasterVolume = Master;
        OnChanged?.Invoke();
    }

    public void SetSfx(float v)
    {
        this.Sfx = v;
        soundManager.SFXVolume = Sfx;
        OnChanged?.Invoke();
    }

    public void SetBgm(float v)
    {
        this.Bgm = v;

        // BGM이 아직 재생 전일 수 있다. 다음 PlayBGM에도 반영되도록 값을 함께 보관한다.
        soundManager.BGMVolume = Bgm;
        if (soundManager.CurrentAudio != null)
            soundManager.CurrentAudio.volume = Bgm;
        OnChanged?.Invoke();
    }

    public void SetSensitivity(float v)
    {
        Sensitivity = v;
        OnChanged?.Invoke();
    }

    public void ClickBack()
    {
        ui.Show(UIKey.Setting, false);
    }

    public void InitResolution()
    {
        for (int i = 0; i < Screen.resolutions.Length; ++i)
        {
            if ((Screen.resolutions[i].width * 9 == Screen.resolutions[i].height * 16) && Screen.resolutions[i].width >= 1280)
                resolutions.Add(Screen.resolutions[i]);
        }
    }

    public void InitScreenMode()
    {
        screenModes = Enum.GetValues(typeof(FullScreenMode)).Cast<FullScreenMode>().ToList();
    }

    public void SetResolutionOptions(ref TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        int optionNum = 0;

        foreach(Resolution item in resolutions)
        {
            TMP_Dropdown.OptionData option = new()
            {
                text = item.width + " x " + item.height
            };
            dropdown.options.Add(option);

            if(item.width == Screen.width && item.height == Screen.height)
            {
                dropdown.value = optionNum;
            }

            optionNum++;
        }

        dropdown.RefreshShownValue();
    }

    public void SetResolution(int idx)
    {
        Resolution res = resolutions[idx];
        Screen.SetResolution(res.width, res.height, false);
    }

    public void SetFullScreenModeOptions(ref TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        int optionNum = 0;

        foreach(var item in screenModes)
        {
            TMP_Dropdown.OptionData option = new();
            option.text = item.ToString();
            dropdown.options.Add(option);

            optionNum++;
        }

        dropdown.RefreshShownValue();
    }

    public void SetScreenMode(int idx)
    {
        Screen.fullScreenMode = screenModes[idx];
    }
}