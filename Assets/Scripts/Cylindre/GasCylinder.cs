using UnityEngine;

public class GasCylinder : MonoBehaviour
{
    // ===============================================
    // Variables Configurables en el Inspector de Unity
    // ===============================================

    [Header("Trigger Settings")]
    [Tooltip("La TAG de los GameObjects que se consideran 'balas' " +
             "y que activar�n la explosi�n al impactar. Escribe la Tag de tu bala en el Inspector.")]
    public string bulletTag = "Bullet";

    [Tooltip("El tiempo en segundos que transcurre desde el impacto inicial de la bala " +
             "hasta que la explosi�n principal (sonido de explosi�n, part�culas y fuerza) sucede.")]
    public float explosionDelay = 0.5f;

    [Header("Audio Settings")]
    [Tooltip("El AudioSource que reproducir� los sonidos de este cilindro.")]
    public AudioSource cylinderAudioSource;

    [Tooltip("El AudioClip que se reproduce inmediatamente cuando el cilindro es impactado.")]
    public AudioClip impactSound;

    [Tooltip("El AudioClip de la explosi�n principal del cilindro.")]
    public AudioClip explosionSound;

    private bool isExploded = false;

    [Header("Particle Effects")]
    [Tooltip("Prefab del sistema de part�culas para el efecto visual de la explosi�n inicial (chispas, llamas).")]
    public GameObject explosionParticlesPrefab;

    [Tooltip("Prefab del sistema de part�culas para el efecto de humo o llamas residuales.")]
    public GameObject smokeParticlesPrefab;

    [Tooltip("La duraci�n en segundos que las part�culas instanciadas permanecer�n visibles.")]
    public float particlesDuration = 3f;

    [Header("Explosion Physics")]
    [Tooltip("La magnitud de la fuerza aplicada a los Rigidbodies dentro del radio de la explosi�n.")]
    public float explosionForce = 500f;

    [Tooltip("El radio esf�rico dentro del cual la explosi�n aplicar� fuerza a otros objetos.")]
    public float explosionRadius = 5f;

    [Tooltip("Un factor que modifica la fuerza ascendente de la explosi�n.")]
    public float explosionUpwardModifier = 0.5f;

    [Header("Explosion Target Layers")]
    [Tooltip("Las capas de los GameObjects que se consideran 'enemigos' y ser�n afectados.")]
    public LayerMask enemyLayer;

    [Tooltip("Las capas de los GameObjects que se consideran 'interactuables' y ser�n afectados.")]
    public LayerMask interactableLayer;

    private LayerMask combinedExplosionLayers;

    // Referencias al Renderer y Collider del hijo para mayor eficiencia
    private Renderer childRenderer;
    private Collider childCollider;

    void Start()
    {
        if (cylinderAudioSource == null)
        {
            Debug.LogError("GasCylinder: No se asign� 'Cylinder Audio Source' en el Inspector. Deshabilitando script.", this);
            enabled = false;
            return;
        }

        cylinderAudioSource.playOnAwake = false;
        cylinderAudioSource.loop = false;
        combinedExplosionLayers = enemyLayer | interactableLayer;

        // �NUEVO! Obtener el Renderer y Collider de los GameObjects hijos
        childRenderer = GetComponentInChildren<Renderer>();
        childCollider = GetComponentInChildren<Collider>();

        if (childRenderer == null)
        {
            Debug.LogWarning("GasCylinder: No se encontr� un Renderer en el cilindro o sus hijos. El cilindro no desaparecer� visualmente.", this);
        }
        if (childCollider == null)
        {
            Debug.LogWarning("GasCylinder: No se encontr� un Collider en el cilindro o sus hijos. El cilindro no dejar� de colisionar.", this);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("GasCylinder: OnCollisionEnter activado con: " + collision.gameObject.name, this);
        Debug.Log($"GasCylinder: Objeto colisionado tiene la Tag '{collision.gameObject.tag}' (esperado: '{bulletTag}')", this);

        if (collision.gameObject.CompareTag(bulletTag) && !isExploded)
        {
            isExploded = true;
            Debug.Log("GasCylinder: Colisi�n v�lida detectada con una bala (por Tag). Iniciando secuencia de explosi�n.");

            // 1. Reproducir sonido de impacto de metal INMEDIATAMENTE.
            if (cylinderAudioSource != null && impactSound != null)
            {
                cylinderAudioSource.PlayOneShot(impactSound);
                Debug.Log("GasCylinder: Sonido de impacto de metal reproducido.");
            }
            else
            {
                Debug.LogWarning("GasCylinder: No se puede reproducir sonido de impacto. AudioSource o AudioClip 'Impact Sound' no asignado.", this);
            }

            // 2. Desactivar el Renderizado (visual) y el Collider del cilindro INMEDIATAMENTE.
            // �AHORA USAMOS LAS REFERENCIAS AL HIJO!
            if (childRenderer != null) childRenderer.enabled = false;
            if (childCollider != null) childCollider.enabled = false;
            Debug.Log("GasCylinder: Renderizado y Collider del cilindro desactivados (desaparece visualmente).");


            // 3. Programar la explosi�n principal despu�s del retraso.
            Debug.Log("GasCylinder: Programando MainExplosion en " + explosionDelay + " segundos.");
            Invoke("MainExplosion", explosionDelay);

        }
        else
        {
            Debug.Log("GasCylinder: Colisi�n no cumple los requisitos (no es bala o ya explot�).");
        }
    }

    void MainExplosion()
    {
        Debug.Log("GasCylinder: �MainExplosion se est� ejecutando AHORA! (despu�s del retraso)");

        // NOTA: El renderizado y el collider ya se desactivaron en OnCollisionEnter().

        // 1. Reproducir sonido de explosi�n principal.
        if (cylinderAudioSource != null && explosionSound != null)
        {
            cylinderAudioSource.PlayOneShot(explosionSound);
            Debug.Log("GasCylinder: Sonido de explosi�n principal reproducido.");
        }
        else
        {
            Debug.LogWarning("GasCylinder: No se puede reproducir sonido de explosi�n. AudioSource o AudioClip 'Explosion Sound' no asignado.", this);
        }

        // 2. Instanciar part�culas de explosi�n (en paralelo con el sonido de explosi�n).
        if (explosionParticlesPrefab != null)
        {
            GameObject explosionFX = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(explosionFX, particlesDuration);
            Debug.Log("GasCylinder: Part�culas de explosi�n instanciadas.");
        }
        else { Debug.LogWarning("GasCylinder: No se asign� 'explosionParticlesPrefab'. La explosi�n visual puede no aparecer.", this); }

        // 3. Instanciar part�culas de humo (en paralelo con lo anterior).
        if (smokeParticlesPrefab != null)
        {
            GameObject smokeFX = Instantiate(smokeParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(smokeFX, particlesDuration);
            Debug.Log("GasCylinder: Part�culas de humo instanciadas.");
        }
        else { Debug.LogWarning("GasCylinder: No se asign� 'smokeParticlesPrefab'. El efecto de humo puede faltar.", this); }

        // 4. Aplicar fuerza de explosi�n a los objetos cercanos (tambi�n en paralelo).
        Collider[] collidersToAffect = Physics.OverlapSphere(transform.position, explosionRadius, combinedExplosionLayers);
        Debug.Log($"GasCylinder: {collidersToAffect.Length} colliders encontrados para afectar en la explosi�n.", this);

        foreach (Collider hitCollider in collidersToAffect)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null && rb.gameObject != gameObject)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);
                Debug.Log($"GasCylinder: Fuerza de explosi�n aplicada a {hitCollider.gameObject.name}", hitCollider.gameObject);
            }
        }

        // 5. Destruir el GameObject del cilindro DESPU�S de que el sonido de explosi�n haya tenido tiempo de reproducirse.
        float finalDestroyDelay = (cylinderAudioSource != null && explosionSound != null) ? explosionSound.length : 0.5f;

        Debug.Log($"GasCylinder: Destruyendo cilindro en {finalDestroyDelay} segundos (basado en duraci�n del sonido de explosi�n).", this);
        Destroy(gameObject, finalDestroyDelay);
    }

    // ===============================================
    // Herramientas de Depuraci�n (Gizmos)
    // ===============================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}