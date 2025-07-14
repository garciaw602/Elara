using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIGraphicsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown qualityDropdown;
    public Toggle vSyncToggle;
    public Toggle showFPSToggle;
    public GameObject fpsCounter;

    void Start()
    {
        LoadQualityOptions();
        vSyncToggle.isOn = QualitySettings.vSyncCount > 0;
        showFPSToggle.isOn = fpsCounter != null && fpsCounter.activeSelf;

        qualityDropdown.onValueChanged.AddListener(SetQuality);
        vSyncToggle.onValueChanged.AddListener(SetVSync);
        showFPSToggle.onValueChanged.AddListener(ToggleFPS);
    }

    void LoadQualityOptions()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "Low", "Medium", "High", "Ultra"
        });

        int currentLevel = QualitySettings.GetQualityLevel();
        qualityDropdown.value = Mathf.Clamp(currentLevel, 0, 3);
        qualityDropdown.RefreshShownValue();
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    public void ToggleFPS(bool show)
    {
        if (fpsCounter != null)
            fpsCounter.SetActive(show);
    }
}
