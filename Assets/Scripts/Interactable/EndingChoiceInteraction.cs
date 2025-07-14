using UnityEngine;

public class EndingChoiceInteraction : MonoBehaviour
{
    [Tooltip("La elección de final asociada a este objeto.")]
    public GameEndingManager.EndingChoice thisChoice = GameEndingManager.EndingChoice.GoodEnding;

    [Tooltip("Referencia al GameEndingManager en la escena.")]
    public GameEndingManager gameEndingManager;

    [Tooltip("El rango en el que el jugador puede interactuar.")]
    public float interactionRange = 3f;

    [Tooltip("Mensaje para mostrar al jugador cuando esté cerca.")]
    public GameObject interactionPromptUI; 

    private GameObject player;
    private bool playerInRange = false;

    void Start()
    {
        // Encuentra el GameEndingManager si no está asignado en el Inspector
        if (gameEndingManager == null)
        {
            gameEndingManager = FindObjectOfType<GameEndingManager>();
            if (gameEndingManager == null)
            {
                Debug.LogError("GameEndingManager no encontrado en la escena. Asegúrate de tener uno.");
            }
        }

        // Encuentra el jugador por su Tag
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No se encontró un GameObject con el Tag 'Player'. Asegúrate de que tu jugador tenga este Tag.");
        }

        
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player != null && gameEndingManager != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= interactionRange)
            {
                if (!playerInRange) // Entró en rango
                {
                    playerInRange = true;
                    if (interactionPromptUI != null)
                    {
                        interactionPromptUI.SetActive(true);
                    }
                    Debug.Log("Jugador cerca de " + gameObject.name);
                }

                if (Input.GetKeyDown(KeyCode.E)) 
                {
                    gameEndingManager.MakeEndingChoice(thisChoice);
                }
            }
            else
            {
                if (playerInRange) // Salió de rango
                {
                    playerInRange = false;
                    if (interactionPromptUI != null)
                    {
                        interactionPromptUI.SetActive(false);
                    }
                    Debug.Log("Jugador fuera de rango de " + gameObject.name);
                }
            }
        }
    }

    // Dibujar el rango de interacción en el Editor para depuración
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}