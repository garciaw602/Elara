using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; 
    private float currentHealth;

    //----- Barra de Vida UI ----
    public GameObject healthBarUIPrefab; 
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0); // Posici�n relativa de la barra sobre el enemigo
    private Slider healthBarSlider; // Referencia al Slider dentro del Canvas instanciado
    private Transform healthBarCanvasTransform; // Referencia al transform del Canvas para rotaci�n


    public GameObject ammoBoxPrefab;//*****CAJA DE MUNICION
    public float spawnOffsetY = 0.5f;//***** Un valor de 0.5f es un buen punto de partida, ajusta seg�n el tama�o de tu caja

    public float destroyDelay = 0f; //  Poner un retardo o no (0f = inmediato)

    private EnemySpawner mySpawner;




    void Awake()
    {
        currentHealth = maxHealth;
        // Si tienes pooling de objetos, esto se llamar� cuando el objeto se activa
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
                Debug.LogWarning("EnemyHealth: No se encontr� un componente Slider dentro del prefab de la barra de vida.", this);
            }
        }
        else
        {
            Debug.LogWarning("EnemyHealth: No se ha asignado un prefab de barra de vida en el Inspector.", this);
        }
    }

    void Update()
    {
        // --- Actualizar posici�n y rotaci�n de la barra de vida ---
        if (healthBarCanvasTransform != null)
        {
            // La barra de vida sigue la posici�n del enemigo con un offset
            healthBarCanvasTransform.position = transform.position + healthBarOffset;

            // Opcional: Hacer que la barra de vida siempre mire a la c�mara del jugador
            // Asume que la c�mara principal es la del jugador
            if (Camera.main != null)
            {
                // Solo rotar en el eje Y para que no se incline
                Vector3 lookAtDir = Camera.main.transform.position - healthBarCanvasTransform.position;
                lookAtDir.y = 0; // Importante para que no se incline
                if (lookAtDir != Vector3.zero)
                {
                    healthBarCanvasTransform.rotation = Quaternion.LookRotation(-lookAtDir); // -lookAtDir para que mire hacia la c�mara
                }
            }
        }
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        mySpawner = spawner;
    }

    // Este m�todo ser� llamado por la bala cuando impacte
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; // Reduce la vida actual
        //Debug.Log(gameObject.name + " ha recibido " + amount + " de da�o. Vida actual: " + currentHealth);

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
        
        
        
        
        
        //********* L�GICA DE LA CAJA DE MUNICI�N AL MORIR 
        if (ammoBoxPrefab == null) // A�ade esta comprobaci�n
        {
            Debug.LogError("�El Prefab de la caja de munici�n NO est� asignado en el Inspector para " + gameObject.name + "!");
            return; // Salir de la funci�n si el prefab no est� asignado
        }
        Vector3 spawnPosition = transform.position + Vector3.up * spawnOffsetY;
        Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        // ******** FIN DE LA L�GICA DE LA CAJA DE MUNICI�N ---

        //if (mySpawner != null)
        //{
        //    mySpawner.EnemyDied(this.gameObject);
        //}
        //else
        //{
        //    Debug.LogWarning("EnemyHealth: Spawner no asignado para este enemigo. No se notificar� su muerte.", this);
        //}

        // --- Destruir la barra de vida cuando el enemigo muere ---
        if (healthBarCanvasTransform != null)
        {
            Destroy(healthBarCanvasTransform.gameObject);
        }

        //  Agregar efectos de muerte:
        // - Reproducir una animaci�n de muerte
        // - Reproducir un sonido de muerte
        // - Instanciar una explosi�n o part�culas

        // Destruye el GameObject del enemigo despu�s de un peque�o retardo
        Destroy(gameObject, destroyDelay);

        // Desactivar el enemigo inmediatamente si se necesita algo m�s antes de destruirlo
        // gameObject.SetActive(false);
    }

    //public void SetSpawner(EnemySpawner callingSpawner)
    //{
    //    spawner = callingSpawner;
    //}
}
