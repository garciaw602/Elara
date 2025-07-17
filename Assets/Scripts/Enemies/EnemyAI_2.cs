using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_2 : MonoBehaviour
{
    public Transform target;
    public float detectionRadius = 10f; // Distancia para detectar al jugador (para la IA de persecuci�n)
    public float fieldOfViewAngle = 90f; // �ngulo de visi�n del enemigo
    public LayerMask visionObstacleMask; // Capa de obst�culos que bloquean la visi�n 

    // --- Configuraci�n de Audio de Voz del Zombie ---
    [Header("Zombie Voice Settings")]
    [Tooltip("Los clips de audio para la voz del zombie (gemidos, gru�idos, etc.). Se elegir� uno aleatoriamente para reproducir en loop.")]
    public AudioClip[] zombieVoiceClips; // Array de clips de voz
    [Tooltip("El radio dentro del cual el jugador puede escuchar la voz del zombie y activar/detener el loop.")]
    public float voiceDetectionRadius = 8f; // Distancia para activar el sonido de voz

    private NavMeshAgent agent;
    private bool playerDetectedByDistance = false;
    private bool playerDetectedByVision = false;

    private AudioSource audioSource;

    // Bandera para detectar la entrada/salida de la zona de voz (para iniciar/detener el loop)
    private bool playerInVoiceZone = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado en el enemigo. Aseg�rate de a�adirlo.", this);
            enabled = false;
        }
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("No se asign� un objetivo (jugador) al script EnemyAI2 y no se encontr� un GameObject con la etiqueta 'Player'.", this);
                enabled = false;
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("EnemyAI2 requiere un componente AudioSource en el mismo GameObject para la voz del zombie.", this);
            enabled = false;
            return;
        }

        // �IMPORTANTE! Aunque tengas Loop marcado en el Inspector, si quieres asegurarte
        // de que el loop se activa por c�digo, puedes a�adir esta l�nea.
        // Pero lo m�s importante es no usar PlayOneShot.
        audioSource.loop = true; // Aseg�rate de que el AudioSource est� configurado para loop
        audioSource.playOnAwake = false;
        // audioSource.spatialBlend = 1f; // Aseg�rate de que los sonidos sean 3D
    }

    void Update()
    {
        if (target == null || !agent.enabled)
        {
            if (playerInVoiceZone)
            {
                audioSource.Stop();
                playerInVoiceZone = false;
            }
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        playerDetectedByDistance = (distanceToTarget <= detectionRadius);

        playerDetectedByVision = false;
        if (distanceToTarget <= detectionRadius)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < fieldOfViewAngle / 2)
            {
                RaycastHit hit;
                Vector3 startRay = transform.position + Vector3.up * 0.5f;
                Vector3 endRay = target.position + Vector3.up * 0.5f;

                if (!Physics.Linecast(startRay, endRay, visionObstacleMask))
                {
                    playerDetectedByVision = true;
                }
            }
        }

        // --- L�gica para la Voz del Zombie en Loop al Entrar en Zona ---
        if (audioSource != null && zombieVoiceClips != null && zombieVoiceClips.Length > 0)
        {
            if (distanceToTarget <= voiceDetectionRadius) // Si el jugador est� dentro de la zona de voz
            {
                // Si el jugador acaba de entrar en la zona de voz (transici�n de fuera a dentro)
                if (!playerInVoiceZone)
                {
                    // Seleccionar un clip aleatorio del array
                    int randomIndex = Random.Range(0, zombieVoiceClips.Length);
                    AudioClip clipToPlay = zombieVoiceClips[randomIndex];

                    if (clipToPlay != null)
                    {
                        audioSource.Stop();       // Detener cualquier sonido anterior que est� sonando
                        audioSource.clip = clipToPlay; // <--- CAMBIO CLAVE: Asignar el clip al AudioSource
                        audioSource.loop = true;  // <--- ASEGURARSE DE QUE EST� EN LOOP (si no est� ya en el Inspector)
                        audioSource.Play();       // <--- CAMBIO CLAVE: Iniciar la reproducci�n en loop
                    }
                    playerInVoiceZone = true; // Marcar que el jugador est� ahora en la zona de voz
                }
                // Si el jugador ya est� en la zona, el sonido sigue en loop, no se hace nada m�s aqu�.
            }
            else // Si el jugador est� fuera de la zona de voz
            {
                // Si el jugador acaba de salir de la zona de voz (transici�n de dentro a fuera)
                if (playerInVoiceZone)
                {
                    audioSource.Stop();       // Detener el sonido en loop
                    audioSource.loop = false; // <--- Opcional: Desactivar el loop expl�citamente al salir
                    playerInVoiceZone = false; // Marcar que el jugador ya no est� en la zona de voz
                }
            }
        }
        // --- FIN L�GICA DE VOZ ---

        // L�gica de movimiento de la IA
        if (playerDetectedByVision || playerDetectedByDistance)
        {
            agent.SetDestination(target.position);
            LookAtTarget();
        }
        else
        {
            if (agent.hasPath)
            {
                agent.ResetPath(); // Detiene el movimiento si no se detecta al jugador
            }
        }
    }

    void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed);
    }

    // Dibujo de Gizmos para depuraci�n en el Editor de Unity
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Radio de detecci�n de la IA (amarillo)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Campo de visi�n de la IA (azul)
            Gizmos.color = Color.blue;
            Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);
            Gizmos.DrawWireSphere(transform.position + transform.forward * detectionRadius, 0.5f);

            // Radio de detecci�n de voz (magenta)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, voiceDetectionRadius);
        }
    }
}