using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 90f;
    public LayerMask visionObstacleMask;

    // Audio
    [Header("Zombie Voice Settings")]
    [Tooltip("Los clips de audio para la voz del zombie (gemidos, gruñidos, etc.). Se elegirá uno aleatoriamente para reproducir en loop.")]
    public AudioClip[] zombieVoiceClips;
    [Tooltip("El radio dentro del cual el jugador puede escuchar la voz del zombie y activar/detener el loop.")]
    public float voiceDetectionRadius = 8f;

    private AudioSource audioSource;
    private bool playerInVoiceZone = false;

    // Patrullaje
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
                Debug.LogWarning("No se encontró un GameObject con la etiqueta 'Player'.", this);
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
            Debug.LogError("EnemyAI requiere un componente AudioSource.", this);
            enabled = false;
            return;
        }
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (!agent.enabled || (target == null && (patrolPoints == null || patrolPoints.Length == 0))) return;

        float distanceToTarget = target != null ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;
        playerDetectedByDistance = (distanceToTarget <= detectionRadius);
        playerDetectedByVision = false;

        if (target != null && playerDetectedByDistance)
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

        HandleAudio(distanceToTarget);

        if (playerDetectedByVision || (playerDetectedByDistance && target != null))
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(target.position);
            LookAtTarget();
            isWaitingAtPatrolPoint = false;
        }
        else
        {
            agent.speed = patrolSpeed;

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Patrol();
            }
            else
            {
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
            }
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
            if (agent.remainingDistance < patrolPointThreshold && !agent.pathPending)
            {
                isWaitingAtPatrolPoint = true;
                waitTimer = waitTimeAtPatrolPoint;
            }
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

    public void SetPatrolPoints(Transform[] newPatrolPoints)
    {
        if (newPatrolPoints != null && newPatrolPoints.Length > 0)
        {
            patrolPoints = newPatrolPoints;
            currentPatrolPointIndex = 0;
        }
        else
        {
            patrolPoints = null;
            Debug.LogWarning("EnemyAI: No se proporcionaron puntos de patrulla válidos.");
        }
    }

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
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.blue;
            Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRadius;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, voiceDetectionRadius);

            if (playerDetectedByVision)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, target.position + Vector3.up * 0.5f);
            }
        }

        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.5f);
                    if (i < patrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints.Length > 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }
}
