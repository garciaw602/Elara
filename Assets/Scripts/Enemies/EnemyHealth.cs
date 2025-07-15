using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; 
    private float currentHealth;
    public GameObject ammoBoxPrefab;//*****CAJA DE MUNICION
    public float spawnOffsetY = 0.5f;//***** Un valor de 0.5f es un buen punto de partida, ajusta según el tamaño de tu caja

    public float destroyDelay = 0f; //  Poner un retardo o no (0f = inmediato)

    private EnemySpawner mySpawner;




    void Start()
    {
        currentHealth = maxHealth; 
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        mySpawner = spawner;
    }

    // Este método será llamado por la bala cuando impacte
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; // Reduce la vida actual
    //    Debug.Log(gameObject.name + " ha recibido " + amount + " de daño. Vida actual: " + currentHealth);

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

        if (mySpawner != null)
        {
            mySpawner.EnemyDied(this.gameObject);
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
}
