using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; 
    private float currentHealth;   

    public Slider healthBarSlider; // Referencia al Slider de la barra de vida UI

    void Start()
    {
        currentHealth = maxHealth; // Inicializa la vida al máximo
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }

    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount; 
        Debug.Log("Player ha recibido " + amount + " de daño. Vida actual: " + currentHealth);

        
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount; 
        
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }
        Debug.Log("Player se ha curado " + amount + ". Vida actual: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("Player ha muerto!");
        // - Reiniciar la escena
        // - Mostrar una pantalla de "Game Over"
        // - Desactivar el control del jugador
        gameObject.SetActive(false); // temporal
    }
}