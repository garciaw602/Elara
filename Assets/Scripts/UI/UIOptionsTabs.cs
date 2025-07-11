using UnityEngine;
using UnityEngine.UI;

public class UIOptionsTabs : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject optionsPanel;

    [Header("Tab Buttons")]
    public Button buttonSettings;
    public Button buttonControllers;
    public Button buttonGraphics;
    public Button buttonClose;

    [Header("Tab Contents")]
    public GameObject settingsContent;
    public GameObject controllersContent;
    public GameObject graphicsContent;

    void Start()
    {
        optionsPanel.SetActive(false);

        buttonSettings.onClick.AddListener(() => ShowTab(settingsContent));
        buttonControllers.onClick.AddListener(() => ShowTab(controllersContent));
        buttonGraphics.onClick.AddListener(() => ShowTab(graphicsContent));
        buttonClose.onClick.AddListener(CloseOptions);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        ShowTab(settingsContent); // Default tab
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    private void ShowTab(GameObject tabToShow)
    {
        settingsContent.SetActive(false);
        controllersContent.SetActive(false);
        graphicsContent.SetActive(false);

        tabToShow.SetActive(true);
    }
}
