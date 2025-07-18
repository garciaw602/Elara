using UnityEngine;

/// <summary>
/// Bullet: Gestiona el comportamiento de las balas, incluyendo su da�o,
/// tiempo de vida y efectos de impacto.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Cantidad de da�o que esta bala causar� al impactar un enemigo.")]
    public float damageAmount = 10f;

    [Tooltip("Tiempo m�ximo en segundos que la bala permanecer� activa si no impacta nada.")]
    public float lifeTime = 3f; // Tiempo de vida de la bala si no impacta nada

    [Header("Impact Effects")]
    [Tooltip("Prefab del sistema de part�culas para el efecto de impacto gen�rico (ej. chispas en pared, tierra).")]
    public GameObject impactEffectPrefab; // Prefab de efecto de impacto (ej. chispas, explosi�n)

    [Tooltip("Prefab del sistema de part�culas espec�fico para cuando la bala impacta a un enemigo.")]
    public GameObject enemyImpactEffectPrefab; // Part�cula para impacto en enemigo

    [Tooltip("La duraci�n en segundos que las part�culas de impacto en el enemigo permanecer�n visibles.")]
    public float enemyImpactParticlesDuration = 1.5f; // Duraci�n de las part�culas de impacto en enemigo


    // --- NUEVO ---
    [Tooltip("La duraci�n en segundos que las part�culas de impacto gen�rico permanecer�n visibles.")]
    public float genericImpactParticlesDuration = 1.0f; // Duraci�n para las part�culas de impacto gen�rico


    /// <summary>
    /// Start se llama antes de la primera actualizaci�n del frame.
    /// Programa la autodestrucci�n de la bala si no impacta nada.
    /// </summary>
    void Start()
    {
        // Destruye la bala despu�s de un tiempo si no ha impactado nada
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// OnCollisionEnter se llama cuando este collider/rigidbody ha comenzado a tocar otro
    /// collider/rigidbody.
    /// </summary>
    /// <param name="collision">La informaci�n de la colisi�n.</param>
    void OnCollisionEnter(Collision collision)
    {
        bool hitEnemy = false; // Bandera para saber si impactamos a un enemigo

        // Intenta obtener el componente EnemyHealth del objeto colisionado
        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

        // Si el objeto tiene un componente EnemyHealth, significa que es un enemigo
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damageAmount); // Causa da�o al enemigo
            hitEnemy = true; // Marcamos que impactamos a un enemigo
        }

        // Instancia un efecto de impacto:
        // Si impactamos a un enemigo y tenemos un efecto espec�fico para ello, lo usamos.
        if (hitEnemy && enemyImpactEffectPrefab != null)
        {
            GameObject enemyFX = Instantiate(enemyImpactEffectPrefab, transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
            // ---  Destruir las part�culas despu�s de su duraci�n ---
            Destroy(enemyFX, enemyImpactParticlesDuration);
        }
        // De lo contrario, si no es un enemigo o no tenemos un efecto espec�fico para enemigo, usamos el efecto gen�rico.
        else if (impactEffectPrefab != null)
        {
            GameObject genericFX = Instantiate(impactEffectPrefab, transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
            // --- Destruir las part�culas despu�s de su duraci�n ---
            Destroy(genericFX, genericImpactParticlesDuration);
        }

        // Destruye la bala despu�s de impactar
        Destroy(gameObject);
    }
}