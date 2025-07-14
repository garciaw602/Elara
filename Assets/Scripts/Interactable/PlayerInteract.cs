using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public TextMeshProUGUI interactText;

    private LineRenderer lineRenderer;
    private Camera cam;

    private IInteractable currentTarget;
    private GameObject lastMapObject;

    private void Start()
    {
        cam = GetComponent<Camera>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        if (interactText != null)
            interactText.enabled = false;
    }

    private void Update()
    {
        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;
        Vector3 end = origin + direction * interactRange;

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, end);

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, interactRange, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;

                // Show floating text
                if (interactText != null)
                {
                    interactText.text = $"Press E to {interactable.GetName()}";
                    interactText.enabled = true;
                }

                // Interact
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();

                    // Track if this is a map to auto-hide later
                    if (interactable is MapInteractable)
                        lastMapObject = hit.collider.gameObject;
                }

                return;
            }
        }

        // No hit: hide UI, text, and map if previously shown
        if (interactText != null)
            interactText.enabled = false;

        currentTarget = null;

        if (lastMapObject != null)
        {
            var map = lastMapObject.GetComponent<MapInteractable>();
            if (map != null)
                map.HideMap();

            lastMapObject = null;
        }
    }
}
