using UnityEngine;

public class EnemyDamagePlayer : MonoBehaviour
{
    public float damageAmount = 10f; // Cantidad de daño que el enemigo inflige
    public float damageInterval = 1f; // Intervalo de tiempo entre daños 

    private float lastDamageTime; // Tiempo del último daño infligido

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null && Time.time >= lastDamageTime + damageInterval)
            {
                playerHealth.TakeDamage(damageAmount);
                lastDamageTime = Time.time; 
            }
        }
    }

}