using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; 
    private float currentHealth;

    //----- Barra de Vida UI ----
    public GameObject healthBarUIPrefab; 
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0); // Posición relativa de la barra sobre el enemigo
    private Slider healthBarSlider; // Referencia al Slider dentro del Canvas instanciado
    private Transform healthBarCanvasTransform; // Referencia al transform del Canvas para rotación


    public GameObject ammoBoxPrefab;//*****CAJA DE MUNICION
    public float spawnOffsetY = 0.5f;//***** Un valor de 0.5f es un buen punto de partida, ajusta según el tamaño de tu caja

    public float destroyDelay = 0f; //  Poner un retardo o no (0f = inmediato)

    private EnemySpawner mySpawner;




    void Awake()
    {
        currentHealth = maxHealth;
        // Si tienes pooling de objetos, esto se llamará cuando el objeto se activa
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
                Debug.LogWarning("EnemyHealth: No se encontró un componente Slider dentro del prefab de la barra de vida.", this);
            }
        }
        else
        {
            Debug.LogWarning("EnemyHealth: No se ha asignado un prefab de barra de vida en el Inspector.", this);
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
                // Solo rotar en el eje Y para que no se incline
                Vector3 lookAtDir = Camera.main.transform.position - healthBarCanvasTransform.position;
                lookAtDir.y = 0; // Importante para que no se incline
                if (lookAtDir != Vector3.zero)
                {
                    healthBarCanvasTransform.rotation = Quaternion.LookRotation(-lookAtDir); // -lookAtDir para que mire hacia la cámara
                }
            }
        }
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        mySpawner = spawner;
    }

    // Este método será llamado por la bala cuando impacte
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; // Reduce la vida actual
        //Debug.Log(gameObject.name + " ha recibido " + amount + " de daño. Vida actual: " + currentHealth);

        // --- Actualizar el Slider de la barra de vida ---
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto!");
        
        
        
        
        
        //********* LÓGICA DE LA CAJA DE MUNICIÓN AL MORIR 
        if (ammoBoxPrefab == null) // Añade esta comprobación
        {
            Debug.LogError("¡El Prefab de la caja de munición NO está asignado en el Inspector para " + gameObject.name + "!");
            return; // Salir de la función si el prefab no está asignado
        }
        Vector3 spawnPosition = transform.position + Vector3.up * spawnOffsetY;
        Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        // ******** FIN DE LA LÓGICA DE LA CAJA DE MUNICIÓN ---

        //if (mySpawner != null)
        //{
        //    mySpawner.EnemyDied(this.gameObject);
        //}
        //else
        //{
        //    Debug.LogWarning("EnemyHealth: Spawner no asignado para este enemigo. No se notificará su muerte.", this);
        //}

        // --- Destruir la barra de vida cuando el enemigo muere ---
        if (healthBarCanvasTransform != null)
        {
            Destroy(healthBarCanvasTransform.gameObject);
        }

        //  Agregar efectos de muerte:
        // - Reproducir una animación de muerte
        // - Reproducir un sonido de muerte
        // - Instanciar una explosión o partículas

        // Destruye el GameObject del enemigo después de un pequeño retardo
        Destroy(gameObject, destroyDelay);

        // Desactivar el enemigo inmediatamente si se necesita algo más antes de destruirlo
        // gameObject.SetActive(false);
    }

    //public void SetSpawner(EnemySpawner callingSpawner)
    //{
    //    spawner = callingSpawner;
    //}
}
