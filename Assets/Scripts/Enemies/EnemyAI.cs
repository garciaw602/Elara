using UnityEngine;
using UnityEngine.AI; 

public class EnemyAI : MonoBehaviour
{
    public Transform target; 
    public float detectionRadius = 10f; // Distancia para detectar al jugador
    public float fieldOfViewAngle = 90f; // Ángulo de visión del enemigo
    public LayerMask visionObstacleMask; // Capa de obstáculos que bloquean la visión 

    
    //Velocidad del enemigo cuando está patrullando
    public float patrolSpeed = 2f;
    //Velocidad del enemigo cuando persigue al jugador
    public float chaseSpeed = 4f;

    //Puntos a los que el enemigo se moverá durante la patrulla
    public Transform[] patrolPoints;
    //Distancia mínima al punto de patrulla para considerarlo alcanzado
    public float patrolPointThreshold = 1f;
    //Tiempo de espera en cada punto de patrulla antes de ir al siguiente
    public float waitTimeAtPatrolPoint = 1f;

    private NavMeshAgent agent;
    private bool playerDetectedByDistance = false;
    private bool playerDetectedByVision = false;

    private int currentPatrolPointIndex = 0;
    private bool isWaitingAtPatrolPoint = false;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado en el enemigo. Asegúrate de añadirlo.");
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
                Debug.LogWarning("No se asignó un objetivo (jugador) al script EnemyAI y no se encontró un GameObject con la etiqueta 'Player'.");
                //enabled = false;
            }
        }

        // Inicializar el primer punto de patrulla si hay puntos definidos
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
            agent.speed = patrolSpeed; // Establece la velocidad inicial de patrulla
        }
    }

    void Update()
    {
        // Si el agente NavMesh no está activo (por ejemplo, si el enemigo está muerto), o si no hay un target asignado
        // y no hay puntos de patrulla definidos, no hace nada.
        if (!agent.enabled || (target == null && (patrolPoints == null || patrolPoints.Length == 0))) return;

        // Detección del Jugador
        playerDetectedByVision = false; // Reinicia en cada frame

        if (target != null) // Solo si hay un jugador a quien detectar
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            playerDetectedByDistance = (distanceToTarget <= detectionRadius);

            if (playerDetectedByDistance)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                
                if (Vector3.Angle(transform.forward, directionToTarget) < fieldOfViewAngle / 2)
                {
                    // Comprobar la línea de visión (raycast)
                    RaycastHit hit;
                    Vector3 startRay = transform.position + Vector3.up * 0.5f; // Desde los "ojos" del enemigo
                    Vector3 endRay = target.position + Vector3.up * 0.5f; // Hacia el "centro" del jugador

                    if (!Physics.Linecast(startRay, endRay, visionObstacleMask))
                    {
                        playerDetectedByVision = true;
                    }
                }
            }
        }

        // --- Comportamiento del Enemigo ---
        if (playerDetectedByVision)
        {
            // El jugador está a la vista, perseguir
            agent.speed = chaseSpeed;
            agent.SetDestination(target.position);
            LookAtTarget();
            isWaitingAtPatrolPoint = false; // Cancela cualquier espera de patrulla
        }
        else if (playerDetectedByDistance && target != null) // Si el jugador está cerca pero no a la vista, seguir persiguiendo
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(target.position);
            isWaitingAtPatrolPoint = false;
        }
        else // Si el jugador no está detectado en absoluto, patrullar
        {
            agent.speed = patrolSpeed; // Asegura la velocidad de patrulla

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Patrol();
            }
            else
            {
                // Si no hay puntos de patrulla y no hay jugador, simplemente detenerse.
                if (agent.hasPath)
                {
                    agent.ResetPath();
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
            // Si el enemigo ha llegado al punto de patrulla actual
            if (agent.remainingDistance < patrolPointThreshold && !agent.pathPending)
            {
                isWaitingAtPatrolPoint = true;
                waitTimer = waitTimeAtPatrolPoint;
            }
            // Si no está esperando y no tiene un destino, ve al destino actual
            else if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) // O si está atascado
            {
                // Que tenga un destino si está patrullando
                agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
            }
        }
    }

    void GoToNextPatrolPoint()
    {
        currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length; // Ciclo entre los puntos
        agent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
    }

    void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0; // Para que el enemigo no se incline en el eje Y
        if (lookPos != Vector3.zero) // Evita errores si lookPos es cero
        {
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed);
        }
    }

    // --- Gizmos para Depuración ---
    void OnDrawGizmosSelected()
    {
        // Dibuja el radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Dibuja el campo de visión
        Gizmos.color = Color.blue;
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        // Dibuja la línea de visión al target si está detectado
        if (target != null && playerDetectedByVision)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, target.position + Vector3.up * 0.5f);
        }

        // Dibuja los puntos de patrulla
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.5f); // Punto de esfera
                    if (i < patrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position); // Línea al siguiente
                    }
                    else if (patrolPoints.Length > 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position); // Línea al primero para cerrar el ciclo
                    }
                }
            }
        }
    }
}