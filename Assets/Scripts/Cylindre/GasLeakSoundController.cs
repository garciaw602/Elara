using UnityEngine;

public class GasLeakSoundController : MonoBehaviour
{
    [Tooltip("El AudioSource que reproducirá el sonido de escape de gas. Arrastra y suelta uno de los AudioSource de este GameObject aquí.")]
    public AudioSource gasAudioSource; // <<-- ¡AHORA ES PÚBLICO PARA ASIGNARLO EN EL INSPECTOR!

    [Tooltip("El sonido del escape de gas que se reproducirá.")]
    public AudioClip gasLeakSound;

    [Tooltip("La distancia máxima a la que el jugador debe estar para que el sonido de gas se active.")]
    public float gasSoundProximityThreshold = 3.0f;

    private bool playerInRangeForGasSound = false; // Bandera para controlar la reproducción del sonido de gas

    void Awake()
    {
        // Ahora usamos la referencia pública asignada en el Inspector
        if (gasAudioSource == null)
        {
            Debug.LogError("GasLeakSoundController: No se ha asignado un AudioSource en el campo 'Gas Audio Source' del Inspector. Deshabilitando script.", this);
            enabled = false;
            return;
        }

        // Configura el AudioSource para que el sonido de gas pueda bucle
        gasAudioSource.loop = true;
        gasAudioSource.playOnAwake = false; // Queremos controlar cuándo se reproduce
    }

    void Update()
    {
        if (gasLeakSound == null)
        {
            // Debug.LogWarning("GasLeakSoundController: No hay AudioClip asignado para el sonido de fuga de gas.", this);
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= gasSoundProximityThreshold)
            {
                // El jugador está dentro del rango
                if (!playerInRangeForGasSound)
                {
                    if (!gasAudioSource.isPlaying || gasAudioSource.clip != gasLeakSound)
                    {
                        gasAudioSource.Stop();
                        gasAudioSource.clip = gasLeakSound;
                        gasAudioSource.Play();
                        Debug.Log("GasLeakSoundController: Reproduciendo sonido de gas. Looping: " + gasAudioSource.loop, this);
                    }
                    playerInRangeForGasSound = true;
                }
                // (Opcional) Puedes añadir un log aquí si quieres confirmar que sigue en rango y sonando.
                // else { Debug.Log("GasLeakSoundController: Jugador aún en rango. Sonando: " + gasAudioSource.isPlaying); }
            }
            else // El jugador está fuera del rango
            {
                if (playerInRangeForGasSound)
                {
                    Debug.Log("GasLeakSoundController: Jugador SALIÓ del rango. Distancia: " + distanceToPlayer, this);
                    if (gasAudioSource.clip == gasLeakSound && gasAudioSource.isPlaying) // Solo detiene si está reproduciendo nuestro clip
                    {
                        gasAudioSource.Stop();
                        Debug.Log("GasLeakSoundController: Deteniendo sonido de gas.", this);
                    }
                    playerInRangeForGasSound = false;
                }
            }
        }
        else
        {
            // Debug.LogWarning("GasLeakSoundController: Player (con el tag 'Player') no encontrado en la escena.", this);
            if (gasAudioSource.isPlaying)
            {
                gasAudioSource.Stop();
                Debug.Log("GasLeakSoundController: Deteniendo sonido de gas (Player no encontrado).", this);
            }
            playerInRangeForGasSound = false;
        }
    }
}