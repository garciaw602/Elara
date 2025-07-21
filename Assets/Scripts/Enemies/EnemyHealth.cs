using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("UI Health Bar Settings")]
    [Tooltip("Prefab del Canvas de la barra de vida del enemigo.")]
    public GameObject healthBarUIPrefab;
    [Tooltip("Posición relativa de la barra de vida sobre el enemigo.")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    // Referencias a los componentes de la barra de vida instanciada
    private Slider healthBarSlider;
    private Transform healthBarCanvasTransform;

    [Header("Drops Settings")]
    public GameObject ammoBoxPrefab; //*****CAJA DE MUNICION
    public float spawnOffsetY = 0.5f; //***** Un valor de 0.5f es un buen punto de partida, ajusta según el tamaño de tu caja

    [Header("Death Settings")]
    public float destroyDelay = 0f; //  Poner un retardo o no (0f = inmediato)

    private EnemySpawner mySpawner;

    void Awake()
    {
        currentHealth = maxHealth;

        // --- Instanciar la barra de vida al inicio ---
        if (healthBarUIPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarUIPrefab, transform.position + healthBarOffset, Quaternion.identity);
            healthBarCanvasTransform = healthBarInstance.transform;
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();

            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = maxHealth;
                healthBarSlider.value = currentHealth;
            }

            // --- ¡NUEVO! Desactivar la barra de vida al instanciarla ---
            SetHealthBarVisibility(false);
        }
    }

    void Update()
    {
        // Asegura que la barra de vida siga al enemigo y mire a la cámara
        if (healthBarCanvasTransform != null)
        {
            healthBarCanvasTransform.position = transform.position + healthBarOffset;

            // Asegúrate de que mire a la cámara principal
            if (Camera.main != null)
            {
                healthBarCanvasTransform.LookAt(healthBarCanvasTransform.position + Camera.main.transform.rotation * Vector3.forward,
                                                 Camera.main.transform.rotation * Vector3.up);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " ha recibido " + amount + " de daño. Vida actual: " + currentHealth);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
            // --- ¡NUEVO! Asegurarse de que la barra se muestre si recibe daño ---
            SetHealthBarVisibility(true);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto!");

        // Lógica para soltar la caja de munición
        // ******** LÓGICA DE LA CAJA DE MUNICIÓN --
        if (ammoBoxPrefab == null) // Agregar esta comprobación
        {
            Debug.LogError("¡El Prefab de la caja de munición NO está asignado en el Inspector para " + gameObject.name + "!");
            return; // Salir de la función si el prefab no está asignado
        }
        Vector3 spawnPosition = transform.position + Vector3.up * spawnOffsetY;
        Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        // ******** FIN DE LA LÓGICA DE LA CAJA DE MUNICIÓN --

        if (mySpawner != null)
        {
            mySpawner.EnemyDied(this.gameObject);
        }
        else
        {
            Debug.LogWarning("EnemyHealth: Spawner no asignado para este enemigo. No se notificará su muerte.", this);
        }

        // --- Destruir la barra de vida cuando el enemigo muere ---
        if (healthBarCanvasTransform != null)
        {
            Destroy(healthBarCanvasTransform.gameObject);
        }

        // Destruye el GameObject del enemigo después de un pequeño retardo
        Destroy(gameObject, destroyDelay);
    }

    // --- ¡NUEVO MÉTODO! Controla la visibilidad de la barra de vida ---
    public void SetHealthBarVisibility(bool isVisible)
    {
        if (healthBarCanvasTransform != null)
        {
            healthBarCanvasTransform.gameObject.SetActive(isVisible);
        }
    }

    public void SetSpawner(EnemySpawner callingSpawner)
    {
        mySpawner = callingSpawner;
    }
}