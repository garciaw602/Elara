using UnityEngine;
using System.Collections; // Necesario para Coroutines
using System.Collections.Generic; // Necesario para List<T>

/// <summary>
/// LampController: Este script gestiona el comportamiento de una lámpara, incluyendo
/// su efecto de parpadeo, sonidos asociados y destrucción al impactar.
/// Proporciona parámetros personalizables para la luz, el audio y los efectos de partículas.
/// </summary>
public class LampController : MonoBehaviour
{
    // ===============================================
    // Variables Configurables (Visibles en el Inspector de Unity)
    // ===============================================

    [Header("Luz y Parpadeo")]
    [Tooltip("El componente Light de la lámpara. Arrastra tu componente Light aquí.")]
    public Light lampLight; // La luz real que se encenderá/apagará

    [Tooltip("Tiempo mínimo que la luz permanecerá encendida/apagada en un ciclo de parpadeo aleatorio.")]
    public float minFlickerDuration = 0.1f;

    [Tooltip("Tiempo máximo que la luz permanecerá encendida/apagada en un ciclo de parpadeo aleatorio.")]
    public float maxFlickerDuration = 0.5f;

    [Tooltip("Prefab del sistema de partículas para el efecto de cortocircuito/parpadeo.")]
    public GameObject shortCircuitParticlesPrefab; // Partícula para el parpadeo

    [Header("Trigger Settings")]
    [Tooltip("La TAG de los GameObjects que se consideran 'balas' y que activarán la destrucción de la lámpara.")]
    public string bulletTag = "Bullet";

    [Header("Audio Settings")]
    [Tooltip("El AudioSource que reproducirá los sonidos de la lámpara.")]
    public AudioSource lampAudioSource;

    [Tooltip("El AudioClip que se reproduce cuando la bala impacta la lámpara.")]
    public AudioClip impactSound; // Sonido de rotura o impacto

    [Tooltip("El AudioClip que se reproduce mientras la lámpara está parpadeando (sonido de electricidad/cortocircuito).")]
    public AudioClip electricitySound; // Sonido para el parpadeo

    [Header("Particle Effects")]
    [Tooltip("Prefab del sistema de partículas para la explosión de la lámpara (chispas, rotura de cristal).")]
    public GameObject explosionParticlesPrefab;

    [Tooltip("La duración en segundos que las partículas instanciadas (explosión) permanecerán visibles.")]
    public float particlesDuration = 2f;

    // ===============================================
    // Variables Internas (No expuestas en el Inspector)
    // ===============================================
    private bool isBroken = false; // Bandera para rastrear si la lámpara ya está rota.
    // Usaremos listas para manejar múltiples renderers y colliders en hijos.
    private List<Renderer> lampRenderers; // <-- ¡AHORA ES UNA LISTA!
    private List<Collider> lampColliders; // <-- ¡AHORA ES UNA LISTA!
    private Coroutine flickerCoroutine; // Para controlar la corrutina de parpadeo
    private GameObject currentShortCircuitParticles; // Referencia a las partículas de cortocircuito activas

    // ===============================================
    // Métodos de Ciclo de Vida de Unity
    // ===============================================

    /// <summary>
    /// Start se llama antes de la primera actualización del frame.
    /// Inicializa los componentes y comienza el efecto de parpadeo.
    /// </summary>
    void Start()
    {
        // Obtener referencias a los componentes necesarios. Priorizar GetComponentInChildren si la asignación directa falla.
        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
            if (lampLight == null)
            {
                Debug.LogError("LampController: No se ha asignado un componente Light o no se encontró en los hijos. Deshabilitando script.", this);
                enabled = false;
                return;
            }
        }

        if (lampAudioSource == null)
        {
            lampAudioSource = GetComponent<AudioSource>();
            if (lampAudioSource == null)
            {
                Debug.LogError("LampController: No se ha asignado un AudioSource o no se encontró en este GameObject. Deshabilitando script.", this);
                enabled = false;
                return;
            }
        }
        lampAudioSource.playOnAwake = false; // Asegurarse de que no suene al inicio.

        // Obtener TODOS los Renderers y Colliders en el objeto padre y sus hijos.
        lampRenderers = new List<Renderer>(GetComponentsInChildren<Renderer>()); // <-- ¡CAMBIO AQUÍ!
        lampColliders = new List<Collider>(GetComponentsInChildren<Collider>()); // <-- ¡CAMBIO AQUÍ!

        if (lampRenderers.Count == 0) Debug.LogWarning("LampController: No se encontró ningún Renderer en la lámpara o sus hijos. No se podrá ocultar visualmente.", this);
        if (lampColliders.Count == 0) Debug.LogWarning("LampController: No se encontró ningún Collider en la lámpara o sus hijos. No se podrá desactivar la colisión.", this);

        // Iniciar la corrutina de parpadeo si la lámpara no está rota al inicio.
        if (!isBroken)
        {
            flickerCoroutine = StartCoroutine(FlickerLight());
        }
    }

    /// <summary>
    /// OnCollisionEnter se llama cuando este collider/rigidbody ha comenzado a tocar otro rigidbody/collider.
    /// Maneja el impacto de la bala, la destrucción de la lámpara y los efectos asociados.
    /// </summary>
    /// <param name="collision">Los datos de colisión asociados con este evento.</param>
    void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto que colisionó tiene la bulletTag y la lámpara no está rota.
        if (collision.gameObject.CompareTag(bulletTag) && !isBroken)
        {
            isBroken = true; // Marcar la lámpara como rota para evitar múltiples activaciones.

            // Detener la corrutina de parpadeo si está en ejecución.
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
            }

            // Detener el sonido de electricidad si está sonando.
            if (lampAudioSource != null && lampAudioSource.isPlaying && lampAudioSource.clip == electricitySound)
            {
                lampAudioSource.Stop();
                Debug.Log("LampController: Deteniendo sonido de electricidad.", this);
            }

            // Destruir cualquier partícula de cortocircuito activa.
            if (currentShortCircuitParticles != null)
            {
                Destroy(currentShortCircuitParticles);
            }

            // Asegurarse de que la luz se apague al romperse.
            if (lampLight != null) lampLight.enabled = false;

            // 1. Reproducir sonido de impacto inmediatamente.
            if (lampAudioSource != null && impactSound != null)
            {
                lampAudioSource.PlayOneShot(impactSound);
            }
            else
            {
                Debug.LogWarning("LampController: Sonido de impacto o AudioSource no asignado.", this);
            }

            // 2. Instanciar partículas de explosión/rotura.
            if (explosionParticlesPrefab != null)
            {
                GameObject explosionFX = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(explosionFX, particlesDuration); // Las partículas se autodestruyen después de 'particlesDuration'.
            }
            else { Debug.LogWarning("LampController: Prefab de partículas de explosión no asignado. La rotura visual podría no aparecer.", this); }

            // 3. Desactivar TODOS los Renderers (visual) y Colliders de la lámpara inmediatamente.
            foreach (Renderer r in lampRenderers) // <-- ¡CAMBIO AQUÍ!
            {
                if (r != null) r.enabled = false;
            }
            foreach (Collider c in lampColliders) // <-- ¡CAMBIO AQUÍ!
            {
                if (c != null) c.enabled = false;
            }

            // 4. Eliminar el objeto de la lámpara después de que el sonido y las partículas hayan terminado.
            // Usar la duración del sonido de impacto para el retraso, o un valor por defecto si no hay sonido.
            float destroyDelay = (impactSound != null) ? impactSound.length : 0.5f;
            Destroy(gameObject, destroyDelay);
        }
    }

    // ===============================================
    // Lógica del Parpadeo (Corrutinas)
    // ===============================================

    /// <summary>
    /// Corrutina para hacer que la luz de la lámpara parpadee con patrones aleatorios,
    /// y gestionar el efecto de partículas de cortocircuito y su sonido.
    /// </summary>
    IEnumerator FlickerLight()
    {
        while (!isBroken) // Bucle infinito hasta que la lámpara se rompa.
        {
            // Cambiar estado de la luz.
            if (lampLight != null)
            {
                lampLight.enabled = !lampLight.enabled;
            }

            // Gestionar el efecto de partículas de cortocircuito y su sonido basado en el estado de la luz.
            if (shortCircuitParticlesPrefab != null || electricitySound != null)
            {
                if (lampLight != null && lampLight.enabled) // Si la luz está ENCENDIDA (o intentando encenderse)
                {
                    // Asegurar que las partículas estén activas o instanciadas.
                    if (shortCircuitParticlesPrefab != null)
                    {
                        if (currentShortCircuitParticles == null)
                        {
                            currentShortCircuitParticles = Instantiate(shortCircuitParticlesPrefab, transform.position, Quaternion.identity, transform);
                            // Padre al GameObject de la lámpara para facilitar la gestión.
                        }
                        currentShortCircuitParticles.SetActive(true);
                    }

                    // Reproducir sonido de electricidad si no está sonando ya.
                    if (electricitySound != null && lampAudioSource != null && !lampAudioSource.isPlaying)
                    {
                        lampAudioSource.clip = electricitySound;
                        lampAudioSource.loop = true; // Asegurar que el sonido de electricidad se repita.
                        lampAudioSource.Play();
                        Debug.Log("LampController: Iniciando sonido de electricidad.", this);
                    }
                }
                else // Si la luz está APAGADA
                {
                    if (shortCircuitParticlesPrefab != null && currentShortCircuitParticles != null)
                    {
                        currentShortCircuitParticles.SetActive(false); // Ocultar partículas cuando la luz está apagada.
                    }

                    // Detener el sonido de electricidad si la luz está apagada y el sonido está activo.
                    if (electricitySound != null && lampAudioSource != null && lampAudioSource.isPlaying && lampAudioSource.clip == electricitySound)
                    {
                        lampAudioSource.Stop();
                        Debug.Log("LampController: Deteniendo sonido de electricidad durante parpadeo (luz apagada).", this);
                    }
                }
            }

            // Esperar un tiempo aleatorio antes del siguiente cambio de estado.
            float waitTime = Random.Range(minFlickerDuration, maxFlickerDuration);
            yield return new WaitForSeconds(waitTime);
        }
    }
}