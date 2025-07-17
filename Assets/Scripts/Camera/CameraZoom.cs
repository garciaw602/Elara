using UnityEngine;

/// <summary>
/// CameraZoom: Gestiona el efecto de zoom de la cámara (simulando una mira)
/// al presionar el botón derecho del ratón, interpolando el Field of View (FOV)
/// y ajustando la posición local de la cámara.
/// La posición de "descanso" o normal de la cámara se deriva automáticamente
/// de su posición inicial en la jerarquía, sin necesidad de configuración manual.
/// </summary>
public class CameraZoom : MonoBehaviour
{
    [Header("Configuración de Zoom")]
    [Tooltip("El Field of View (FOV) normal de la cámara cuando no hay zoom.")]
    public float defaultFOV = 60f;

    [Tooltip("El Field of View (FOV) cuando el zoom está activo (menor valor = más zoom).")]
    public float zoomedFOV = 20f;

    [Tooltip("Velocidad de interpolación para el FOV, para un zoom suave.")]
    public float zoomSpeedFOV = 8f; // Velocidad de interpolación FOV

    [Header("Ajuste de Posición")]
    [Tooltip("La posición local de la cámara cuando el zoom está activo (ajusta para alinear con la mira).")]
    public Vector3 zoomedLocalPosition = new Vector3(0.5f, -0.2f, 0.5f); // Ejemplo: ajusta X, Y, Z para alinear la mira

    [Tooltip("Velocidad de interpolación para la posición local, para un movimiento suave de la cámara.")]
    public float zoomSpeedPosition = 8f; // Velocidad de interpolación de posición

    private Camera mainCamera; // Referencia al componente Camera
    private Vector3 initialRestLocalPosition; // La posición local de la cámara cuando no hay zoom (se guarda al inicio)

    /// <summary>
    /// Awake se llama cuando se carga la instancia del script.
    /// Obtiene la referencia al componente Camera y guarda la posición local inicial.
    /// </summary>
    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraZoom: No se encontró un componente Camera en este GameObject. Deshabilitando script.", this);
            enabled = false;
            return; // Salir si no hay cámara
        }

        // Guardar la posición local inicial de la cámara.
        // Esta será la posición a la que regresará la cámara cuando no esté en zoom.
        initialRestLocalPosition = transform.localPosition;
    }

    /// <summary>
    /// Update se llama una vez por frame.
    /// Gestiona la entrada del usuario y la interpolación del zoom y la posición.
    /// </summary>
    void Update()
    {
        // Detecta si se mantiene presionado el botón derecho del ratón
        bool isZooming = Input.GetMouseButton(1); // 1 es el código para el botón derecho del ratón

        // Interpolar el Field of View (FOV)
        float targetFOV = isZooming ? zoomedFOV : defaultFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeedFOV);

        // Interpolar la posición local de la cámara
        // El objetivo es la posición de zoom si isZooming es true,
        // de lo contrario, es la posición inicial que se guardó en Awake.
        Vector3 targetLocalPosition = isZooming ? zoomedLocalPosition : initialRestLocalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * zoomSpeedPosition);
    }
}