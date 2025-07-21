using UnityEngine;
using UnityEngine.UI; // Necesario para Slider
using System.Collections; // Necesario para Coroutines

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
    public float spawnOffsetY = 0.5f; //***** Un valor de 0.5f es un buen punto de partida

    private EnemySpawner mySpawner; // Referencia al spawner si es que este enemigo fue spawneado


    void Awake()
    {
        currentHealth = maxHealth;
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
        if (healthBarCanvasTransform != null)
        {
            healthBarCanvasTransform.position = transform.position + healthBarOffset;
            if (Camera.main != null)
            {
                Vector3 lookAtDir = Camera.main.transform.position - healthBarCanvasTransform.position;
                lookAtDir.y = 0;
                if (lookAtDir != Vector3.zero)
                {
                    healthBarCanvasTransform.rotation = Quaternion.LookRotation(-lookAtDir);
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

    public void SetHealthBarVisibility(bool isVisible)
    {
        if (healthBarCanvasTransform != null)
        {
            healthBarCanvasTransform.gameObject.SetActive(isVisible);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto. Iniciando secuencia de caída y despawn.");

        // --- 1. Detener la IA y el NavMeshAgent (movimiento y daño) ---
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false; // Desactiva el script de IA (esto debería detener el daño)
        }

        UnityEngine.AI.NavMeshAgent navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false; // Desactiva el NavMeshAgent
            navMeshAgent.isStopped = true; 
            navMeshAgent.velocity = Vector3.zero;
        }

        // --- 2. Detener Sonidos ---
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop(); // Detiene cualquier sonido que se esté reproduciendo
            audioSource.enabled = false; // Desactiva el componente para que no reproduzca más
        }

        // --- 3. Desactivar Animaciones ---
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false; // Desactiva el componente Animator
        }

        // --- 4. Activar el Rigidbody para que caiga por gravedad ---
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Permite que la física lo mueva para la caída inicial
            rb.useGravity = true;   // Asegura que la gravedad lo jale

            // Aplica un pequeño torque inicial para una caída más natural (opcional)
            rb.AddTorque(Random.insideUnitSphere * 1.7f, ForceMode.Impulse);

            // Le pasamos el Rigidbody y el Collider para que la coroutine los desactive.
            Collider mainCollider = GetComponent<Collider>();
            StartCoroutine(StopPhysicsAndColliderAfterDelay(rb, mainCollider, 1.5f)); // Delay para la caída inicial
        }
        else
        {
            Debug.LogWarning("EnemyHealth: No se encontró un Rigidbody en el enemigo " + gameObject.name + ". No podrá caer por física.", this);
            // Si no hay Rigidbody, desactivar el collider principal de inmediato para evitar bloqueos.
            Collider mainCollider = GetComponent<Collider>();
            if (mainCollider != null)
            {
                mainCollider.enabled = false;
                Debug.Log("Enemy " + gameObject.name + " without Rigidbody: Collider disabled immediately.");
            }
        }

        // --- LÓGICA DE LA CAJA DE MUNICIÓN (existente) ---
        if (ammoBoxPrefab == null)
        {
            Debug.LogError("¡El Prefab de la caja de munición NO está asignado en el Inspector para " + gameObject.name + "!");
        }
        else
        {
            Vector3 spawnPosition = transform.position + Vector3.up * spawnOffsetY;
            Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        }

        // --- Destruir la barra de vida  ---
        if (healthBarCanvasTransform != null)
        {
            Destroy(healthBarCanvasTransform.gameObject);
        }

        // --- Notificar al spawner ---
        if (mySpawner != null)
        {
            mySpawner.EnemyDied(this.gameObject);
        }
        else
        {
            Debug.LogWarning("EnemyHealth: Spawner no asignado para este enemigo. No se notificará su muerte.", this);
        }

        // --- Notificar al GameManager ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementZombieKillCount();
        }
        else
        {
            Debug.LogError("EnemyHealth: GameManager.Instance no encontrado. El contador de zombies no se actualizará.");
        }

        // Destruir el GameObject del enemigo después de 20 segundos
        float despawnDelay = 20f;
        Destroy(gameObject, despawnDelay);
        Debug.Log($"Enemigo {gameObject.name} se destruirá completamente en {despawnDelay} segundos.");
    }

    // Coroutine para detener la física y el collider
    IEnumerator StopPhysicsAndColliderAfterDelay(Rigidbody rb, Collider col, float delay)
    {
        yield return new WaitForSeconds(delay); // Espera el tiempo especificado para la caída inicial

        if (rb != null)
        {
            rb.velocity = Vector3.zero;     // Detener cualquier movimiento lineal
            rb.angularVelocity = Vector3.zero; // Detener cualquier rotación
            rb.isKinematic = true;          // ¡Congela el Rigidbody en su posición actual!
            rb.useGravity = false;          // Deja de ser afectado por la gravedad
            rb.freezeRotation = true;       // Asegura que no gire más por si acaso
            Debug.Log("Física del enemigo detenida y congelada.");
        }

        if (col != null)
        {
            col.enabled = false; // Desactiva el collider para que sea atravesable y no cause daño
            Debug.Log("Collider del enemigo desactivado.");
        }
    }

    public void SetSpawner(EnemySpawner callingSpawner)
    {
        mySpawner = callingSpawner;
    }
}