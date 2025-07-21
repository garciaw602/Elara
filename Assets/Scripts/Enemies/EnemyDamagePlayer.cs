using UnityEngine;

public class EnemyDamagePlayer : MonoBehaviour
{
    [Tooltip("Cantidad de daño que el enemigo inflige por golpe.")]
    public float damageAmount = 10f; 

    [Tooltip("Intervalo de tiempo en segundos entre daños consecutivos.")]
    public float damageInterval = 1f;  

    private float lastDamageTime; // Tiempo del último daño infligido

    void OnCollisionEnter(Collision collision) // Se llama la primera vez que la colisión ocurre
    {
        HandlePlayerCollision(collision.gameObject);
    }

    void OnCollisionStay(Collision collision) // Se llama mientras la colisión continúa
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void HandlePlayerCollision(GameObject collidedObject)
    {
        // Verifica si el objeto colisionado es el jugador (usando su Tag "Player")
        if (collidedObject.CompareTag("Player"))
        {
            // Intenta obtener el componente PlayerHealth del jugador
            PlayerHealth playerHealth = collidedObject.GetComponent<PlayerHealth>();

            // Si el jugador tiene PlayerHealth y ha pasado suficiente tiempo desde el último ataque
            if (playerHealth != null && Time.time >= lastDamageTime + damageInterval)
            {
                playerHealth.TakeDamage(damageAmount); // Causa daño al jugador

                // Llama al método para mostrar la UI de ataque 
                playerHealth.ShowAttackIndicator();

                lastDamageTime = Time.time; // Actualiza el tiempo del último ataque
            }
        }
    }
}