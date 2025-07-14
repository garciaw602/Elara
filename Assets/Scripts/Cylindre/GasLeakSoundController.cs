using UnityEngine;

public class GasLeakSoundController : MonoBehaviour
{
    [Tooltip("El AudioSource que reproducir� el sonido de escape de gas. Arrastra y suelta uno de los AudioSource de este GameObject aqu�.")]
    public AudioSource gasAudioSource; // <<-- �AHORA ES P�BLICO PARA ASIGNARLO EN EL INSPECTOR!

    [Tooltip("El sonido del escape de gas que se reproducir�.")]
    public AudioClip gasLeakSound;

    [Tooltip("La distancia m�xima a la que el jugador debe estar para que el sonido de gas se active.")]
    public float gasSoundProximityThreshold = 3.0f;

    private bool playerInRangeForGasSound = false; // Bandera para controlar la reproducci�n del sonido de gas

    void Awake()
    {
        // Ahora usamos la referencia p�blica asignada en el Inspector
        if (gasAudioSource == null)
        {
            Debug.LogError("GasLeakSoundController: No se ha asignado un AudioSource en el campo 'Gas Audio Source' del Inspector. Deshabilitando script.", this);
            enabled = false;
            return;
        }

        // Configura el AudioSource para que el sonido de gas pueda bucle
        gasAudioSource.loop = true;
        gasAudioSource.playOnAwake = false; // Queremos controlar cu�ndo se reproduce
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
                // El jugador est� dentro del rango
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
                // (Opcional) Puedes a�adir un log aqu� si quieres confirmar que sigue en rango y sonando.
                // else { Debug.Log("GasLeakSoundController: Jugador a�n en rango. Sonando: " + gasAudioSource.isPlaying); }
            }
            else // El jugador est� fuera del rango
            {
                if (playerInRangeForGasSound)
                {
                    Debug.Log("GasLeakSoundController: Jugador SALI� del rango. Distancia: " + distanceToPlayer, this);
                    if (gasAudioSource.clip == gasLeakSound && gasAudioSource.isPlaying) // Solo detiene si est� reproduciendo nuestro clip
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