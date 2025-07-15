using UnityEngine;
using UnityEngine.UI;
public class UIMainOptionsTabs : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject optionsPanel;
    public GameUIManager uiManager;

    [Header("Tab Buttons")]
    public Button buttonCharacter;
    public Button buttonMap;
    public Button buttonObjectives;
    public Button buttonClose;

    [Header("Tab Contents")]
    public GameObject characterContent;
    public GameObject mapContent;
    public GameObject objectivesContent;

    void Start()
    {
        optionsPanel.SetActive(false);

        buttonCharacter.onClick.AddListener(() => ShowTab(characterContent));
        buttonMap.onClick.AddListener(() => ShowTab(mapContent));
        buttonObjectives.onClick.AddListener(() => ShowTab(objectivesContent));
        buttonClose.onClick.AddListener(CloseOptions);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        ShowTab(characterContent); // Default tab
    }

    public void CloseOptions()
    {
        uiManager.CloseAllUI();
    }

    private void ShowTab(GameObject tabToShow)
    {
        characterContent.SetActive(false);
        mapContent.SetActive(false);
        objectivesContent.SetActive(false);

        tabToShow.SetActive(true);
    }
}
