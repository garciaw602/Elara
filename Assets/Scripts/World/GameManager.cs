using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Data (Persistente)")]
    [Tooltip("La vida actual del jugador. Persiste entre escenas.")]
    public float playerHealth = 100f;
    [Tooltip("La cantidad de munición actual del jugador. Persiste entre escenas.")]
    public int gunAmmo = 10;

    // --- NUEVAS VARIABLES PARA GUARDAR LOS VALORES INICIALES ---
    private float initialPlayerHealth;
    private int initialGunAmmo;

    [Header("UI References")]
    [Tooltip("Referencia al TextMeshProUGUI para mostrar la munición.")]
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ¡Importante para la persistencia!

        // --- NUEVO: Guardar los valores iniciales al primer Awake ---
        // Esto se ejecutará solo una vez al inicio del juego.
        initialPlayerHealth = playerHealth;
        initialGunAmmo = gunAmmo;

        Debug.Log("GameManager Singleton listo y persistirá entre escenas.");
    }

    private void Update()
    {
        if (ammoText != null)
            ammoText.text = gunAmmo.ToString();
    }

    public void AddAmmo(int amount)
    {
        gunAmmo += amount;
        Debug.Log($"Munición añadida. Total: {gunAmmo}");
    }

    public void ReduceAmmo(int amount)
    {
        gunAmmo -= amount;
        gunAmmo = Mathf.Max(0, gunAmmo);
        Debug.Log($"Munición reducida. Restante: {gunAmmo}");
    }

    public void SetPlayerHealth(float newHealth)
    {
        playerHealth = Mathf.Clamp(newHealth, 0f, 100f); // Asume un máximo de 100f
        Debug.Log($"Salud del jugador actualizada en GameManager: {playerHealth}");
    }

    /// <summary>
    /// NUEVO: Reinicia la vida del jugador y la munición a sus valores iniciales.
    /// Debería llamarse cuando el juego se reinicia o el jugador vuelve al menú.
    /// </summary>
    public void ResetGameData()
    {
        playerHealth = initialPlayerHealth;
        gunAmmo = initialGunAmmo;
        Debug.Log("Datos de juego reiniciados (vida y munición).");
    }
}