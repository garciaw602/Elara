using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshProUGUI
using UnityEngine.SceneManagement; // Necesario para SceneManager

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Data (Persistente)")]
    [Tooltip("La vida actual del jugador. Persiste entre escenas.")]
    public float playerHealth = 100f;
    [Tooltip("La cantidad de munición actual del jugador. Persiste entre escenas.")]
    public int gunAmmo = 10;    
    [Tooltip("Cantidad de zombies eliminados. Persiste entre escenas.")]
    public int zombieKillCount = 0;
    


    //  VARIABLES PARA GUARDAR LOS VALORES INICIALES ---
    private float initialPlayerHealth;
    private int initialGunAmmo;
    private int initialZombieKillCount;
    [Header("UI References")]
    [Tooltip("Referencia al TextMeshProUGUI para mostrar la munición.")]
    private TextMeshProUGUI ammoText;
    [Tooltip("Referencia al TextMeshProUGUI para mostrar el contador de zombies.")]
    public TextMeshProUGUI killCountText;


    private void Awake()
    {
        // Implementación del patrón Singleton para asegurar que solo haya una instancia del GameManager.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destruye esta nueva instancia si ya existe una.
            return;
        }

        Instance = this; // Asigna esta instancia como la única.
        DontDestroyOnLoad(gameObject); // ¡Importante para la persistencia entre escenas!

        // Suscribirse al evento de carga de escenas. Esto es clave para re-conectar la UI.
        SceneManager.sceneLoaded += OnSceneLoaded;


        // --- Guardar los valores iniciales al primer Awake ---
        // Esto se ejecutará solo una vez al inicio del juego, no al cargar nuevas escenas si ya existe el GM.
        initialPlayerHealth = playerHealth;
        initialGunAmmo = gunAmmo;
        initialZombieKillCount = zombieKillCount; // NUEVO: Guardar el conteo inicial

        // Intentar encontrar las referencias de UI solo si no están asignadas (útil para la primera carga de escena).
        // Se recomienda que la asignación principal de estos TextMeshProUGUI se haga en el Inspector si es posible,
        // o que tengan nombres consistentes para encontrarlos automáticamente.
        GameObject foundAmmoTextObject = GameObject.Find("AmmoTxt"); // Usa el nombre exacto del GameObject en tu jerarquía
        if (foundAmmoTextObject != null)
        {
            ammoText = foundAmmoTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("GameManager: GameObject 'AmmoTxt' no encontrado en la escena inicial. Asegúrate de que existe y se llama así.");
        }

        GameObject foundKillCountTextObject = GameObject.Find("KillCountText"); // Asume que se llama el  objeto de UI "KillCountText"
        if (foundKillCountTextObject != null)
        {
            killCountText = foundKillCountTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("GameManager: GameObject 'KillCountText' no encontrado en la escena inicial. Asegúrate de que existe y se llama así.");
        }

        Debug.Log("GameManager Singleton listo y persistirá entre escenas.");
    }

    private void Update()
    {
        // Actualiza la UI de munición en cada frame.
        if (ammoText != null)
        {
            ammoText.text = gunAmmo.ToString();
        }

        if (killCountText != null)
        {
            killCountText.text = "Zombies: " + zombieKillCount.ToString();
        }
    }

    /// <summary>
    /// Se llama automáticamente cuando se carga una nueva escena.
    /// Utilizado para re-conectar las referencias de UI que son específicas de cada escena.
    /// </summary>
    /// <param name="scene">La escena que acaba de ser cargada.</param>
    /// <param name="mode">El modo de carga de la escena.</param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: Escena '{scene.name}' cargada. Re-conectando UI.");

        // Intenta encontrar el objeto de texto de la munición en la nueva escena.
        GameObject foundAmmoTextObject = GameObject.Find("AmmoTxt");
        if (foundAmmoTextObject != null)
        {
            ammoText = foundAmmoTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            ammoText = null; // Si no se encuentra, establece a null para evitar errores de referencia.
            Debug.LogWarning($"GameManager: 'AmmoTxt' no encontrado en la escena '{scene.name}'.");
        }

        //  Re-conectar el TextMeshProUGUI del contador de zombies ---
        GameObject foundKillCountTextObject = GameObject.Find("KillCountText");
        if (foundKillCountTextObject != null)
        {
            killCountText = foundKillCountTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            killCountText = null;
            Debug.LogWarning($"GameManager: 'KillCountText' no encontrado en la escena '{scene.name}'.");
        }
    }

    // Asegúrate de desuscribirte del evento para evitar Memory Leaks cuando el GameManager se destruya (aunque DontDestroyOnLoad lo evita en la práctica, es buena práctica si el GM fuera destruible).
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    /// <summary>
    /// Añade munición al total del jugador.
    /// </summary>
    /// <param name="amount">La cantidad de munición a añadir.</param>
    public void AddAmmo(int amount)
    {
        gunAmmo += amount;
        Debug.Log($"Munición añadida. Total: {gunAmmo}");
    }

    /// <summary>
    /// Reduce la munición del total del jugador, asegurando que no baje de cero.
    /// </summary>
    /// <param name="amount">La cantidad de munición a reducir.</param>
    public void ReduceAmmo(int amount)
    {
        gunAmmo -= amount;
        gunAmmo = Mathf.Max(0, gunAmmo); // Asegura que la munición no sea negativa.
        Debug.Log($"Munición reducida. Restante: {gunAmmo}");
    }

    /// <summary>
    /// Establece la vida actual del jugador, con un límite para no exceder la vida máxima.
    /// </summary>
    /// <param name="newHealth">La nueva cantidad de vida del jugador.</param>
    public void SetPlayerHealth(float newHealth)
    {
        // Asume un máximo de 100f para la vida del jugador, puedes ajustarlo a una variable si es necesario.
        playerHealth = Mathf.Clamp(newHealth, 0f, 100f);
        Debug.Log($"Salud del jugador actualizada en GameManager: {playerHealth}");
    }

    /// <summary>
    /// Incrementa el contador de zombies eliminados.
    /// Este método es llamado por EnemyHealth cuando un zombie muere.
    /// </summary>
    public void IncrementZombieKillCount()
    {
        zombieKillCount++; // Incrementa el contador.
        Debug.Log("Zombie eliminado! Total: " + zombieKillCount);
        // La UI se actualizará automáticamente en el método Update() del GameManager.
    }



    /// <summary>
    /// Reinicia la vida del jugador, la munición y el contador de zombies a sus valores iniciales.
    /// Debería llamarse cuando el juego se reinicia o el jugador vuelve al menú principal.
    /// </summary>
    public void ResetGameData()
    {
        playerHealth = initialPlayerHealth;
        gunAmmo = initialGunAmmo;
        zombieKillCount = initialZombieKillCount; // Reiniciar el contador de zombies
        Debug.Log("GameManager: Datos del juego reiniciados a valores iniciales.");
    }
}