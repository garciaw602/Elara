using UnityEngine;

/// <summary>
/// AudioZone: Controla la reproducci�n de un AudioClip cuando un GameObject con el tag "Player"
/// entra en un trigger. El audio se reproduce una sola vez y el GameObject se destruye
/// autom�ticamente al finalizar la reproducci�n del clip.
/// </summary>
[RequireComponent(typeof(BoxCollider))] // Asegura que siempre haya un BoxCollider
[RequireComponent(typeof(AudioSource))] // Asegura que siempre haya un AudioSource
public class AudioZone : MonoBehaviour
{
    [Header("Configuraci�n de Audio Zone")]
    [Tooltip("El AudioClip que se reproducir� cuando el jugador entre en esta zona.")]
    public AudioClip zoneAudioClip;

    [Tooltip("La referencia al componente AudioSource en este GameObject. Asigna uno o se buscar� autom�ticamente.")]
    public AudioSource zoneAudioSource; // Referencia al AudioSource

    [Tooltip("El tag del GameObject que debe activar este trigger (ej. 'Player').")]
    public string playerTag = "Player";

    private bool hasPlayed = false; // Controla si el audio ya se reprodujo

    /// <summary>
    /// Awake se llama cuando se carga la instancia del script.
    /// Inicializa el AudioSource.
    /// </summary>
    void Awake()
    {
        // Si no se asigna un AudioSource en el Inspector, intenta obtener uno en este GameObject.
        if (zoneAudioSource == null)
        {
            zoneAudioSource = GetComponent<AudioSource>();
        }

        // Si a�n no hay un AudioSource, muestra un error y deshabilita el script.
        if (zoneAudioSource == null)
        {
            Debug.LogError("AudioZone: No se encontr� un AudioSource en este GameObject o no se asign�. Deshabilitando script.", this);
            enabled = false; // Deshabilitar el script si no hay AudioSource
        }

        // Configurar el AudioSource
        if (zoneAudioSource != null)
        {
            zoneAudioSource.playOnAwake = false; // No queremos que suene al inicio
            zoneAudioSource.loop = false;       // Asegurar que no se repita
        }
    }

    /// <summary>
    /// OnTriggerEnter se llama cuando otro collider entra en este trigger.
    /// Controla la reproducci�n del audio al entrar y la posterior destrucci�n del objeto.
    /// </summary>
    /// <param name="other">El Collider del objeto que entr� en el trigger.</param>
    void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entr� tiene el tag del jugador y el audio no se ha reproducido.
        if (other.CompareTag(playerTag) && !hasPlayed)
        {
            if (zoneAudioClip != null && zoneAudioSource != null)
            {
                zoneAudioSource.clip = zoneAudioClip;
                zoneAudioSource.Play();
                hasPlayed = true; // Marcar que ya se reprodujo

                Debug.Log($"AudioZone: Reproduciendo audio '{zoneAudioClip.name}' y programando destrucci�n.", this);

                // Destruir el GameObject despu�s de que el audio termine de reproducirse.
                // Si el audioSource es nulo o el clip es nulo, se destruye inmediatamente.
                float destroyDelay = (zoneAudioSource != null && zoneAudioClip != null) ? zoneAudioClip.length : 0f;
                Destroy(gameObject, destroyDelay);
            }
            else if (zoneAudioClip == null)
            {
                Debug.LogWarning("AudioZone: No hay AudioClip asignado para reproducir. Destruyendo objeto inmediatamente.", this);
                Destroy(gameObject); // Destruir si no hay audio que reproducir
            }
        }
    }

    // OnTriggerExit ya no es necesario, ya que el objeto se destruir� al finalizar el audio.
}