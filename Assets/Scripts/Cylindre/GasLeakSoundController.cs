using UnityEngine;

/// <summary>
/// GasLeakSoundController: Este script controla la reproducción del sonido de fuga de gas
/// basado en la proximidad del jugador y se detiene cuando el cilindro explota.
/// </summary>
public class GasLeakSoundController : MonoBehaviour
{
    [Tooltip("El AudioSource que reproducirá el sonido de escape de gas. Arrastra y suelta uno de los AudioSource de este GameObject aquí.")]
    public AudioSource gasAudioSource;

    [Tooltip("El sonido del escape de gas que se reproducirá.")]
    public AudioClip gasLeakSound;

    [Tooltip("La distancia máxima a la que el jugador debe estar para que el sonido de gas se active.")]
    public float gasSoundProximityThreshold = 3.0f;

    private bool playerInRangeForGasSound = false; // Bandera para controlar la reproducción del sonido de gas

    void Awake()
    {
        // Asegúrate de que el AudioSource esté asignado.
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
        // Si no hay AudioClip asignado, no intentamos reproducir nada.
        if (gasLeakSound == null)
        {
            return;
        }

        // Buscar al jugador por Tag.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Si el jugador entra en rango
            if (distanceToPlayer <= gasSoundProximityThreshold)
            {
                if (!playerInRangeForGasSound) // Si acaba de entrar en rango
                {
                    // Asegurarse de que el sonido no se esté reproduciendo o que sea el correcto antes de iniciarlo
                    if (!gasAudioSource.isPlaying || gasAudioSource.clip != gasLeakSound)
                    {
                        gasAudioSource.Stop(); // Detener cualquier sonido anterior
                        gasAudioSource.clip = gasLeakSound; // Asignar el clip de gas
                        gasAudioSource.Play(); // Iniciar la reproducción
                        Debug.Log("GasLeakSoundController: Reproduciendo sonido de gas. Looping: " + gasAudioSource.loop, this);
                    }
                    playerInRangeForGasSound = true;
                }
            }
            else // Si el jugador está fuera del rango
            {
                if (playerInRangeForGasSound) // Si acaba de salir del rango
                {
                    Debug.Log("GasLeakSoundController: Jugador SALIÓ del rango. Distancia: " + distanceToPlayer, this);
                    if (gasAudioSource.clip == gasLeakSound && gasAudioSource.isPlaying) // Solo detiene si está reproduciendo nuestro clip
                    {
                        gasAudioSource.Stop(); // Detener el sonido de gas
                        Debug.Log("GasLeakSoundController: Deteniendo sonido de gas.", this);
                    }
                    playerInRangeForGasSound = false;
                }
            }
        }
        else // Si el Player no se encuentra en la escena
        {
            if (gasAudioSource.isPlaying)
            {
                gasAudioSource.Stop();
                Debug.Log("GasLeakSoundController: Deteniendo sonido de gas (Player no encontrado).", this);
            }
            playerInRangeForGasSound = false;
        }
    }

    /// <summary>
    /// Detiene el sonido de fuga de gas. Este método es llamado por el GasCylinder cuando explota.
    /// </summary>
    public void StopGasLeakSound()
    {
        if (gasAudioSource != null && gasAudioSource.isPlaying)
        {
            gasAudioSource.Stop();
            Debug.Log("GasLeakSoundController: Sonido de fuga de gas detenido por explosión del cilindro.", this);
        }
        enabled = false; // Deshabilitamos el script una vez que el gas ha explotado y el sonido ha parado.
    }
}