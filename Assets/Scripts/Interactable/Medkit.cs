using UnityEngine;

public class Medkit : MonoBehaviour, IInteractable
{
    public float healAmount = 25f;
    public bool destroyOnUse = true;
    public float destroyDelay = 0.1f;

    public string itemName = "Heal";

    private bool used = false;

    public void Interact()
    {
        if (used) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);

                // TODO: Add sound or healing VFX here if you want
                // AudioSource.PlayClipAtPoint(healingSound, transform.position);
                // Instantiate(healingEffectPrefab, transform.position, Quaternion.identity);

                if (destroyOnUse)
                {
                    used = true;
                    GetComponent<Renderer>().enabled = false;
                    GetComponent<Collider>().enabled = false;
                    Destroy(gameObject, destroyDelay);
                }
            }
        }
    }

    public string GetName()
    {
        return itemName;
    }
}
