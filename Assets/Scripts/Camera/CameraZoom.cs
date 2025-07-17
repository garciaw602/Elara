using UnityEngine;

/// <summary>
/// CameraZoom: Gestiona el efecto de zoom de la c�mara (simulando una mira)
/// al presionar el bot�n derecho del rat�n, interpolando el Field of View (FOV)
/// y ajustando la posici�n local de la c�mara.
/// La posici�n de "descanso" o normal de la c�mara se deriva autom�ticamente
/// de su posici�n inicial en la jerarqu�a, sin necesidad de configuraci�n manual.
/// </summary>
public class CameraZoom : MonoBehaviour
{
    [Header("Configuraci�n de Zoom")]
    [Tooltip("El Field of View (FOV) normal de la c�mara cuando no hay zoom.")]
    public float defaultFOV = 60f;

    [Tooltip("El Field of View (FOV) cuando el zoom est� activo (menor valor = m�s zoom).")]
    public float zoomedFOV = 20f;

    [Tooltip("Velocidad de interpolaci�n para el FOV, para un zoom suave.")]
    public float zoomSpeedFOV = 8f; // Velocidad de interpolaci�n FOV

    [Header("Ajuste de Posici�n")]
    [Tooltip("La posici�n local de la c�mara cuando el zoom est� activo (ajusta para alinear con la mira).")]
    public Vector3 zoomedLocalPosition = new Vector3(0.5f, -0.2f, 0.5f); // Ejemplo: ajusta X, Y, Z para alinear la mira

    [Tooltip("Velocidad de interpolaci�n para la posici�n local, para un movimiento suave de la c�mara.")]
    public float zoomSpeedPosition = 8f; // Velocidad de interpolaci�n de posici�n

    private Camera mainCamera; // Referencia al componente Camera
    private Vector3 initialRestLocalPosition; // La posici�n local de la c�mara cuando no hay zoom (se guarda al inicio)

    /// <summary>
    /// Awake se llama cuando se carga la instancia del script.
    /// Obtiene la referencia al componente Camera y guarda la posici�n local inicial.
    /// </summary>
    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraZoom: No se encontr� un componente Camera en este GameObject. Deshabilitando script.", this);
            enabled = false;
            return; // Salir si no hay c�mara
        }

        // Guardar la posici�n local inicial de la c�mara.
        // Esta ser� la posici�n a la que regresar� la c�mara cuando no est� en zoom.
        initialRestLocalPosition = transform.localPosition;
    }

    /// <summary>
    /// Update se llama una vez por frame.
    /// Gestiona la entrada del usuario y la interpolaci�n del zoom y la posici�n.
    /// </summary>
    void Update()
    {
        // Detecta si se mantiene presionado el bot�n derecho del rat�n
        bool isZooming = Input.GetMouseButton(1); // 1 es el c�digo para el bot�n derecho del rat�n

        // Interpolar el Field of View (FOV)
        float targetFOV = isZooming ? zoomedFOV : defaultFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeedFOV);

        // Interpolar la posici�n local de la c�mara
        // El objetivo es la posici�n de zoom si isZooming es true,
        // de lo contrario, es la posici�n inicial que se guard� en Awake.
        Vector3 targetLocalPosition = isZooming ? zoomedLocalPosition : initialRestLocalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * zoomSpeedPosition);
    }
}