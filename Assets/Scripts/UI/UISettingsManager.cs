using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public TMP_Dropdown screenModeDropdown;

    void Start()
    {
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetVolume);

        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "Fullscreen", "Windowed", "Borderless"
        });

        screenModeDropdown.value = GetCurrentScreenModeIndex();
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void SetScreenMode(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.Windowed; break;
            case 2: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
        }

        Screen.fullScreen = (index != 1);
    }

    private int GetCurrentScreenModeIndex()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen: return 0;
            case FullScreenMode.Windowed: return 1;
            case FullScreenMode.FullScreenWindow: return 2;
            default: return 1;
        }
    }
}
