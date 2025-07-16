using UnityEngine;
using System.Collections; // Necesario para Coroutines
using System.Collections.Generic; // Necesario para List<T>

/// <summary>
/// LampController: Este script gestiona el comportamiento de una l�mpara, incluyendo
/// su efecto de parpadeo, sonidos asociados y destrucci�n al impactar.
/// Proporciona par�metros personalizables para la luz, el audio y los efectos de part�culas.
/// </summary>
public class LampController : MonoBehaviour
{
    // ===============================================
    // Variables Configurables (Visibles en el Inspector de Unity)
    // ===============================================

    [Header("Luz y Parpadeo")]
    [Tooltip("El componente Light de la l�mpara. Arrastra tu componente Light aqu�.")]
    public Light lampLight; // La luz real que se encender�/apagar�

    [Tooltip("Tiempo m�nimo que la luz permanecer� encendida/apagada en un ciclo de parpadeo aleatorio.")]
    public float minFlickerDuration = 0.1f;

    [Tooltip("Tiempo m�ximo que la luz permanecer� encendida/apagada en un ciclo de parpadeo aleatorio.")]
    public float maxFlickerDuration = 0.5f;

    [Tooltip("Prefab del sistema de part�culas para el efecto de cortocircuito/parpadeo.")]
    public GameObject shortCircuitParticlesPrefab; // Part�cula para el parpadeo

    [Header("Trigger Settings")]
    [Tooltip("La TAG de los GameObjects que se consideran 'balas' y que activar�n la destrucci�n de la l�mpara.")]
    public string bulletTag = "Bullet";

    [Header("Audio Settings")]
    [Tooltip("El AudioSource que reproducir� los sonidos de la l�mpara.")]
    public AudioSource lampAudioSource;

    [Tooltip("El AudioClip que se reproduce cuando la bala impacta la l�mpara.")]
    public AudioClip impactSound; // Sonido de rotura o impacto

    [Tooltip("El AudioClip que se reproduce mientras la l�mpara est� parpadeando (sonido de electricidad/cortocircuito).")]
    public AudioClip electricitySound; // Sonido para el parpadeo

    [Header("Particle Effects")]
    [Tooltip("Prefab del sistema de part�culas para la explosi�n de la l�mpara (chispas, rotura de cristal).")]
    public GameObject explosionParticlesPrefab;

    [Tooltip("La duraci�n en segundos que las part�culas instanciadas (explosi�n) permanecer�n visibles.")]
    public float particlesDuration = 2f;

    // ===============================================
    // Variables Internas (No expuestas en el Inspector)
    // ===============================================
    private bool isBroken = false; // Bandera para rastrear si la l�mpara ya est� rota.
    // Usaremos listas para manejar m�ltiples renderers y colliders en hijos.
    private List<Renderer> lampRenderers; // <-- �AHORA ES UNA LISTA!
    private List<Collider> lampColliders; // <-- �AHORA ES UNA LISTA!
    private Coroutine flickerCoroutine; // Para controlar la corrutina de parpadeo
    private GameObject currentShortCircuitParticles; // Referencia a las part�culas de cortocircuito activas

    // ===============================================
    // M�todos de Ciclo de Vida de Unity
    // ===============================================

    /// <summary>
    /// Start se llama antes de la primera actualizaci�n del frame.
    /// Inicializa los componentes y comienza el efecto de parpadeo.
    /// </summary>
    void Start()
    {
        // Obtener referencias a los componentes necesarios. Priorizar GetComponentInChildren si la asignaci�n directa falla.
        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
            if (lampLight == null)
            {
                Debug.LogError("LampController: No se ha asignado un componente Light o no se encontr� en los hijos. Deshabilitando script.", this);
                enabled = false;
                return;
            }
        }

        if (lampAudioSource == null)
        {
            lampAudioSource = GetComponent<AudioSource>();
            if (lampAudioSource == null)
            {
                Debug.LogError("LampController: No se ha asignado un AudioSource o no se encontr� en este GameObject. Deshabilitando script.", this);
                enabled = false;
                return;
            }
        }
        lampAudioSource.playOnAwake = false; // Asegurarse de que no suene al inicio.

        // Obtener TODOS los Renderers y Colliders en el objeto padre y sus hijos.
        lampRenderers = new List<Renderer>(GetComponentsInChildren<Renderer>()); // <-- �CAMBIO AQU�!
        lampColliders = new List<Collider>(GetComponentsInChildren<Collider>()); // <-- �CAMBIO AQU�!

        if (lampRenderers.Count == 0) Debug.LogWarning("LampController: No se encontr� ning�n Renderer en la l�mpara o sus hijos. No se podr� ocultar visualmente.", this);
        if (lampColliders.Count == 0) Debug.LogWarning("LampController: No se encontr� ning�n Collider en la l�mpara o sus hijos. No se podr� desactivar la colisi�n.", this);

        // Iniciar la corrutina de parpadeo si la l�mpara no est� rota al inicio.
        if (!isBroken)
        {
            flickerCoroutine = StartCoroutine(FlickerLight());
        }
    }

    /// <summary>
    /// OnCollisionEnter se llama cuando este collider/rigidbody ha comenzado a tocar otro rigidbody/collider.
    /// Maneja el impacto de la bala, la destrucci�n de la l�mpara y los efectos asociados.
    /// </summary>
    /// <param name="collision">Los datos de colisi�n asociados con este evento.</param>
    void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto que colision� tiene la bulletTag y la l�mpara no est� rota.
        if (collision.gameObject.CompareTag(bulletTag) && !isBroken)
        {
            isBroken = true; // Marcar la l�mpara como rota para evitar m�ltiples activaciones.

            // Detener la corrutina de parpadeo si est� en ejecuci�n.
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
            }

            // Detener el sonido de electricidad si est� sonando.
            if (lampAudioSource != null && lampAudioSource.isPlaying && lampAudioSource.clip == electricitySound)
            {
                lampAudioSource.Stop();
                Debug.Log("LampController: Deteniendo sonido de electricidad.", this);
            }

            // Destruir cualquier part�cula de cortocircuito activa.
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

            // 2. Instanciar part�culas de explosi�n/rotura.
            if (explosionParticlesPrefab != null)
            {
                GameObject explosionFX = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(explosionFX, particlesDuration); // Las part�culas se autodestruyen despu�s de 'particlesDuration'.
            }
            else { Debug.LogWarning("LampController: Prefab de part�culas de explosi�n no asignado. La rotura visual podr�a no aparecer.", this); }

            // 3. Desactivar TODOS los Renderers (visual) y Colliders de la l�mpara inmediatamente.
            foreach (Renderer r in lampRenderers) // <-- �CAMBIO AQU�!
            {
                if (r != null) r.enabled = false;
            }
            foreach (Collider c in lampColliders) // <-- �CAMBIO AQU�!
            {
                if (c != null) c.enabled = false;
            }

            // 4. Eliminar el objeto de la l�mpara despu�s de que el sonido y las part�culas hayan terminado.
            // Usar la duraci�n del sonido de impacto para el retraso, o un valor por defecto si no hay sonido.
            float destroyDelay = (impactSound != null) ? impactSound.length : 0.5f;
            Destroy(gameObject, destroyDelay);
        }
    }

    // ===============================================
    // L�gica del Parpadeo (Corrutinas)
    // ===============================================

    /// <summary>
    /// Corrutina para hacer que la luz de la l�mpara parpadee con patrones aleatorios,
    /// y gestionar el efecto de part�culas de cortocircuito y su sonido.
    /// </summary>
    IEnumerator FlickerLight()
    {
        while (!isBroken) // Bucle infinito hasta que la l�mpara se rompa.
        {
            // Cambiar estado de la luz.
            if (lampLight != null)
            {
                lampLight.enabled = !lampLight.enabled;
            }

            // Gestionar el efecto de part�culas de cortocircuito y su sonido basado en el estado de la luz.
            if (shortCircuitParticlesPrefab != null || electricitySound != null)
            {
                if (lampLight != null && lampLight.enabled) // Si la luz est� ENCENDIDA (o intentando encenderse)
                {
                    // Asegurar que las part�culas est�n activas o instanciadas.
                    if (shortCircuitParticlesPrefab != null)
                    {
                        if (currentShortCircuitParticles == null)
                        {
                            currentShortCircuitParticles = Instantiate(shortCircuitParticlesPrefab, transform.position, Quaternion.identity, transform);
                            // Padre al GameObject de la l�mpara para facilitar la gesti�n.
                        }
                        currentShortCircuitParticles.SetActive(true);
                    }

                    // Reproducir sonido de electricidad si no est� sonando ya.
                    if (electricitySound != null && lampAudioSource != null && !lampAudioSource.isPlaying)
                    {
                        lampAudioSource.clip = electricitySound;
                        lampAudioSource.loop = true; // Asegurar que el sonido de electricidad se repita.
                        lampAudioSource.Play();
                        Debug.Log("LampController: Iniciando sonido de electricidad.", this);
                    }
                }
                else // Si la luz est� APAGADA
                {
                    if (shortCircuitParticlesPrefab != null && currentShortCircuitParticles != null)
                    {
                        currentShortCircuitParticles.SetActive(false); // Ocultar part�culas cuando la luz est� apagada.
                    }

                    // Detener el sonido de electricidad si la luz est� apagada y el sonido est� activo.
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