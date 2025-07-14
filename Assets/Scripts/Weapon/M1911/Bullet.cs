using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damageAmount = 10f; 
    public float lifeTime = 3f; // Tiempo de vida de la bala si no impacta nada
    public GameObject impactEffectPrefab; // Prefab de efecto de impacto (ej. chispas, explosión)

    void Start()
    {
        // Destruye la bala después de un tiempo si no ha impactado nada
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Intenta obtener el componente EnemyHealth del objeto colisionado
        EnemyHealth enemyHealth2 = collision.gameObject.GetComponent<EnemyHealth>();//******** (cambiar)

        // Si el objeto tiene un componente EnemyHealth, significa que es un enemigo
        if (enemyHealth2 != null)
        {
            enemyHealth2.TakeDamage(damageAmount); // Causa daño al enemigo
        }
       

        // Instancia un efecto de impacto 
        if (impactEffectPrefab != null)
        {
           Instantiate(impactEffectPrefab, transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
        }
        
        // Destruye la bala después de impactar
        Destroy(gameObject);
    }
}