using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("La vida m�xima del jugador. Usado para el l�mite del Slider de vida.")]
    public float maxHealth = 100f;

    [Header("UI References")]
    [Tooltip("Asigna aqu� el Slider de tu barra de vida en el Canvas.")]
    public Slider healthBarSlider;
    [Tooltip("Asigna aqu� el panel de Game Over que debe activarse al morir el jugador.")]
    public GameObject gameOverPanel;

    [Header("Player Control References")]
    [Tooltip("Asigna aqu� el script que controla el disparo del jugador (ej. Gun.cs).")]
    public MonoBehaviour playerShootingScript; // <-- �Este es el campo donde arrastras tu script Gun.cs!



    void Start()
    {
        // Verifica que el GameManager exista y est� funcionando
        if (GameManager.Instance == null)
        {
            Debug.LogError("PlayerHealth: GameManager.Instance no encontrado. La vida no persistir� correctamente.", this);
            return;
        }

        // Configura el valor m�ximo del Slider de vida una sola vez al inicio
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
        }

        // Actualiza la UI de vida con el valor actual del GameManager
        UpdateHealthUI();

        // Asegura que el panel de Game Over est� inactivo al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Asegura que los controles del jugador (disparo, movimiento, etc.) est�n activos al inicio del nivel
        // Esto es crucial para reiniciar el juego despu�s de una muerte
        EnablePlayerControls(true);
    }

    /// <summary>
    /// Aplica da�o al jugador, actualiza la vida en GameManager y la UI.
    /// </summary>
    /// <param name="amount">Cantidad de da�o a recibir.</param>
    public void TakeDamage(float amount)
    {
        if (GameManager.Instance == null) return;

        // Reduce la vida en el GameManager y la actualiza (clamping est� dentro de SetPlayerHealth)
        GameManager.Instance.SetPlayerHealth(GameManager.Instance.playerHealth - amount);
        Debug.Log($"Player ha recibido {amount} de da�o. Vida actual: {GameManager.Instance.playerHealth}");

        // Actualiza la interfaz de usuario de la vida
        UpdateHealthUI();

        // Verifica si el jugador ha muerto
        if (GameManager.Instance.playerHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Cura al jugador, actualiza la vida en GameManager y la UI.
    /// </summary>
    /// <param name="amount">Cantidad de vida a recuperar.</param>
    public void Heal(float amount)
    {
        if (GameManager.Instance == null) return;

        // Aumenta la vida en el GameManager y la actualiza (clamping est� dentro de SetPlayerHealth)
        GameManager.Instance.SetPlayerHealth(GameManager.Instance.playerHealth + amount);
        Debug.Log($"Player se ha curado {amount}. Vida actual: {GameManager.Instance.playerHealth}");

        // Actualiza la interfaz de usuario de la vida
        UpdateHealthUI();
    }

    /// <summary>
    /// Actualiza el valor del Slider de la barra de vida con la vida actual del GameManager.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthBarSlider != null && GameManager.Instance != null)
        {
            // El valor del Slider siempre refleja la vida actual del GameManager
            healthBarSlider.value = GameManager.Instance.playerHealth;
        }
    }

    /// <summary>
    /// L�gica a ejecutar cuando el jugador muere.
    /// </summary>
    void Die()
    {
        Debug.Log("�Player ha muerto!");

        // Resetea los datos de vida y munici�n en el GameManager para la pr�xima partida
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }

        // Desactiva los controles del jugador (disparo, movimiento, c�mara)
        EnablePlayerControls(false);

        // Muestra la pantalla de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pausa el tiempo del juego
        Time.timeScale = 0f;

        // Muestra y desbloquea el cursor para interactuar con la UI de Game Over
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Activa o desactiva los scripts de control del jugador.
    /// </summary>
    /// <param name="enable">True para activar, False para desactivar.</param>
    private void EnablePlayerControls(bool enable)
    {
        // Activa/desactiva el script de disparo
        if (playerShootingScript != null)
        {
            playerShootingScript.enabled = enable;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: No se ha asignado 'Player Shooting Script' en el Inspector. Aseg�rate de hacerlo.", this);
        }

        // Aqu� podr�as a�adir otros scripts de control si los asignaste:
        // if (playerMovementScript != null) { playerMovementScript.enabled = enable; }
        // if (playerCameraLookScript != null) { playerCameraLookScript.enabled = enable; }

        // Si tienes otros scripts en el GameObject del jugador que quieres desactivar autom�ticamente,
        // puedes mantener un bucle general, pero es m�s espec�fico con referencias directas.
        // MonoBehaviour[] playerScriptsOnGameObject = GetComponents<MonoBehaviour>();
        // foreach (MonoBehaviour script in playerScriptsOnGameObject)
        // {
        //     // Evita desactivar este script (PlayerHealth) o scripts UI si est�n en el mismo GameObject
        //     if (script != this && !(script is UnityEngine.UI.Graphic) && !(script is CanvasRenderer)) 
        //     {
        //         script.enabled = enable;
        //     }
        // }
    }

    /// <summary>
    /// Reinicia el juego cargando la escena actual.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Reanuda el tiempo del juego
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor
        Cursor.visible = false; // Oculta el cursor

        // Recarga la escena actual.
        // El m�todo Start() de este script se llamar� de nuevo, activando los controles.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Carga la escena del men� principal.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reanuda el tiempo
        Cursor.visible = true; // Asegura que el cursor sea visible en el men�

        SceneManager.LoadScene("MainMenu"); // Asume que tienes una escena llamada "MainMenu"
    }
}