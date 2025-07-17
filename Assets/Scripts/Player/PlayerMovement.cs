using UnityEngine;
using UnityEngine.UI; // Asegúrate de que esta línea esté presente para usar Slider

/// <summary>
/// PlayerMovement: Este script gestiona el movimiento del jugador, incluyendo
/// caminar, esprintar, saltar, detección de suelo y actualizaciones del Animator,
/// e incorpora un sistema de estamina para controlar el esprint.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento horizontal normal del personaje.")]
    public float moveSpeed = 5f;
    [Tooltip("Velocidad de movimiento cuando el personaje está esprintando.")]
    public float sprintSpeed = 10f;
    [Tooltip("Fuerza aplicada para el salto.")]
    public float jumpForce = 8f;
    [Tooltip("Transform para detectar el suelo (usualmente un GameObject vacío bajo el personaje).")]
    public Transform groundCheck;
    [Tooltip("Radio de la esfera de detección de suelo.")]
    public float groundDistance = 0.4f;
    [Tooltip("Capas consideradas como suelo (ej. 'Default', 'Floor').")]
    public LayerMask groundMask;

    [Tooltip("La capa de los GameObjects que son 'Enemigos'. También se considerarán suelo para saltar.")]
    public LayerMask enemyLayer; // <-- ¡NUEVA VARIABLE!

    [Header("Configuración de Estamina")]
    [Tooltip("Estamina máxima del jugador.")]
    public float maxStamina = 100f;
    [Tooltip("Estamina actual del jugador. Se inicializa al máximo.")]
    public float currentStamina;
    [Tooltip("Velocidad a la que la estamina disminuye al esprintar (por segundo).")]
    public float staminaDrainRate = 15f;
    [Tooltip("Velocidad a la que la estamina se recupera cuando no se esprinta (por segundo).")]
    public float staminaRegenRate = 10f;
    [Tooltip("Tiempo de retraso antes de que la estamina empiece a regenerarse después de dejar de esprintar.")]
    public float staminaRegenDelay = 1.0f;
    [Tooltip("Estamina mínima requerida para poder iniciar o mantener el esprint.")]
    public float minStaminaToSprint = 10f;
    [Tooltip("Slider de la UI que muestra la estamina del jugador. Asigna uno si tienes UI de estamina.")]
    public Slider StaminaBarSlider; // Referencia al Slider dentro del Canvas instanciado


    // Referencias y Variables Internas
    private Rigidbody rbPlayer;             // Referencia al componente Rigidbody del jugador
    private bool isGrounded;                // Booleano para verificar si el jugador está tocando el suelo
    private PlayerAudioController audioController;
    private bool wasGrounded;
    [Tooltip("Asigna aquí el componente Animator de tu GameObject de jugador.")]
    public Animator animator;               // <-- Campo para arrastrar el componente Animator
    private bool jumpedInitiated = false;   // Indica si el jugador ha iniciado un salto.

    private bool canSprint = true;          // Controla si el jugador tiene suficiente estamina para esprintar
    private float regenDelayTimer;          // Temporizador para el retraso de regeneración de estamina

    /// <summary>
    /// Start se llama antes de la primera actualización del frame.
    /// Inicializa componentes y variables.
    /// </summary>
    void Start()
    {
        rbPlayer = GetComponent<Rigidbody>();           // Obtener el componente Rigidbody al inicio del juego
        rbPlayer.freezeRotation = true;                 // Asegurar que el Rigidbody no rote para mantener el personaje erguido

        audioController = GetComponent<PlayerAudioController>();
        if (audioController == null)
        {
            Debug.LogError("PlayerMovement requiere un componente PlayerAudioController en el mismo GameObject.", this);
            enabled = false;
            return; // Salir si el componente falta para evitar errores de referencia nula
        }

        // Inicializar wasGrounded una vez al inicio
        // ¡AHORA COMBINAMOS AMBAS CAPAS PARA LA DETECCIÓN DE SUELO!
        wasGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask | enemyLayer);

        // Inicializar Estamina
        currentStamina = maxStamina;
        regenDelayTimer = staminaRegenDelay;
        // Asegurarse de que el Slider esté conectado y con el valor inicial
        if (StaminaBarSlider != null)
        {
            StaminaBarSlider.maxValue = maxStamina;
            StaminaBarSlider.value = currentStamina;
        }
    }

    /// <summary>
    /// Update se llama una vez por frame.
    /// Gestiona la detección de suelo, movimiento, salto, estamina y animaciones.
    /// </summary>
    void Update()
    {
        // --- Detección de Suelo y Lógica de Aterrizaje ---
        // Obtener el estado actual del suelo
        // ¡AHORA COMBINAMOS AMBAS CAPAS PARA LA DETECCIÓN DE SUELO!
        bool currentIsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask | enemyLayer);

        // Si el jugador no estaba en el suelo pero ahora lo está, significa que acaba de aterrizar
        if (!wasGrounded && currentIsGrounded)
        {
            if (jumpedInitiated)
            {
                audioController.PlayLandSound();
                jumpedInitiated = false; // Resetear la bandera una vez que aterriza del salto
            }
        }

        // Actualizar la bandera isGrounded para el resto de la lógica del frame actual
        isGrounded = currentIsGrounded;
        // Actualizar wasGrounded para la comparación del siguiente frame
        wasGrounded = currentIsGrounded;

        // Si está en el suelo y su velocidad vertical es negativa (cayendo), ajustar ligeramente
        if (isGrounded && rbPlayer.linearVelocity.y < 0)
        {
            rbPlayer.linearVelocity = new Vector3(rbPlayer.linearVelocity.x, -0.5f, rbPlayer.linearVelocity.z);
        }

        // --- Movimiento Horizontal (Solo en el Suelo) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;

        float currentSpeed = moveSpeed;
        bool isSprintingInput = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        bool isCurrentlySprinting = false; // Estado real de esprint (input + estamina)

        // Lógica de Estamina para Esprintar
        // El jugador intenta esprintar, tiene suficiente estamina, está en el suelo y se está moviendo.
        if (isSprintingInput && currentStamina > minStaminaToSprint && isGrounded && moveDirection.magnitude > 0.1f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime; // Consume estamina al esprintar
            isCurrentlySprinting = true;
            regenDelayTimer = staminaRegenDelay; // Reinicia el temporizador de regeneración
        }
        else
        {
            // Si no está esprintando (no hay input, poca estamina, no en el suelo o no se mueve)
            if (regenDelayTimer > 0)
            {
                regenDelayTimer -= Time.deltaTime; // Reduce el temporizador de retraso
            }
            else
            {
                currentStamina += staminaRegenRate * Time.deltaTime; // Regenera estamina
            }
        }

        // Asegurar que la estamina no exceda los límites
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        // Actualizar Slider de Estamina si está asignado
        if (StaminaBarSlider != null)
        {
            StaminaBarSlider.value = currentStamina;
        }

        // Si la estamina es muy baja, no permitir esprintar aunque se mantenga Shift
        if (currentStamina < minStaminaToSprint)
        {
            canSprint = false;
        }
        else
        {
            canSprint = true; // Permitir esprintar si la estamina es suficiente
        }

        // Aplicar velocidad de esprint si el jugador está presionando el botón Y PUEDE esprintar
        if (isSprintingInput && canSprint && isCurrentlySprinting)
        {
            currentSpeed = sprintSpeed;
        }


        if (isGrounded)
        {
            // Aplicar velocidad de movimiento. La velocidad vertical se mantiene.
            rbPlayer.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rbPlayer.linearVelocity.y, moveDirection.z * currentSpeed);
        }

        // --- Saltar ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rbPlayer.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            audioController.PlayJumpSound(); // Reproducir sonido de salto
            jumpedInitiated = true; // El jugador ha iniciado un salto
        }

        // --- Lógica de Pasos ---
        // Calcular la magnitud de la velocidad horizontal para los pasos
        float horizontalVelocityMagnitude = new Vector3(rbPlayer.linearVelocity.x, 0, rbPlayer.linearVelocity.z).magnitude;
        bool isMovingHorizontally = horizontalVelocityMagnitude > 0.1f; // Umbral para evitar sonido cuando está casi estático

        // Pasar el estado real de esprint (teniendo en cuenta la estamina) al controlador de audio
        audioController.HandleFootsteps(isMovingHorizontally, isCurrentlySprinting, isGrounded);

        // --- Actualizaciones del Animator ---
        if (animator != null)
        {
            // Actualizar el parámetro Speed para el blend tree
            // Usar la magnitud del input horizontal para la velocidad del Animator
            float inputMagnitude = new Vector3(x, 0, z).magnitude; // Obtener la magnitud del input del jugador
            // Si hay input, pasar la velocidad actual; de lo contrario, 0.
            animator.SetFloat("Speed", inputMagnitude > 0.1f ? currentSpeed : 0);

            animator.SetBool("IsGrounded", isGrounded); // Actualizar parámetro IsGrounded
            animator.SetBool("IsSprinting", isCurrentlySprinting); // Actualizar IsSprinting (el estado real, con estamina)

            // Activar animación de Salto al saltar
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                // La fuerza y el sonido ya se aplican arriba.
                animator.SetTrigger("Jump"); // Activar animación de salto
            }
        }
    }

    // ===============================================
    // Métodos Públicos para Acceder a la Estamina (para la UI)
    // ===============================================

    /// <summary>
    /// Obtiene la estamina actual del jugador.
    /// </summary>
    /// <returns>La cantidad actual de estamina.</returns>
    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    /// <summary>
    /// Obtiene la estamina máxima del jugador.
    /// </summary>
    /// <returns>La cantidad máxima de estamina.</returns>
    public float GetMaxStamina()
    {
        return maxStamina;
    }

    /// <summary>
    /// Verifica si el jugador tiene suficiente estamina y permiso para esprintar.
    /// </summary>
    /// <returns>True si el jugador puede esprintar, de lo contrario false.</returns>
    public bool CanPlayerSprint()
    {
        return canSprint;
    }
}