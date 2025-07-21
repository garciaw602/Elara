using UnityEngine;
using UnityEngine.UI; // Necesario para Slider

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("UI Health Bar Settings")]
    [Tooltip("Prefab del Canvas de la barra de vida del enemigo.")]
    public GameObject healthBarUIPrefab;
    [Tooltip("Posición relativa de la barra de vida sobre el enemigo.")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0); // Posición relativa de la barra sobre el enemigo

    // Referencias a los componentes de la barra de vida instanciada
    private Slider healthBarSlider; // Referencia al Slider dentro del Canvas instanciado
    private Transform healthBarCanvasTransform; // Referencia al transform del Canvas para rotación


    [Header("Drops Settings")]
    public GameObject ammoBoxPrefab; //*****CAJA DE MUNICION
    public float spawnOffsetY = 0.5f; //***** Un valor de 0.5f es un buen punto de partida, ajusta según el tamaño de tu caja

    [Header("Death Settings")]
    public float destroyDelay = 0f; //  Poner un retardo o no (0f = inmediato)

    private EnemySpawner mySpawner; // Referencia al spawner si es que este enemigo fue spawneado


    void Awake()
    {
        currentHealth = maxHealth;
        // Si tienes pooling de objetos, esto se llamará cuando el objeto se activa.
        // Si no tienes pooling, se llama al inicio.

        // --- Instanciar la barra de vida al inicio ---
        if (healthBarUIPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarUIPrefab, transform.position + healthBarOffset, Quaternion.identity);
            healthBarCanvasTransform = healthBarInstance.transform;

            // Encontrar el Slider dentro del Canvas instanciado
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();

            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = maxHealth;
                healthBarSlider.value = currentHealth;
            }
            else
            {
                Debug.LogWarning("EnemyHealth: No se encontró un componente Slider dentro del prefab de la barra de vida. Asegúrate de que el prefab lo contiene.", this);
            }
        }
        else
        {
            Debug.LogWarning("EnemyHealth: No se ha asignado un prefab de barra de vida en el Inspector. La barra de vida no se mostrará.", this);
        }
    }

    void Update()
    {
        // --- Actualizar posición y rotación de la barra de vida ---
        if (healthBarCanvasTransform != null)
        {
            // La barra de vida sigue la posición del enemigo con un offset
            healthBarCanvasTransform.position = transform.position + healthBarOffset;

            // Opcional: Hacer que la barra de vida siempre mire a la cámara del jugador
            // Asume que la cámara principal es la del jugador
            if (Camera.main != null)
            {
                // Solo rotar en el eje Y para que no se incline.
                // Apunta desde la barra hacia la cámara, luego invierte para que la UI mire correctamente.
                Vector3 lookAtDir = Camera.main.transform.position - healthBarCanvasTransform.position;
                lookAtDir.y = 0; // Importante para que la barra se mantenga vertical
                if (lookAtDir != Vector3.zero) // Evitar Quaternion.LookRotation(Vector3.zero)
                {
                    healthBarCanvasTransform.rotation = Quaternion.LookRotation(-lookAtDir); // -lookAtDir para que mire hacia la cámara
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Controla la visibilidad de la barra de vida del enemigo.
    /// </summary>
    /// <param name="isVisible">True para mostrar, False para ocultar.</param>
    public void SetHealthBarVisibility(bool isVisible)
    {
        if (healthBarCanvasTransform != null)
        {
            healthBarCanvasTransform.gameObject.SetActive(isVisible);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto.");

        // ******** LÓGICA DE LA CAJA DE MUNICIÓN ********
        // Asegúrate de que el prefab esté asignado antes de intentar instanciarlo.
        if (ammoBoxPrefab == null)
        {
            Debug.LogError("¡El Prefab de la caja de munición NO está asignado en el Inspector para " + gameObject.name + "!");
            // No retornamos aquí para permitir que el enemigo muera incluso si no hay drop.
        }
        else
        {
            Vector3 spawnPosition = transform.position + Vector3.up * spawnOffsetY;
            Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        }
        // ******** FIN DE LA LÓGICA DE LA CAJA DE MUNICIÓN ********

        // Notificar al spawner si este enemigo fue creado por uno.
        if (mySpawner != null)
        {
            mySpawner.EnemyDied(this.gameObject);
        }
        else
        {
            Debug.LogWarning("EnemyHealth: Spawner no asignado para este enemigo. No se notificará su muerte.", this);
        }

        // --- Notificar al GameManager que un zombie murió ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementZombieKillCount(); // Llama al método del GameManager.
        }
        else
        {
            Debug.LogError("EnemyHealth: GameManager.Instance no encontrado. El contador de zombies no se actualizará.");
        }
        // 


        // --- Destruir la barra de vida cuando el enemigo muere ---
        if (healthBarCanvasTransform != null)
        {
            Destroy(healthBarCanvasTransform.gameObject);
        }

        // Destruye el GameObject del enemigo después de un pequeño retardo.
        // Aquí puedes agregar efectos de muerte (animaciones, sonidos, partículas).
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// Usado por EnemySpawner para asignar una referencia a sí mismo.
    /// </summary>
    /// <param name="callingSpawner">El EnemySpawner que creó este enemigo.</param>
    public void SetSpawner(EnemySpawner callingSpawner)
    {
        mySpawner = callingSpawner;
    }
}