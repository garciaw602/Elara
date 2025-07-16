using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Necesario para List

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuraci�n del Spawner")]
    [Tooltip("El prefab del enemigo que se va a generar.")]
    public GameObject enemyPrefab;
    [Tooltip("Tiempo que tarda un enemigo en reaparecer despu�s de ser derrotado.")]
    public float respawnTime = 5f;
    // [Tooltip("�El spawner debe generar enemigos al inicio del juego hasta el l�mite de Max Active Enemies?")]
    // public bool spawnOnStart = true; // <--- ELIMINAMOS O DESACTIVAMOS ESTA VARIABLE
    [Tooltip("M�ximo de enemigos que este spawner puede tener vivos al mismo tiempo.")]
    public int maxActiveEnemies = 3;

    [Header("Puntos de Aparici�n (Spawn Points)")]
    [Tooltip("Arrastra GameObjects vac�os aqu� para definir d�nde pueden aparecer los enemigos. Un enemigo aparecer� en un punto aleatorio y disponible de esta lista.")]
    public Transform[] spawnPoints;

    [Header("M�ltiples Rutas de Patrulla para Enemigos Spawneados")]
    [Tooltip("Define aqu� diferentes rutas de patrulla. Cada enemigo generado por este spawner recibir� una ruta aleatoria de esta lista.")]
    public List<PatrolRouteGroup> patrolRoutes = new List<PatrolRouteGroup>();

    [System.Serializable]
    public class PatrolRouteGroup
    {
        [Tooltip("Define los puntos de una ruta de patrulla espec�fica para los enemigos.")]
        public Transform[] routePoints;
    }

    // --- NUEVAS VARIABLES PARA ACTIVACI�N POR PROXIMIDAD ---
    [Header("Activaci�n del Spawner por Proximidad")]
    [Tooltip("Si est� activado, el spawner solo generar� enemigos cuando el jugador est� dentro del rango de activaci�n.")]
    public bool activateOnPlayerProximity = true;
    [Tooltip("Distancia a la que el jugador debe acercarse al spawner para que este comience a generar enemigos.")]
    public float activationDistance = 20f;
    private bool spawnerActivated = false; // Indica si el spawner ya ha sido activado por proximidad

    // --- Variables de Respawn por Proximidad (Mantener si a�n lo quieres) ---
    [Header("Configuraci�n de Respawn por Proximidad")]
    [Tooltip("Habilitar respawn solo cuando el jugador est� lejos del �rea de spawn.")]
    public bool enableProximityRespawn = true;
    [Tooltip("Distancia m�nima que el jugador debe estar del spawner para que el respawn ocurra.")]
    public float minDistanceForRespawn = 15f;


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
            Debug.LogWarning("EnemySpawner: No se encontr� un GameObject con la etiqueta 'Player'. Respawn y activaci�n por proximidad podr�an no funcionar correctamente.", this);
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("El Spawner '" + gameObject.name + "' no tiene puntos de aparici�n asignados. Asigna GameObjects vac�os en el Inspector.", this);
            enabled = false;
            return;
        }

        if (patrolRoutes.Count == 0 || (patrolRoutes.Count == 1 && patrolRoutes[0].routePoints.Length == 0))
        {
            Debug.LogWarning("El Spawner '" + gameObject.name + "' no tiene rutas de patrulla definidas. Los enemigos no patrullar�n.", this);
        }

        // --- CAMBIO CLAVE EN START ---
        // Si la activaci�n por proximidad est� activa, el spawner no genera nada al inicio.
        // Si no est� activa, o si el player no se encontr�, establece el timer para el primer spawn normal.
        if (activateOnPlayerProximity)
        {
            spawnerActivated = false; // Asegura que el spawner empieza inactivo
            // No hacemos ning�n spawn inicial aqu�.
        }
        else
        {
            // Comportamiento original si no hay activaci�n por proximidad: Generar todos al inicio.
            for (int i = 0; i < maxActiveEnemies; i++)
            {
                SpawnEnemy();
            }
        }
        // El nextSpawnTime siempre se inicializa para futuras comprobaciones de spawn.
        nextSpawnTime = Time.time + respawnTime;
    }

    void Update()
    {
        // --- NUEVA L�GICA DE ACTIVACI�N POR PROXIMIDAD ---
        if (activateOnPlayerProximity && !spawnerActivated)
        {
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= activationDistance)
                {
                    spawnerActivated = true; // El spawner ha sido activado
                    Debug.Log("Spawner " + gameObject.name + " activado por proximidad del jugador.");
                    // Una vez activado, podemos forzar el primer spawn o esperar el timer
                    // Aqu� generamos todos los enemigos iniciales que el spawner debe tener.
                    for (int i = 0; i < maxActiveEnemies; i++)
                    {
                        SpawnEnemy();
                    }
                    nextSpawnTime = Time.time + respawnTime; // Reinicia el timer para el siguiente respawn
                }
            }
            return; // Si no est� activado a�n, no hagas nada m�s en este Update
        }

        // --- L�gica existente (ahora solo se ejecuta si el spawner est� activado) ---
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
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < minDistanceForRespawn)
            {
                nextSpawnTime = Time.time + 1f; // Espera un poco m�s si el jugador est� cerca
                return;
            }
        }

        SpawnEnemy();
        nextSpawnTime = Time.time + respawnTime;
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

        if (availableSpawnPointIndices.Count == 0)
        {
            InitializeSpawnPoints();
            Debug.Log("Todos los spawn points han sido utilizados. Reiniciando la lista de disponibles.");
            if (availableSpawnPointIndices.Count == 0)
            {
                Debug.LogWarning("No hay spawn points disponibles para generar el enemigo.", this);
                return;
            }
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
            Debug.LogWarning("El prefab del enemigo no tiene el script EnemyHealth, no se podr� detectar su muerte.", newEnemy);
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
                    Debug.LogWarning("La ruta de patrulla seleccionada est� vac�a para el enemigo generado por " + gameObject.name, this);
                    enemyAI.SetPatrolPoints(null);
                }
            }
            else
            {
                Debug.LogWarning("No hay rutas de patrulla definidas en el spawner '" + gameObject.name + "'. El enemigo no patrullar�.", this);
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

    // --- Gizmos para Depuraci�n ---
    void OnDrawGizmosSelected()
    {
        // Dibuja el radio de activaci�n del spawner
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

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
                        Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i + 1].position); // L�nea al siguiente
                    }
                    else if (spawnPoints.Length > 1)
                    {
                        Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[0].position); // L�nea al primero para cerrar el ciclo
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