using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel; 
    private bool isPaused = false; 

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true); // Muestra el panel de pausa
        }
        Time.timeScale = 0f; // Pausa el tiempo del juego
        Cursor.lockState = CursorLockMode.None; // Desbloquea el cursor
        Cursor.visible = true; // Hace visible el cursor
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false); // Oculta el panel de pausa
        }
        Time.timeScale = 1f; // Reanuda el tiempo del juego
        Cursor.lockState = CursorLockMode.Locked; // Vuelve a bloquear el cursor
        Cursor.visible = false; // Oculta el cursor
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Carga la escena del menú principal
        SceneManager.LoadScene("MainMenu");
    }
}