using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIPauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    private bool isPaused = false;
    public GameUIManager uiManager;

    public void ResumeGame()
    {
        uiManager.CloseAllUI();
    }

    public void RestartGame()
    {
        // Recarga la escena actual

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void GoToMainMenu()
    {

        Time.timeScale = 1f;
        // Carga la escena del menú principal
        SceneManager.LoadScene("MainMenu");
    }
}
