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
    [Tooltip("�El spawner debe generar enemigos al inicio del juego hasta el l�mite de Max Active Enemies?")]
    public bool spawnOnStart = true;
    [Tooltip("M�ximo de enemigos que este spawner puede tener vivos al mismo tiempo.")]
    public int maxActiveEnemies = 3;

    [Header("Puntos de Aparici�n (Spawn Points)")]
    [Tooltip("Arrastra GameObjects vac�os aqu� para definir d�nde pueden aparecer los enemigos. Un enemigo aparecer� en un punto aleatorio de esta lista.")]
    public Transform[] spawnPoints;

    // --- �NUEVA CONFIGURACI�N PARA M�LTIPLES RUTAS DE PATRULLA! ---
    [Header("M�ltiples Rutas de Patrulla para Enemigos Spawneados")]
    [Tooltip("Define aqu� diferentes rutas de patrulla. Cada enemigo generado por este spawner recibir� una ruta aleatoria de esta lista.")]
    public List<PatrolRouteGroup> patrolRoutes = new List<PatrolRouteGroup>();

    [System.Serializable] // Permite que esta clase se serialice y aparezca en el Inspector
    public class PatrolRouteGroup
    {
        [Tooltip("Define los puntos de una ruta de patrulla espec�fica para los enemigos.")]
        public Transform[] routePoints;
    }
    // -----------------------------------------------------------

    [Header("Configuraci�n de Respawn por Proximidad")]
    [Tooltip("Habilitar respawn solo cuando el jugador est� lejos del �rea de spawn.")]
    public bool enableProximityRespawn = true;
    [Tooltip("Distancia m�nima que el jugador debe estar del spawner para que el respawn ocurra.")]
    public float minDistanceForRespawn = 15f;

    private int currentActiveEnemiesCount = 0;
    private float nextSpawnTime = 0f;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("EnemySpawner: No se encontr� un GameObject con la etiqueta 'Player'. Respawn por proximidad podr�a no funcionar correctamente.", this);
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("El Spawner '" + gameObject.name + "' no tiene puntos de aparici�n asignados. Asigna GameObjects vac�os en el Inspector.", this);
            enabled = false;
            return;
        }

        // Verificar que hay al menos una ruta de patrulla si se espera que los enemigos patrullen
        if (patrolRoutes.Count == 0 || (patrolRoutes.Count == 1 && patrolRoutes[0].routePoints.Length == 0))
        {
            Debug.LogWarning("El Spawner '" + gameObject.name + "' no tiene rutas de patrulla definidas. Los enemigos no patrullar�n.", this);
        }

        nextSpawnTime = Time.time + respawnTime;

        if (spawnOnStart)
        {
            for (int i = 0; i < maxActiveEnemies; i++)
            {
                SpawnEnemy();
            }
        }
    }

    void Update()
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
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < minDistanceForRespawn)
            {
                nextSpawnTime = Time.time + 1f;
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

        // Seleccionar un punto de aparici�n aleatorio
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawnPoint = spawnPoints[randomSpawnIndex];

        // Instancia el enemigo
        GameObject newEnemy = Instantiate(enemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene el script EnemyHealth, no se podr� detectar su muerte.", newEnemy);
        }

        // --- ASIGNAR RUTA DE PATRULLA ALEATORIA DESDE LA LISTA ---
        EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            if (patrolRoutes.Count > 0)
            {
                // Seleccionar una ruta de patrulla aleatoria de la lista
                int randomRouteIndex = Random.Range(0, patrolRoutes.Count);
                PatrolRouteGroup selectedRouteGroup = patrolRoutes[randomRouteIndex];

                if (selectedRouteGroup.routePoints != null && selectedRouteGroup.routePoints.Length > 0)
                {
                    enemyAI.SetPatrolPoints(selectedRouteGroup.routePoints); // Asigna los puntos de la ruta seleccionada
                }
                else
                {
                    Debug.LogWarning("La ruta de patrulla seleccionada est� vac�a para el enemigo generado por " + gameObject.name, this);
                    enemyAI.SetPatrolPoints(null); // Asegurarse de que no haya una ruta si est� vac�a
                }
            }
            else
            {
                Debug.LogWarning("No hay rutas de patrulla definidas en el spawner '" + gameObject.name + "'. El enemigo no patrullar�.", this);
                enemyAI.SetPatrolPoints(null); // No hay rutas que asignar
            }

            enemyAI.ResetAIState(); // Aseg�rate de resetear el estado de la IA despu�s de asignar
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
    }

    // Dibujo de Gizmos para visualizar el spawner, el radio de respawn y los puntos de spawn
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        if (enableProximityRespawn)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, minDistanceForRespawn);
        }

        // Dibuja los puntos de aparici�n
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Gizmos.color = Color.green; // Nuevo color para spawn points
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.7f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }

        // Dibuja TODAS las rutas de patrulla definidas en el spawner
        if (patrolRoutes != null && patrolRoutes.Count > 0)
        {
            // Colores para diferenciar las rutas
            Color[] routeColors = { Color.yellow, Color.magenta, Color.blue, Color.white, Color.grey };

            for (int i = 0; i < patrolRoutes.Count; i++)
            {
                PatrolRouteGroup routeGroup = patrolRoutes[i];
                if (routeGroup.routePoints != null && routeGroup.routePoints.Length > 0)
                {
                    Gizmos.color = routeColors[i % routeColors.Length]; // Ciclo de colores

                    for (int j = 0; j < routeGroup.routePoints.Length; j++)
                    {
                        if (routeGroup.routePoints[j] != null)
                        {
                            Gizmos.DrawSphere(routeGroup.routePoints[j].position, 0.4f);
                            // Dibuja una l�nea al siguiente punto o al inicio si es una ruta cerrada
                            if (j < routeGroup.routePoints.Length - 1)
                            {
                                Gizmos.DrawLine(routeGroup.routePoints[j].position, routeGroup.routePoints[j + 1].position);
                            }
                            else if (routeGroup.routePoints.Length > 1) // Cierra la ruta si tiene m�s de 1 punto
                            {
                                Gizmos.DrawLine(routeGroup.routePoints[j].position, routeGroup.routePoints[0].position);
                            }
                        }
                    }
                }
            }
        }
    }
}