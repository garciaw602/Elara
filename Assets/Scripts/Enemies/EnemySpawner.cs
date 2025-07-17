using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Necesario para List

public class EnemySpawner : MonoBehaviour
{
    // ----- Configuración del Spawner -------
    public GameObject enemyPrefab; //El prefab del enemigo que se va a generar
    public float respawnTime = 5f; //Tiempo que tarda un enemigo en reaparecer después de ser derrotado
    // ¿El spawner debe generar enemigos al inicio del juego hasta el límite de Max Active Enemies?
    // public bool spawnOnStart = true;
    public int maxActiveEnemies = 3;//Máximo de enemigos que este spawner puede tener vivos al mismo tiempo

    //------- Puntos de Aparición (Spawn Points) -------
    //Arrastrar los GameObjects vacíos aquí para definir dónde pueden aparecer los enemigos. Un enemigo aparecerá en un punto aleatorio y disponible de esta lista
    public Transform[] spawnPoints;

    //Múltiples Rutas de Patrulla para Enemigos Spawneados
    //Define aquí diferentes rutas de patrulla. Cada enemigo generado por este spawner recibirá una ruta aleatoria de esta lista
    public List<PatrolRouteGroup> patrolRoutes = new List<PatrolRouteGroup>();

    [System.Serializable]
    public class PatrolRouteGroup
    {
        [Tooltip("Define los puntos de una ruta de patrulla específica para los enemigos.")]
        public Transform[] routePoints;
    }

    // --- NUEVAS VARIABLES PARA ACTIVACIÓN POR PROXIMIDAD ---
    //------- Activación del Spawner por Proximidad -------
    public bool activateOnPlayerProximity = true;  //Si está activado, el spawner solo generará enemigos cuando el jugador esté dentro del rango de activación
    public float activationDistance = 20f; //Distancia a la que el jugador debe acercarse al spawner para que este comience a generar enemigos
    private bool spawnerActivated = false; // Indica si el spawner ya ha sido activado por proximidad

    // --- Variables de Respawn por Proximidad (existentes, para evitar respawn si el jugador está muy cerca) ---
    // ------- Configuración de Respawn por Proximidad -------
    public bool enableProximityRespawn = true; // Habilitar respawn solo cuando el jugador esté lejos del área de spawn
    public float minDistanceForRespawn = 15f; // Distancia mínima que el jugador debe estar del spawner para que el respawn ocurra.

    // --- NUEVAS VARIABLES PARA EL REINICIO DE ZONA POR AUSENCIA ---
    [Header("Reinicio de Zona por Ausencia del Jugador")]
    [Tooltip("Si el jugador se aleja de esta distancia por el tiempo especificado, la zona se 'reiniciará' para permitir un respawn completo al regresar.")]
    public bool enableZoneResetOnPlayerAbsence = true;
    [Tooltip("Distancia a la que el jugador debe alejarse del spawner para que el temporizador de ausencia comience.")]
    public float deactivationDistance = 25f; // Ligeramente mayor que activationDistance
    [Tooltip("Tiempo que el jugador debe permanecer fuera de la 'deactivationDistance' para que la zona se reinicie.")]
    public float zoneResetTime = 30f; // 30 segundos como pediste

    private float playerLeftAreaTime = -Mathf.Infinity; // Momento en que el jugador dejó la zona
    private bool zoneReadyForReset = false; // Indica si la zona está lista para un reinicio completo al re-entrar

    private int currentActiveEnemiesCount = 0;
    private float nextSpawnTime = 0f;
    private Transform playerTransform;

    private List<int> availableSpawnPointIndices;
    private Dictionary<GameObject, int> enemySpawnPointMap;

    void Awake()
    {
        InitializeSpawnPoints();
    }

    void InitializeSpawnPoints()
    {
        availableSpawnPointIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawnPointIndices.Add(i);
        }
        enemySpawnPointMap = new Dictionary<GameObject, int>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("EnemySpawner: No se encontró un GameObject con la etiqueta 'Player'. Respawn y activación por proximidad podrían no funcionar correctamente.", this);
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("El Spawner '" + gameObject.name + "' no tiene puntos de aparición asignados. Asigna GameObjects vacíos en el Inspector.", this);
            enabled = false;
            return;
        }

        if (patrolRoutes.Count == 0 || (patrolRoutes.Count == 1 && patrolRoutes[0].routePoints.Length == 0))
        {
            Debug.LogWarning("El Spawner '" + gameObject.name + "' no tiene rutas de patrulla definidas. Los enemigos no patrullarán.", this);
        }

        // Si la activación por proximidad está activa, el spawner no genera nada al inicio.
        if (activateOnPlayerProximity)
        {
            spawnerActivated = false; // Asegurarse que el spawner empieza inactivo
        }
        else
        {
            // Comportamiento original si no hay activación por proximidad: Generar todos al inicio.
            for (int i = 0; i < maxActiveEnemies; i++)
            {
                SpawnEnemy();
            }
        }
        nextSpawnTime = Time.time + respawnTime;
    }

    void Update()
    {
        if (playerTransform == null) return; // Si no hay jugador, no hacer nada.

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- LÓGICA DE REINICIO DE ZONA POR AUSENCIA DEL JUGADOR ---
        if (enableZoneResetOnPlayerAbsence)
        {
            if (spawnerActivated && distanceToPlayer > deactivationDistance)
            {
                // El jugador ha salido de la zona activa. Iniciar o actualizar el temporizador de ausencia.
                if (playerLeftAreaTime == -Mathf.Infinity) // Si es la primera vez que sale
                {
                    playerLeftAreaTime = Time.time;
                    Debug.Log($"Jugador salió de la zona del spawner '{gameObject.name}'. Iniciando temporizador de reinicio.");
                }

                // Si el jugador ha estado fuera el tiempo suficiente y la zona no está ya lista para reiniciar
                if (Time.time >= playerLeftAreaTime + zoneResetTime && !zoneReadyForReset)
                {
                    zoneReadyForReset = true;
                    Debug.Log($"Zona del spawner '{gameObject.name}' lista para reinicio completo al re-entrar.");
                }
            }
            else if (distanceToPlayer <= activationDistance)
            {
                // El jugador ha re-entrado en la zona de activación.
                if (!spawnerActivated) // Si el spawner no estaba activado (primera entrada)
                {
                    spawnerActivated = true;
                    Debug.Log("Spawner " + gameObject.name + " activado por proximidad del jugador.");
                    // Generar todos los enemigos iniciales
                    for (int i = 0; i < maxActiveEnemies; i++)
                    {
                        SpawnEnemy();
                    }
                    nextSpawnTime = Time.time + respawnTime;
                    playerLeftAreaTime = -Mathf.Infinity; // Resetear el temporizador de ausencia
                    zoneReadyForReset = false; // Resetear la bandera de reinicio
                }
                else if (zoneReadyForReset) // Si el spawner ya estaba activado Y la zona está lista para reiniciar
                {
                    Debug.Log($"Jugador re-entró en la zona del spawner '{gameObject.name}'. Reiniciando enemigos.");
                    // Reiniciar el conteo de enemigos activos y generar hasta el máximo
                    // currentActiveEnemiesCount = 0; // Opcional: si quieres que se respawneen TODOS, incluso los que no murieron.
                    // Si solo quieres rellenar los que faltan, no resetees currentActiveEnemiesCount.
                    InitializeSpawnPoints(); // Para asegurar que todos los puntos están disponibles de nuevo
                    for (int i = currentActiveEnemiesCount; i < maxActiveEnemies; i++)
                    {
                        SpawnEnemy();
                    }
                    nextSpawnTime = Time.time + respawnTime;
                    playerLeftAreaTime = -Mathf.Infinity; // Resetear el temporizador de ausencia
                    zoneReadyForReset = false; // Resetear la bandera de reinicio
                }
                else
                {
                    // Jugador dentro de la zona, pero no es la primera activación ni un reinicio.
                    // Esto es para el respawn normal de enemigos muertos uno por uno.
                }
                playerLeftAreaTime = -Mathf.Infinity; // Resetear el temporizador de ausencia si el jugador está dentro
            }
            else
            {
                // Jugador fuera de la zona de activación pero dentro de la de desactivación, o spawner inactivo y jugador lejos.
                // No hacer nada especial con los timers de ausencia.
            }
        }
        else // Si enableZoneResetOnPlayerAbsence está desactivado, el comportamiento es el anterior
        {
            if (activateOnPlayerProximity && !spawnerActivated)
            {
                if (distanceToPlayer <= activationDistance)
                {
                    spawnerActivated = true;
                    Debug.Log("Spawner " + gameObject.name + " activado por proximidad del jugador.");
                    for (int i = 0; i < maxActiveEnemies; i++)
                    {
                        SpawnEnemy();
                    }
                    nextSpawnTime = Time.time + respawnTime;
                }
                return;
            }
        }


        // --- Lógica de respawn normal (solo se ejecuta si el spawner está activado y no está en proceso de reinicio) ---
        // Y si el spawner no está esperando un reinicio de zona completo
        if (spawnerActivated && !zoneReadyForReset)
        {
            if (currentActiveEnemiesCount >= maxActiveEnemies)
            {
                return;
            }

            if (Time.time < nextSpawnTime)
            {
                return;
            }

            if (enableProximityRespawn && playerTransform != null)
            {
                if (distanceToPlayer < minDistanceForRespawn)
                {
                    nextSpawnTime = Time.time + 1f; // Espera un poco más si el jugador está cerca
                    return;
                }
            }

            SpawnEnemy();
            nextSpawnTime = Time.time + respawnTime;
        }
    }

    void SpawnEnemy()
    {
        if (currentActiveEnemiesCount >= maxActiveEnemies)
        {
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab no asignado en el spawner: " + gameObject.name, this);
            return;
        }

        // Si todos los puntos están ocupados, no se puede spawnear
        if (availableSpawnPointIndices.Count == 0)
        {
            Debug.LogWarning("No hay spawn points disponibles para generar el enemigo en el spawner: " + gameObject.name + ". Esperando liberación.", this);
            return;
        }

        int randomAvailableIndexInList = Random.Range(0, availableSpawnPointIndices.Count);
        int selectedSpawnPointOriginalIndex = availableSpawnPointIndices[randomAvailableIndexInList];

        availableSpawnPointIndices.RemoveAt(randomAvailableIndexInList);

        Transform selectedSpawnPoint = spawnPoints[selectedSpawnPointOriginalIndex];

        GameObject newEnemy = Instantiate(enemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        enemySpawnPointMap.Add(newEnemy, selectedSpawnPointOriginalIndex);

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene el script EnemyHealth, no se podrá detectar su muerte.", newEnemy);
        }

        EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            // Ajustar el target del enemigo spawneado al jugador
            if (playerTransform != null)
            {
                enemyAI.target = playerTransform;
            }
            else
            {
                Debug.LogWarning("Player Transform no encontrado para asignar al EnemyAI del nuevo enemigo.", newEnemy);
            }

            if (patrolRoutes.Count > 0)
            {
                int randomRouteIndex = Random.Range(0, patrolRoutes.Count);
                PatrolRouteGroup selectedRouteGroup = patrolRoutes[randomRouteIndex];

                if (selectedRouteGroup.routePoints != null && selectedRouteGroup.routePoints.Length > 0)
                {
                    enemyAI.SetPatrolPoints(selectedRouteGroup.routePoints);
                }
                else
                {
                    Debug.LogWarning("La ruta de patrulla seleccionada está vacía para el enemigo generado por " + gameObject.name, this);
                    enemyAI.SetPatrolPoints(null);
                }
            }
            else
            {
                Debug.LogWarning("No hay rutas de patrulla definidas en el spawner '" + gameObject.name + "'. El enemigo no patrullará.", this);
                enemyAI.SetPatrolPoints(null);
            }

            enemyAI.ResetAIState();
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene el script EnemyAI, no se le puede asignar una ruta de patrulla.", newEnemy);
        }

        currentActiveEnemiesCount++;
        Debug.Log("Enemigo " + newEnemy.name + " generado por " + gameObject.name + " en " + selectedSpawnPoint.name + ". Total activos: " + currentActiveEnemiesCount + "/" + maxActiveEnemies);
    }

    public void EnemyDied(GameObject deadEnemy)
    {
        currentActiveEnemiesCount--;
        Debug.Log("Enemigo de " + gameObject.name + " ha muerto. Enemigos activos restantes: " + currentActiveEnemiesCount + "/" + maxActiveEnemies);

        if (enemySpawnPointMap.ContainsKey(deadEnemy))
        {
            int freedSpawnPointIndex = enemySpawnPointMap[deadEnemy];
            if (!availableSpawnPointIndices.Contains(freedSpawnPointIndex))
            {
                availableSpawnPointIndices.Add(freedSpawnPointIndex);
                Debug.Log($"Spawn Point {freedSpawnPointIndex} liberado.");
            }
            enemySpawnPointMap.Remove(deadEnemy);
        }
    }

    // --- Gizmos para Depuración ---
    void OnDrawGizmosSelected()
    {
        // Dibuja el radio de activación del spawner
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        // Dibuja el radio de desactivación del spawner
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deactivationDistance);

        // ... (El resto de tus Gizmos existentes, como puntos de spawn y rutas de patrulla) ...
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    Gizmos.DrawSphere(spawnPoints[i].position, 0.5f); // Punto de esfera
                    if (i < spawnPoints.Length - 1)
                    {
                        Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i + 1].position); // Línea al siguiente
                    }
                    else if (spawnPoints.Length > 1)
                    {
                        Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[0].position); // Línea al primero para cerrar el ciclo
                    }
                }
            }
        }

        // Dibuja las rutas de patrulla (opcional, si quieres que se vean en el editor)
        if (patrolRoutes != null && patrolRoutes.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var routeGroup in patrolRoutes)
            {
                if (routeGroup.routePoints != null && routeGroup.routePoints.Length > 1)
                {
                    for (int i = 0; i < routeGroup.routePoints.Length - 1; i++)
                    {
                        if (routeGroup.routePoints[i] != null && routeGroup.routePoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(routeGroup.routePoints[i].position, routeGroup.routePoints[i + 1].position);
                            Gizmos.DrawSphere(routeGroup.routePoints[i].position, 0.3f);
                        }
                    }
                    if (routeGroup.routePoints[routeGroup.routePoints.Length - 1] != null)
                    {
                        Gizmos.DrawSphere(routeGroup.routePoints[routeGroup.routePoints.Length - 1].position, 0.3f);
                    }
                }
            }
        }
    }
}