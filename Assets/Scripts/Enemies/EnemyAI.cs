using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 90f;
    public LayerMask visionObstacleMask;

    [Header("Zombie Voice Settings")]
    public AudioClip[] zombieVoiceClips;
    public float voiceDetectionRadius = 8f;

    private AudioSource audioSource;
    private bool playerInVoiceZone = false;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform[] patrolPoints;
    public float patrolPointThreshold = 1f;
    public float waitTimeAtPatrolPoint = 1f;

    private NavMeshAgent agent;
    private bool playerDetectedByDistance = false;
    private bool playerDetectedByVision = false;

    private int currentPatrolPointIndex = 0;
    private bool isWaitingAtPatrolPoint = false;
    private float waitTimer = 0f;

    // Control de la barra de vida
    private EnemyHealth enemyHealth; // Referencia al script EnemyHealth
    private bool wasChasingPlayer = false; // Bandera para detectar cambios de estado de persecución

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado en el enemigo.", this);
            enabled = false;
            return;
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
                Debug.LogWarning("No se encontró un GameObject con la etiqueta 'Player'. Asegúrate de que el jugador tenga esa etiqueta.", this);
            }
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
            agent.speed = patrolSpeed;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("EnemyAI requiere un componente AudioSource para reproducir sonidos de zombie.", this);
            enabled = false;
            return;
        }
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Obtener referencia a EnemyHealth
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyHealth no encontrado en el enemigo. Asegúrate de que el script EnemyHealth esté en el mismo GameObject del zombie.", this);
            enabled = false;
            return;
        }
        // Inicialmente, la barra de vida está oculta. ¡Esta línea es crucial!
        enemyHealth.SetHealthBarVisibility(false);
    }

    void Update()
    {
        // Si el agente no está habilitado o no hay objetivo/puntos de patrulla, no hacer nada.
        if (!agent.enabled || (target == null && (patrolPoints == null || patrolPoints.Length == 0))) return;

        float distanceToTarget = target != null ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;
        playerDetectedByDistance = (distanceToTarget <= detectionRadius);
        playerDetectedByVision = false;

        if (target != null && playerDetectedByDistance)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Verificar si el jugador está dentro del campo de visión del enemigo.
            if (Vector3.Angle(transform.forward, directionToTarget) < fieldOfViewAngle / 2)
            {
                RaycastHit hit;
                Vector3 startRay = transform.position + Vector3.up * 0.5f; // Origen del rayo un poco por encima del suelo.
                Vector3 endRay = target.position + Vector3.up * 0.5f;     // Destino del rayo un poco por encima del suelo.

                // Lanzar un Linecast para verificar si hay obstáculos entre el enemigo y el jugador.
                if (!Physics.Linecast(startRay, endRay, visionObstacleMask))
                {
                    playerDetectedByVision = true; // El jugador es visible.
                }
            }
        }

        HandleAudio(distanceToTarget);

        // Lógica de comportamiento y visibilidad de la barra de vida
        bool currentlyChasingPlayer = false; // Variable para rastrear el estado actual de persecución

        // Si el jugador es visible por visión o está muy cerca (por distancia).
        if (playerDetectedByVision || (playerDetectedByDistance && target != null))
        {
            agent.speed = chaseSpeed; // Cambia a velocidad de persecución.
            agent.SetDestination(target.position); // Establece el destino al jugador.
            LookAtTarget(); // Hace que el enemigo mire al jugador.
            isWaitingAtPatrolPoint = false; // Detiene cualquier espera en punto de patrulla.
            currentlyChasingPlayer = true; // El enemigo está persiguiendo.
        }
        else // Si el jugador no es detectado, vuelve a patrullar.
        {
            agent.speed = patrolSpeed; // Cambia a velocidad de patrullaje.

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Patrol(); // Ejecuta la lógica de patrullaje.
            }
            else // Si no hay puntos de patrulla, detiene el movimiento.
            {
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
            }
            currentlyChasingPlayer = false; // El enemigo no está persiguiendo.
        }

        // Lógica para controlar la visibilidad de la barra de vida:
        if (enemyHealth != null) // Asegúrate de que la referencia no sea nula antes de usarla
        {
            if (currentlyChasingPlayer && !wasChasingPlayer)
            {
                // El enemigo acaba de empezar a perseguir al jugador, mostrar la barra de vida.
                enemyHealth.SetHealthBarVisibility(true);
            }
            else if (!currentlyChasingPlayer && wasChasingPlayer)
            {
                // El enemigo acaba de dejar de perseguir al jugador, ocultar la barra de vida.
                enemyHealth.SetHealthBarVisibility(false);
            }
            wasChasingPlayer = currentlyChasingPlayer; // Actualiza el estado para el próximo frame.
        }
    }

    void HandleAudio(float distanceToTarget)
    {
        if (audioSource != null && zombieVoiceClips != null && zombieVoiceClips.Length > 0)
        {
            if (distanceToTarget <= voiceDetectionRadius)
            {
                if (!playerInVoiceZone)
                {
                    int randomIndex = Random.Range(0, zombieVoiceClips.Length);
                    AudioClip clipToPlay = zombieVoiceClips[randomIndex];

                    if (clipToPlay != null)
                    {
                        audioSource.Stop();
                        audioSource.clip = clipToPlay;
                        audioSource.loop = true;
                        audioSource.Play();
                    }
                    playerInVoiceZone = true;
                }
            }
            else
            {
                if (playerInVoiceZone)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                    playerInVoiceZone = false;
                }
            }
        }
    }

    void Patrol()
    {
        if (isWaitingAtPatrolPoint)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaitingAtPatrolPoint = false;
                GoToNextPatrolPoint();
            }
        }
        else
        {
            // Si el agente ha llegado o está muy cerca del punto de patrulla.
            // agent.remainingDistance es la distancia al final del camino.
            // !agent.pathPending asegura que el cálculo del camino haya terminado.
            if (agent.remainingDistance < patrolPointThreshold && !agent.pathPending)
            {
                isWaitingAtPatrolPoint = true;
                waitTimer = waitTimeAtPatrolPoint;
            }
            // Si el agente no tiene un camino o no se está moviendo (se quedó atascado o llegó), establece el destino.
            else if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
            }
        }
    }

    void GoToNextPatrolPoint()
    {
        currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
    }

    /// <summary>
    /// Establece nuevos puntos de patrulla para el enemigo.
    /// </summary>
    /// <param name="newPatrolPoints">Un array de Transforms que representan los nuevos puntos.</param>
    public void SetPatrolPoints(Transform[] newPatrolPoints)
    {
        if (newPatrolPoints != null && newPatrolPoints.Length > 0)
        {
            patrolPoints = newPatrolPoints;
            currentPatrolPointIndex = 0; // Reinicia al primer punto.
            // Asegura que el agente vaya al primer punto si ya está activo.
            if (agent != null && agent.enabled)
            {
                agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
                agent.speed = patrolSpeed;
            }
        }
        else
        {
            patrolPoints = null; // No hay puntos de patrulla.
            Debug.LogWarning("EnemyAI: No se proporcionaron puntos de patrulla válidos. El enemigo permanecerá estático o perseguirá si detecta.", this);
            if (agent != null && agent.enabled && agent.hasPath)
            {
                agent.ResetPath(); // Detiene el movimiento actual.
            }
        }
    }

    /// <summary>
    /// Reinicia el estado de la IA del enemigo a sus valores de patrullaje iniciales.
    /// Útil después de que el enemigo ha sido dañado o ha perdido el rastro del jugador.
    /// </summary>
    public void ResetAIState()
    {
        currentPatrolPointIndex = 0;
        isWaitingAtPatrolPoint = false;
        waitTimer = 0f;
        if (agent != null)
        {
            agent.ResetPath();
            agent.speed = patrolSpeed;
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
            }
        }
    }

    void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0; // Mantiene la rotación solo en el eje Y (horizontal).
        if (lookPos != Vector3.zero) // Evita errores si la posición es la misma.
        {
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            // Suaviza la rotación para que el enemigo gire gradualmente.
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed);
        }
    }

    // Dibujo de Gizmos para depuración en el Editor de Unity
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Radio de detección de la IA (amarillo)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Campo de visión de la IA (azul)
            Gizmos.color = Color.blue;
            Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);

            // Radio de la voz del zombie (magenta)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, voiceDetectionRadius);

            // Línea de visión si el jugador es detectado (rojo)
            if (playerDetectedByVision)
            {
                Gizmos.color = Color.red;
                // Dibuja una línea desde un punto elevado del enemigo al jugador para simular la línea de visión real.
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, target.position + Vector3.up * 0.5f);
            }
        }

        // Dibujo de puntos de patrulla (verde)
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.5f); // Dibuja una esfera en cada punto.
                    // Dibuja líneas entre los puntos para mostrar el camino de patrulla.
                    if (i < patrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints.Length > 1) // Cierra el ciclo de patrulla si hay más de un punto.
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }
}