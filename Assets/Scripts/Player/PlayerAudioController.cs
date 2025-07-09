using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Sound played when the player initiates a jump.")]
    public AudioClip jumpSound;
    [Tooltip("Sound played when the player lands after a jump.")]
    public AudioClip landSound;
    [Tooltip("Sound played repeatedly while the player is walking.")]
    public AudioClip walkFootstepSound; // 
    [Tooltip("Sound played repeatedly while the player is sprinting.")]
    public AudioClip sprintFootstepSound; //  

    [Header("Footstep Settings")]
    [Tooltip("Time delay between footstep sounds when walking.")]
    public float walkFootstepDelay = 0.5f;
    [Tooltip("Time delay between footstep sounds when sprinting.")]
    public float sprintFootstepDelay = 0.3f;

    private AudioSource audioSource;
    private Rigidbody rbPlayer;
    private float nextFootstepTime; // Controls when to play the next footstep sound

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rbPlayer = GetComponent<Rigidbody>();

        // Check if an AudioSource and Rigidbody are present
        if (audioSource == null)
        {
            Debug.LogError("PlayerAudioController requires an AudioSource component on the same GameObject!");
            enabled = false; // Disable the script if no AudioSource is found
        }
        if (rbPlayer == null)
        {
            Debug.LogError("PlayerAudioController requires a Rigidbody component on the same GameObject!");
            enabled = false; // Disable the script if no Rigidbody is found
        }
    }

    // --- Public Functions to be called by PlayerMovement ---

    /// <summary>
    /// Plays the jump sound effect.
    /// Should be called by PlayerMovement when the jump input is detected.
    /// </summary>
    public void PlayJumpSound()
    {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    /// <summary>
    /// Plays the land sound effect.
    /// Should be called by PlayerMovement when the player transitions from air to ground.
    /// </summary>
    public void PlayLandSound()
    {
        if (landSound != null)
        {
            audioSource.PlayOneShot(landSound);
        }
    }

    /// <summary>
    /// Handles playing footstep sounds based on movement speed and ground status.
    /// This function should be called from PlayerMovement's Update.
    /// </summary>
    /// <param name="isMoving">True if the player is moving horizontally.</param>
    /// <param name="isSprinting">True if the player is currently sprinting.</param>
    /// <param name="isGroundedParam">True if the player is currently on the ground.</param>
    public void HandleFootsteps(bool isMoving, bool isSprinting, bool isGroundedParam)
    {
        // Only play footsteps if the player is moving and on the ground
        if (isMoving && isGroundedParam)
        {
            float currentDelay = isSprinting ? sprintFootstepDelay : walkFootstepDelay;

            if (Time.time >= nextFootstepTime)
            {
                // Select the correct single AudioClip based on sprinting status
                AudioClip currentFootstepSound = isSprinting ? sprintFootstepSound : walkFootstepSound;

                if (currentFootstepSound != null)
                {
                    audioSource.PlayOneShot(currentFootstepSound);
                }
                nextFootstepTime = Time.time + currentDelay;
            }
        }
    }
}