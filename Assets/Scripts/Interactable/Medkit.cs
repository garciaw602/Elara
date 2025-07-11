using UnityEngine;

public class Medkit : MonoBehaviour
{
    public float healAmount = 25f; // Cantidad de vida que restaura este botiqu�n

    public bool destroyOnUse = true;

    //Duraci�n del sonido o efecto visual despu�s de usarlo (antes de destruir el objeto)
    public float destroyDelay = 0.1f; // Peque�o retraso para efectos/sonidos

    // Efectos visuales o sonidos
    // public GameObject healingEffectPrefab;
    // public AudioClip healingSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount); // Llama a la funci�n Heal del jugador

                // Reproducir sonido o efecto visual
                // if (healingSound != null) AudioSource.PlayClipAtPoint(healingSound, transform.position);
                // if (healingEffectPrefab != null) Instantiate(healingEffectPrefab, transform.position, Quaternion.identity);

                // Destruye el botiqu�n
                if (destroyOnUse)
                {
                    GetComponent<Renderer>().enabled = false;
                    GetComponent<Collider>().enabled = false;
                    Destroy(gameObject, destroyDelay);
                }
            }
        }
    }
}