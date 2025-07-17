using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteractable
{
    public int ammo = 10;
    public string itemName = "Loot Ammo";

    private AudioSource audioSource;
    private bool used = false;

    public bool destroyOnUse = true;
    public float destroyDelay = 0.1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("AmmoBox requires an AudioSource component.", this);
        }
    }

    public void Interact()
    {
        if (used) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddAmmo(ammo);
            //Debug.Log($"Added {ammo} ammo. Total: {GameManager.Instance.gunAmmo}");
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        if (destroyOnUse)
        {
            used = true;
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, destroyDelay);
        }
    }

    public string GetName()
    {
        return itemName;
    }
}
