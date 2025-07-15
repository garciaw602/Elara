using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; 
    private float currentHealth;
    public GameObject ammoBoxPrefab;//*****CAJA DE MUNICION
    public float spawnOffsetY = 0.5f;//***** Un valor de 0.5f es un buen punto de partida, ajusta seg�n el tama�o de tu caja

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

    // Este m�todo ser� llamado por la bala cuando impacte
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; // Reduce la vida actual
    //    Debug.Log(gameObject.name + " ha recibido " + amount + " de da�o. Vida actual: " + currentHealth);

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

        if (mySpawner != null)
        {
            mySpawner.EnemyDied(this.gameObject);
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
}
