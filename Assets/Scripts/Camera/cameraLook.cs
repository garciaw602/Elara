using UnityEngine;

public class cameraLook : MonoBehaviour
{
    [Tooltip("Sensitivity of the mouse for camera rotation.")]
    public float mouseSensitivity = 80f;
    [Tooltip("Transform of the player's body (for horizontal rotation).")]
    public Transform playerBody;
    private float xRotation = 0f; // Stores the current vertical rotation (pitch)

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Make the cursor invisible
    }

    private void Update()
    {
        // Get mouse input for X and Y axes, scaled by sensitivity and deltaTime for frame-rate independence.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // Mouse X movement (horizontal).
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // Mouse Y movement (vertical).

        // Calculate vertical rotation (pitch). Subtract mouseY because Unity's Y-axis is inverted for camera rotation.
        xRotation -= mouseY;
        // Clamp the vertical rotation to prevent the camera from looking too far up or down (e.g., flipping over).
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        // Apply the calculated vertical rotation to the camera itself (local rotation).
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player's body horizontally (yaw) based on horizontal mouse movement.
        // CAMBIO: Rotate around the global Y-axis to prevent displacement.
        playerBody.Rotate(Vector3.up * mouseX, Space.World);
    }
}