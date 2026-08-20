using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : UIPanel, IBindable<SettingViewModel>
{
    [Header("Elements")]
    [SerializeField] private Slider masterVol;
    [SerializeField] private Slider sfxVol;
    [SerializeField] private Slider bgmVol;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private Button backBtn;

    SettingViewModel vm;

    public void Bind(SettingViewModel vm)
    {
        Unbind();
        this.vm = vm;

        if(resolutionDropdown.options.Count == 0)
        {
            this.vm.InitResolution();
            this.vm.SetResolutionOptions(ref resolutionDropdown);
        }

        if (screenModeDropdown.options.Count == 0)
        {
            this.vm.InitScreenMode();
            this.vm.SetFullScreenModeOptions(ref screenModeDropdown);
        }

        this.vm.OnChanged += Refresh;

        masterVol.onValueChanged.AddListener(this.vm.SetMaster);
        sfxVol.onValueChanged.AddListener(this.vm.SetSfx);
        bgmVol.onValueChanged.AddListener(this.vm.SetBgm);
        sensitivitySlider.onValueChanged.AddListener(this.vm.SetSensitivity);
        resolutionDropdown.onValueChanged.AddListener(this.vm.SetResolution);
        screenModeDropdown.onValueChanged.AddListener(this.vm.SetScreenMode);

        backBtn.onClick.AddListener(this.vm.ClickBack);

        Refresh();
    }

    private void Refresh()
    {
        masterVol.SetValueWithoutNotify(vm.Master);
        sfxVol.SetValueWithoutNotify(vm.Sfx);
        bgmVol.SetValueWithoutNotify(vm.Bgm);
        sensitivitySlider.SetValueWithoutNotify(vm.Sensitivity);
    }

    public void Unbind()
    {
        if(vm != null)
        {
            vm.OnChanged -= Refresh;

            masterVol.onValueChanged.RemoveListener(vm.SetMaster);
            sfxVol.onValueChanged.RemoveListener(vm.SetSfx);
            bgmVol.onValueChanged.RemoveListener(vm.SetBgm);
            sensitivitySlider.onValueChanged.RemoveListener(vm.SetSensitivity);
            resolutionDropdown.onValueChanged.RemoveListener(vm.SetResolution);
            screenModeDropdown.onValueChanged.RemoveListener(vm.SetScreenMode);

            backBtn.onClick.RemoveListener(vm.ClickBack);
        }
    }
}