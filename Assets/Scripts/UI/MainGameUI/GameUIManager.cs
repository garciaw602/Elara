using UnityEngine;
using UnityEngine.Audio;

public class GameUIManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject mainUIPanel;
    public GameObject gameOverPanel;
    public GameObject hudCanvas;

    private bool isPauseMenuOpen = false;
    private bool isMainUIOpen = false;
    public AudioSource audioSource;
    public bool isCanvasOpen = false;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCanvasOpen = false;
        CloseAllUI();
    }

    void Update()
    {
        // Toggle Pause Menu with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPauseMenuOpen = !isPauseMenuOpen;
            pauseMenuPanel.SetActive(isPauseMenuOpen);
            mainUIPanel.SetActive(false);
            isMainUIOpen = false;
            isCanvasOpen = isPauseMenuOpen; 

            Time.timeScale = isPauseMenuOpen ? 0f : 1f;
            Cursor.lockState = isPauseMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPauseMenuOpen;

        }

        // Toggle Main UI Panel with I, M, or Tab
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
        {
            isMainUIOpen = !isMainUIOpen;
            mainUIPanel.SetActive(isMainUIOpen);
            pauseMenuPanel.SetActive(false);
            isPauseMenuOpen = false;
            isCanvasOpen = isMainUIOpen;

            Time.timeScale = isMainUIOpen ? 0f : 1f;
            Cursor.lockState = isMainUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isMainUIOpen;
        }
    }

    // Manually toggle HUDCanvas visibility
    public void SetHUDActive(bool isActive)
    {
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(isActive);
        }
    }

    // Call this when the game ends
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        mainUIPanel.SetActive(false);
        isCanvasOpen = true;
        Time.timeScale = 0f;
    }

    public void CloseAllUI()
    {
        pauseMenuPanel.SetActive(false);
        mainUIPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        isPauseMenuOpen = false;
        isMainUIOpen = false;
        isCanvasOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
