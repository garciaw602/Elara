using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public int ammo = 10;
    private AudioSource audioSource;
    void Awake() // Usamos Awake porque se llama apenas el objeto es creado
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("AmmoBoxLoot requiere un componente AudioSource en el mismo GameObject.", this);
            return; 
        }

        // Si hay un AudioClip asignado al AudioSource en el Inspector del Prefab, lo reproducimos.
        if (audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip); // Reproduce el sonido una vez
        }
        else
        {
            Debug.LogWarning("No hay AudioClip asignado al AudioSource en AmmoBoxLoot Prefab.", this);
        }
    }
}
