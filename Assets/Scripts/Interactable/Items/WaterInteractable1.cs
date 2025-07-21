using UnityEngine;

public class WaterInteractable1 : MonoBehaviour, IInteractable
{
    public string itemName = "Open Water";
    [Tooltip("Audio clip to play when the door toggles.")]
    public AudioClip waterSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        // Play sound
        if (waterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(waterSound);
        }
    }

    public string GetName()
    {
        return itemName;
    }
}
