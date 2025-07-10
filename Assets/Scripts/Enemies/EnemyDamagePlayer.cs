using UnityEngine;

public class EnemyDamagePlayer : MonoBehaviour
{
    public float damageAmount = 10f; // Cantidad de da�o que el enemigo inflige
    public float damageInterval = 1f; // Intervalo de tiempo entre da�os 

    private float lastDamageTime; // Tiempo del �ltimo da�o infligido

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