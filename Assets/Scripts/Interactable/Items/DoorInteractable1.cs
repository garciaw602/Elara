using UnityEngine;

public class DoorInteractable1 : MonoBehaviour, IInteractable
{
    public string itemName = "Open Door";
    private bool isRotated = false; // Add this at the class level
    [Tooltip("The Y-axis rotation the door should toggle to.")]
    public float targetYRotation = 140.78f;
    [Tooltip("Audio clip to play when the door toggles.")]
    public AudioClip doorSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        // Rotate
        float y = isRotated ? 0f : targetYRotation;
        transform.rotation = Quaternion.Euler(0f, y, 0f);
        isRotated = !isRotated;

        // Play sound
        if (doorSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorSound);
        }
    }

    public string GetName()
    {
        return itemName;
    }
}
