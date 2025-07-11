using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; 
    private float currentHealth;   

    public Slider healthBarSlider;
    public GameObject gameOverPanel;

    void Start()
    {
        currentHealth = maxHealth; // Inicializa la vida al máximo
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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
        // gameObject.SetActive(false); // temporal

        MonoBehaviour[] playerScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in playerScripts)
        {
            if (script != this) // No desactives este mismo script PlayerHealth
            {
                script.enabled = false;
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f; // Pausa el tiempo del juego

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reanuda el tiempo antes de cargar la escena
        // Oculta el cursor y lo bloquea
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarga la escena actual
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reanuda el tiempo
        Cursor.visible = true;
        
        SceneManager.LoadScene("MainMenu"); // Carga el menú principal
    }

}