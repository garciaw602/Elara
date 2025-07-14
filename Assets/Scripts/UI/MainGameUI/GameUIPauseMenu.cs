using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIPauseMenu : MonoBehaviour
{
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false); // Hides the pause menu
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }
}
