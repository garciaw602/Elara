using UnityEngine;

/// <summary>
/// GasCylinder: Este script controla el comportamiento de un cilindro de gas explosivo.
/// Reacciona al impacto de objetos en una capa específica ("BulletLayer"), reproduce sonidos,
/// genera efectos de partículas y aplica una fuerza de explosión a los objetos en capas específicas.
/// </summary>
public class GasCylinder : MonoBehaviour
{
    // ===============================================
    // Variables Configurables en el Inspector de Unity
    // ===============================================

    [Header("Trigger Settings")] // Configuración de cómo se activa la explosión.
    [Tooltip("La TAG de los GameObjects que se consideran 'balas' " +
             "y que activarán la explosión al impactar. Escribe la Tag de tu bala en el Inspector.")]
    public string bulletTag = "Bullet";

    [Tooltip("El tiempo en segundos que transcurre desde el impacto inicial de la bala " +
             "hasta que la explosión principal (sonido de explosión, partículas y fuerza) sucede.")]
    public float explosionDelay = 0.5f;

    [Header("Audio Settings")] // Configuración de audio.
    [Tooltip("El AudioSource que reproducirá los sonidos de este cilindro.")]
    public AudioSource cylinderAudioSource;

    [Tooltip("El AudioClip que se reproduce inmediatamente cuando el cilindro es impactado.")]
    public AudioClip impactSound;

    [Tooltip("El AudioClip de la explosión principal del cilindro.")]
    public AudioClip explosionSound;

    private bool isExploded = false; // Bandera para evitar múltiples explosiones.

    [Header("Particle Effects")] // Configuración de efectos de partículas.
    [Tooltip("Prefab del sistema de partículas para el efecto visual de la explosión inicial (chispas, llamas).")]
    public GameObject explosionParticlesPrefab;

    [Tooltip("Prefab del sistema de partículas para el efecto de humo o llamas residuales.")]
    public GameObject smokeParticlesPrefab;

    [Tooltip("La duración en segundos que las partículas instanciadas permanecerán visibles.")]
    public float particlesDuration = 3f;

    [Header("Explosion Physics")] // Configuración de la física de la explosión.
    [Tooltip("La magnitud de la fuerza aplicada a los Rigidbodies dentro del radio de la explosión.")]
    public float explosionForce = 500f;

    [Tooltip("El radio esférico dentro del cual la explosión aplicará fuerza a otros objetos.")]
    public float explosionRadius = 5f;

    [Tooltip("Un factor que modifica la fuerza ascendente de la explosión.")]
    public float explosionUpwardModifier = 0.5f;

    [Header("Explosion Target Layers")] // Nuevas configuraciones para las capas objetivo de la explosión.
    [Tooltip("Las capas de los GameObjects que se consideran 'enemigos' y serán afectados.")]
    public LayerMask enemyLayer;

    [Tooltip("Las capas de los GameObjects que se consideran 'interactuables' y serán afectados.")]
    public LayerMask interactableLayer;

    private LayerMask combinedExplosionLayers; // Una LayerMask combinada para usar en Physics.OverlapSphere para mayor eficiencia.

    // Referencias al Renderer y Collider del hijo para mayor eficiencia
    private Renderer childRenderer;
    private Collider childCollider;

    // Referencia al GasLeakSoundController en este mismo GameObject.
    private GasLeakSoundController gasLeakSoundController;

    // ===============================================
    // Métodos de Ciclo de Vida de Unity
    // ===============================================

    void Start()
    {
        // Verificar que el AudioSource esté asignado.
        if (cylinderAudioSource == null)
        {
            Debug.LogError("GasCylinder: No se asignó 'Cylinder Audio Source' en el Inspector. Deshabilitando script.", this);
            enabled = false;
            return;
        }

        cylinderAudioSource.playOnAwake = false;
        cylinderAudioSource.loop = false;

        // Combinamos las capas de enemigo e interactuable en una sola LayerMask.
        combinedExplosionLayers = enemyLayer | interactableLayer;

        // Obtener el Renderer y Collider de los GameObjects hijos
        childRenderer = GetComponentInChildren<Renderer>();
        childCollider = GetComponentInChildren<Collider>();

        if (childRenderer == null)
        {
            Debug.LogWarning("GasCylinder: No se encontró un Renderer en el cilindro o sus hijos. El cilindro no desaparecerá visualmente.", this);
        }
        if (childCollider == null)
        {
            Debug.LogWarning("GasCylinder: No se encontró un Collider en el cilindro o sus hijos. El cilindro no dejará de colisionar.", this);
        }

        // Obtener la referencia al GasLeakSoundController si existe en este GameObject
        gasLeakSoundController = GetComponent<GasLeakSoundController>();
        if (gasLeakSoundController == null)
        {
            Debug.LogWarning("GasCylinder: No se encontró GasLeakSoundController en este GameObject. El sonido de fuga de gas podría no detenerse correctamente.", this);
        }
    }

    /// <summary>
    /// OnCollisionEnter() es llamado cuando un Collider entra en contacto físico.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // DEBUG: Confirmar que OnCollisionEnter se activa.
        Debug.Log("GasCylinder: OnCollisionEnter activado con: " + collision.gameObject.name, this);
        Debug.Log($"GasCylinder: Objeto colisionado tiene la Tag '{collision.gameObject.tag}' (esperado: '{bulletTag}')", this);

        // Verificamos si el objeto que colisionó tiene la TAG de la bala y si el cilindro aún no ha explotado.
        if (collision.gameObject.CompareTag(bulletTag) && !isExploded)
        {
            isExploded = true; // Marcamos el cilindro como explotado para evitar múltiples activaciones.
            Debug.Log("GasCylinder: Colisión válida detectada con una bala (por Tag). Iniciando secuencia de explosión.");

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
            // Esto hará que el cilindro desaparezca visualmente al instante del impacto, usando las referencias del hijo.
            if (childRenderer != null) childRenderer.enabled = false;
            if (childCollider != null) childCollider.enabled = false;
            Debug.Log("GasCylinder: Renderizado y Collider del cilindro desactivados (desaparece visualmente).");

            // 3. Programar la explosión principal después del retraso.
            Debug.Log("GasCylinder: Programando MainExplosion en " + explosionDelay + " segundos.");
            Invoke("MainExplosion", explosionDelay);
        }
        else
        {
            // DEBUG: Si OnCollisionEnter se activa pero la condición IF no se cumple.
            Debug.Log("GasCylinder: Colisión no cumple los requisitos (no es bala o ya explotó).");
        }
    }

    // ===============================================
    // Lógica de la Explosión Principal
    // ===============================================

    void MainExplosion()
    {
        // DEBUG: ¡CRÍTICO! Confirmar que MainExplosion se ejecuta.
        Debug.Log("GasCylinder: ¡MainExplosion se está ejecutando AHORA! (después del retraso)");

        // El renderizado y el collider ya se desactivaron en OnCollisionEnter().

        // 1. Reproducir sonido de explosión principal.
        if (cylinderAudioSource != null && explosionSound != null)
        {
            cylinderAudioSource.PlayOneShot(explosionSound);
            Debug.Log("GasCylinder: Sonido de explosión principal reproducido.");
        }
        else
        {
            Debug.LogWarning("GasCylinder: No se puede reproducir sonido de explosión. AudioSource o AudioClip 'Explosion Sound' no asignado.", this);
        }

        // 2. Instanciar partículas de explosión (en paralelo con el sonido de explosión).
        if (explosionParticlesPrefab != null)
        {
            GameObject explosionFX = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(explosionFX, particlesDuration);
            Debug.Log("GasCylinder: Partículas de explosión instanciadas.");
        }
        else { Debug.LogWarning("GasCylinder: No se asignó 'explosionParticlesPrefab'. La explosión visual puede no aparecer.", this); }

        // 3. Instanciar partículas de humo (en paralelo con lo anterior).
        if (smokeParticlesPrefab != null)
        {
            GameObject smokeFX = Instantiate(smokeParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(smokeFX, particlesDuration);
            Debug.Log("GasCylinder: Partículas de humo instanciadas.");
        }
        else { Debug.LogWarning("GasCylinder: No se asignó 'smokeParticlesPrefab'. El efecto de humo puede faltar.", this); }

        // 4. Aplicar fuerza de explosión a los objetos cercanos (también en paralelo).
        // Usamos el 'combinedExplosionLayers' para filtrar los colliders directamente en OverlapSphere.
        Collider[] collidersToAffect = Physics.OverlapSphere(transform.position, explosionRadius, combinedExplosionLayers);
        Debug.Log($"GasCylinder: {collidersToAffect.Length} colliders encontrados para afectar en la explosión.", this);

        foreach (Collider hitCollider in collidersToAffect)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null && rb.gameObject != gameObject) // Evitar afectarse a sí mismo.
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);
                Debug.Log($"GasCylinder: Fuerza de explosión aplicada a {hitCollider.gameObject.name}", hitCollider.gameObject);
            }
        }

        // ¡NUEVO! Detener el sonido de fuga de gas si el GasLeakSoundController está presente.
        if (gasLeakSoundController != null)
        {
            gasLeakSoundController.StopGasLeakSound();
        }

        // 5. Destruir el GameObject del cilindro DESPUÉS de que el sonido de explosión haya tenido tiempo de reproducirse.
        float finalDestroyDelay = (cylinderAudioSource != null && explosionSound != null) ? explosionSound.length : 0.5f;

        Debug.Log($"GasCylinder: Destruyendo cilindro en {finalDestroyDelay} segundos (basado en duración del sonido de explosión).", this);
        Destroy(gameObject, finalDestroyDelay);
    }

    // ===============================================
    // Herramientas de Depuración (Gizmos)
    // ===============================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}