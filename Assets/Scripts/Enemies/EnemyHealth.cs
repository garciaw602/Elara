using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; 
    private float currentHealth;   

    
    public float destroyDelay = 0f; //  Poner un retardo o no (0f = inmediato)

    void Start()
    {
        currentHealth = maxHealth; 
    }

    // Este m�todo ser� llamado por la bala cuando impacte
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; // Reduce la vida actual
        Debug.Log(gameObject.name + " ha recibido " + amount + " de da�o. Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto!");

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
