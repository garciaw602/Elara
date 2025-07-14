using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;            // Normal horizontal movement speed of the character
    public float sprintSpeed = 10f;         // Movement speed when the character is sprinting
    public float jumpForce = 8f;            // Force applied for jumping
    public Transform groundCheck;           // Transform to detect the ground (usually an empty GameObject under the character)
    public float groundDistance = 0.4f;     // Radius of the ground detection sphere
    public LayerMask groundMask;            // Layer(s) considered as ground        
    private Rigidbody rbPlayer;             // Reference to the player's Rigidbody component
    private bool isGrounded;                // Boolean to check if the player is touching the ground
    private PlayerAudioController audioController;
    private bool wasGrounded;
    public Animator animator;
    private bool jumpedInitiated = false; // Indica si el jugador ha iniciado un salto.
    void Start()
    {
        rbPlayer = GetComponent<Rigidbody>();           // Get the Rigidbody component at the start of the game
        rbPlayer.freezeRotation = true;                 // Ensure the Rigidbody doesn't rotate to keep the character upright

        audioController = GetComponent<PlayerAudioController>();
        if (audioController == null)
        {
            Debug.LogError("PlayerMovement requires a PlayerAudioController component on the same GameObject!");
            enabled = false;
            return; // Exit if component is missing to prevent NullReferenceException
        }

        // Initialize wasGrounded once at the start
        wasGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void Update()
    {
        // --- Ground Detection & Landing Logic ---
        // Get the current ground status
        bool currentIsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // If player was not grounded but is now grounded, it means they just landed
        if (!wasGrounded && currentIsGrounded)
        {
            if (jumpedInitiated)
            {
                audioController.PlayLandSound();
                jumpedInitiated = false; // Resetear la bandera una vez que aterriza del salto
            }
        }

        // Update the isGrounded flag for the rest of the current frame's logic
        isGrounded = currentIsGrounded;
        // Update wasGrounded for the next frame's comparison
        wasGrounded = currentIsGrounded;

        // If on the ground and its vertical velocity is negative (falling), adjust slightly
        if (isGrounded && rbPlayer.linearVelocity.y < 0)
        {
            rbPlayer.linearVelocity = new Vector3(rbPlayer.linearVelocity.x, -0.5f, rbPlayer.linearVelocity.z);
        }

        // --- Horizontal Movement (Ground Only) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;

        float currentSpeed = moveSpeed;
        bool isSprinting = false;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;
        }

        if (isGrounded)
        {
            rbPlayer.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rbPlayer.linearVelocity.y, moveDirection.z * currentSpeed);
        }

        // --- Jumping ---
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rbPlayer.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            audioController.PlayJumpSound(); // Call jump sound here
            jumpedInitiated = true; // El jugador ha iniciado un salto
        }

        // --- Footstep Logic ---
        // Calculate horizontal speed magnitude for footsteps
        float horizontalVelocityMagnitude = new Vector3(rbPlayer.linearVelocity.x, 0, rbPlayer.linearVelocity.z).magnitude;
        bool isMovingHorizontally = horizontalVelocityMagnitude > 0.1f; // Threshold to prevent sound when almost static

        audioController.HandleFootsteps(isMovingHorizontally, isSprinting, isGrounded);

        // --- Animator Updates ---
        if (animator != null)
        {
            // Update Speed parameter for blend tree
            // Use the magnitude of horizontal velocity for Speed
            // Make sure the speed is relative to the movement input, not just rigidbody velocity
            float inputMagnitude = new Vector3(x, 0, z).magnitude; // Get the magnitude of player input
            animator.SetFloat("Speed", inputMagnitude > 0.1f ? currentSpeed : 0); // Pass actual speed or 0 if no input

            animator.SetBool("IsGrounded", isGrounded); // Update IsGrounded parameter
            animator.SetBool("IsSprinting", isSprinting); // Update IsSprinting parameter (if not using Speed for blend)

            // Trigger Jump animation when jumping
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rbPlayer.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                audioController.PlayJumpSound(); // Call jump sound here
                animator.SetTrigger("Jump"); // Trigger jump animation
            }
        }
    }


}