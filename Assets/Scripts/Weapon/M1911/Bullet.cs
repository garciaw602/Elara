using UnityEngine;

/// <summary>
/// Bullet: Gestiona el comportamiento de las balas, incluyendo su daño,
/// tiempo de vida y efectos de impacto.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Cantidad de daño que esta bala causará al impactar un enemigo.")]
    public float damageAmount = 10f;

    [Tooltip("Tiempo máximo en segundos que la bala permanecerá activa si no impacta nada.")]
    public float lifeTime = 3f; // Tiempo de vida de la bala si no impacta nada

    [Header("Impact Effects")]
    [Tooltip("Prefab del sistema de partículas para el efecto de impacto genérico (ej. chispas en pared, tierra).")]
    public GameObject impactEffectPrefab; // Prefab de efecto de impacto (ej. chispas, explosión)

    [Tooltip("Prefab del sistema de partículas específico para cuando la bala impacta a un enemigo.")]
    public GameObject enemyImpactEffectPrefab; // <-- ¡NUEVO! Partícula para impacto en enemigo

    [Tooltip("La duración en segundos que las partículas de impacto en el enemigo permanecerán visibles.")]
    public float enemyImpactParticlesDuration = 1.5f; // <-- ¡NUEVO! Duración de las partículas de impacto en enemigo

    /// <summary>
    /// Start se llama antes de la primera actualización del frame.
    /// Programa la autodestrucción de la bala si no impacta nada.
    /// </summary>
    void Start()
    {
        // Destruye la bala después de un tiempo si no ha impactado nada
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// OnCollisionEnter se llama cuando este collider/rigidbody ha comenzado a tocar otro rigidbody/collider.
    /// Maneja el daño y los efectos de impacto.
    /// </summary>
    /// <param name="collision">Los datos de colisión asociados con este evento.</param>
    void OnCollisionEnter(Collision collision)
    {
        bool hitEnemy = false; // Bandera para saber si impactamos a un enemigo

        // Intenta obtener el componente EnemyHealth del objeto colisionado
        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

        // Si el objeto tiene un componente EnemyHealth, significa que es un enemigo
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damageAmount); // Causa daño al enemigo
            hitEnemy = true; // Marcamos que impactamos a un enemigo
        }

        // Instancia un efecto de impacto:
        // Si impactamos a un enemigo y tenemos un efecto específico para ello, lo usamos.
        if (hitEnemy && enemyImpactEffectPrefab != null)
        {
            GameObject enemyFX = Instantiate(enemyImpactEffectPrefab, transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
            Destroy(enemyFX, enemyImpactParticlesDuration); // Destruye las partículas del enemigo después de su duración
        }
        // De lo contrario, si no es un enemigo o no tenemos un efecto específico para enemigo, usamos el efecto genérico.
        else if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
        }

        // Destruye la bala después de impactar
        Destroy(gameObject);
    }
}