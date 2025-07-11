using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "GameScene";
    public string optionsSceneName = "OptionsScene";
    public string creditsSceneName = "CreditsScene";

    [Header("Buttons")]
    public Button startButton;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(() => LoadScene(gameSceneName));

    }

    void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
