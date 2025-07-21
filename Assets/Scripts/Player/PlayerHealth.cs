using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para Coroutines

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("La vida máxima del jugador. Usado para el límite del Slider de vida.")]
    public float maxHealth = 100f;

    [Header("UI References")]
    [Tooltip("Asigna aquí el Slider de tu barra de vida en el Canvas.")]
    public Slider healthBarSlider;
    [Tooltip("Asigna aquí el panel de Game Over que debe activarse al morir el jugador.")]
    public GameObject gameOverPanel;
    [Tooltip("Asigna aquí el GameObject de la UI que indica que el jugador está siendo atacado ")]
    public GameObject attackIndicatorUI; 

    [Header("Player Control References")]
    [Tooltip("Asigna aquí el script que controla el disparo del jugador (ej. Gun.cs).")]
    public MonoBehaviour playerShootingScript;

    // Coroutine para el efecto de ataque UI
    private Coroutine attackIndicatorCoroutine;

    void Start()
    {
        // Verifica que el GameManager exista y esté funcionando
        if (GameManager.Instance == null)
        {
            Debug.LogError("PlayerHealth: GameManager.Instance no encontrado. La vida no persistirá correctamente.", this);
            return;
        }

        // Configura el valor máximo del Slider de vida
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
        }

        // Inicializa la vida del jugador a la máxima en el GameManager 

        GameManager.Instance.SetPlayerHealth(maxHealth);

        // Actualiza la UI de vida
        UpdateHealthUI();

        // Asegura que el panel de Game Over esté inactivo al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Asegura que el indicador de ataque UI esté inactivo al inicio
        if (attackIndicatorUI != null)
        {
            attackIndicatorUI.SetActive(false);
        }

        // Asegura que los controles del jugador estén activos al inicio del nivel
        EnablePlayerControls(true);
    }

    /// <summary>
    /// Aplica daño al jugador, actualiza la vida en GameManager y la UI.
    /// </summary>
    /// <param name="amount">Cantidad de daño a recibir.</param>
    public void TakeDamage(float amount)
    {
        if (GameManager.Instance == null) return;

        // Reduce la vida en el GameManager
        GameManager.Instance.SetPlayerHealth(GameManager.Instance.playerHealth - amount);
        Debug.Log($"Player ha recibido {amount} de daño. Vida actual: {GameManager.Instance.playerHealth}");

        // Actualiza la interfaz de usuario de la vida
        UpdateHealthUI();

        // Mostrar el indicador de ataque UI 
        ShowAttackIndicator();

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

        // Aumenta la vida en el GameManager
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
            healthBarSlider.value = GameManager.Instance.playerHealth;
        }
    }

    /// <summary>
    /// Lógica a ejecutar cuando el jugador muere.
    /// </summary>
    void Die()
    {
        Debug.Log("¡Player ha muerto!");

        // Resetea los datos de vida y munición en el GameManager para la próxima partida
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }

        // Desactiva los controles del jugador (disparo, movimiento, cámara)
        EnablePlayerControls(false);

        // Asegura que el indicador de ataque se desactive al morir
        if (attackIndicatorUI != null)
        {
            attackIndicatorUI.SetActive(false);
        }

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
            Debug.LogWarning("PlayerHealth: No se ha asignado 'Player Shooting Script' en el Inspector. Asegúrate de hacerlo.", this);
        }


    }

    /// <summary>
    /// Activa el indicador de ataque UI por un corto período.
    /// </summary>
    public void ShowAttackIndicator()
    {
        if (attackIndicatorUI == null) return;

        // Si ya hay una coroutine activa, la detenemos para evitar parpadeos y que se superpongan
        if (attackIndicatorCoroutine != null)
        {
            StopCoroutine(attackIndicatorCoroutine);
        }
        // Inicia la coroutine para mostrar y luego ocultar el indicador. Ajusta la duración (0.5f) .
        attackIndicatorCoroutine = StartCoroutine(AttackIndicatorRoutine(0.5f));
    }

    /// <summary>
    /// Coroutine para mostrar el indicador de ataque y luego ocultarlo.
    /// </summary>
    /// <param name="duration">Tiempo en segundos que el indicador estará visible.</param>
    private IEnumerator AttackIndicatorRoutine(float duration)
    {
        attackIndicatorUI.SetActive(true); // Activa el GameObject de la UI
        yield return new WaitForSeconds(duration); // Espera el tiempo especificado
        attackIndicatorUI.SetActive(false); // Desactiva el GameObject de la UI
        attackIndicatorCoroutine = null; // Limpia la referencia a la coroutine
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reanuda el tiempo del juego
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor
        Cursor.visible = false; // Oculta el cursor

        // Recarga la escena actual.
        // El método Start() de este script se llamará de nuevo, activando los controles.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reanuda el tiempo
        Cursor.visible = true; // Asegura que el cursor sea visible en el menú
        SceneManager.LoadScene("MainMenu"); // Asume que tienes una escena llamada "MainMenu"
    }
}