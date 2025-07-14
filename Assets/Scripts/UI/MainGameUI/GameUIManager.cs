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

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
        }
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

            Time.timeScale = isPauseMenuOpen ? 0f : 1f;
        }

        // Toggle Main UI Panel with I, M, or Tab
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
        {
            isMainUIOpen = !isMainUIOpen;
            mainUIPanel.SetActive(isMainUIOpen);
            pauseMenuPanel.SetActive(false);
            isPauseMenuOpen = false;

            Time.timeScale = isMainUIOpen ? 0f : 1f;
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
        Time.timeScale = 0f;
    }
}
