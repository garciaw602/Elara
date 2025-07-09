using UnityEngine;
using UnityEngine.AI; 

public class EnemyAI : MonoBehaviour
{
    public Transform target; 
    public float detectionRadius = 10f; // Distancia para detectar al jugador
    public float fieldOfViewAngle = 90f; // Ángulo de visión del enemigo
    public LayerMask visionObstacleMask; // Capa de obstáculos que bloquean la visión 

    private NavMeshAgent agent;
    private bool playerDetectedByDistance = false;
    private bool playerDetectedByVision = false;

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
                enabled = false;
            }
        }
    }

    void Update()
    {
        if (target == null || !agent.enabled) return;

        // 1. Detección por Distancia
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        playerDetectedByDistance = (distanceToTarget <= detectionRadius);

        // 2. Detección por Visión (Line of Sight)
        playerDetectedByVision = false;
        if (distanceToTarget <= detectionRadius) // Solo si está dentro del radio de detección general
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Calcular si el objetivo está dentro del campo de visión (FOV)
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

        // Lógica de Movimiento
        if (playerDetectedByVision)
        {
            agent.SetDestination(target.position);
            LookAtTarget();
        }
        else if (playerDetectedByDistance)
        {
            agent.SetDestination(target.position);
            LookAtTarget();
        }
        else
        {
            // Implementar un comportamiento de patrulla
            if (agent.hasPath)
            {
                agent.ResetPath();
            }
        }
    }

    void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0; // Para que el enemigo no se incline en el eje Y
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed); 
    }

    // 
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Radio de detección
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Campo de visión
            Gizmos.color = Color.blue;
            Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);
            Gizmos.DrawWireSphere(transform.position + transform.forward * detectionRadius, 0.5f); // Un punto al final del FOV
        }
    }
}