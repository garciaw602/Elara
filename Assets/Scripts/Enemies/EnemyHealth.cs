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

    // Este método será llamado por la bala cuando impacte
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; // Reduce la vida actual
        Debug.Log(gameObject.name + " ha recibido " + amount + " de daño. Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto!");

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
