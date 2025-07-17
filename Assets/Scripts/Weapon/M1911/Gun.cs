using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public Transform casingEjectionPoint;
    public Transform hammerTransform;           // Transform del Hammer que se moverá. Asignar en el Inspector.
    public float shotForce = 1500f;
    public float casingEjectionForce = 100f;
    public float hammerRotationAngleX = -20f;  // Ángulo de rotación en el eje X local (negativo para inclinación hacia adelante). Ajustar.
    public float hammerRotationSpeed = 10f;    // Velocidad de rotación del Hammer. Ajustar.
    public float hammerReturnSpeed = 5f;       // Velocidad de retorno del Hammer. Ajustar.
    public float shotRate = 0.5f;
    private float shotRateTime = 0;
    public AudioSource shootSound;
    public ParticleSystem muzzleFlash;
    public Light muzzleFlashLight;
    public float lightDuration = 0.1f;
    private float lightTimer = 0f;
    private bool isLightActive = false;
    private Quaternion initialHammerRotation; // Rotación inicial del Hammer.
    private bool isHammerRotating = false;    // Indica si el Hammer está rotando.
    private float hammerRotationTimer = 0f;   // Temporizador para la rotación del Hammer.
    public GameUIManager uiManager; 

    void Start()
    {
        shootSound = GetComponent<AudioSource>();
        if (shootSound == null) Debug.LogError("No AudioSource found on the weapon!");
        shotRateTime = Time.time;
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
        if (hammerTransform != null) initialHammerRotation = hammerTransform.localRotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && uiManager != null && uiManager.isCanvasOpen!=true)
        {
            if (Time.time > shotRateTime && GameManager.Instance.gunAmmo > 0) 
            {
                GameManager.Instance.gunAmmo--;
                GameObject newBullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
                Rigidbody bulletRb = newBullet.GetComponent<Rigidbody>();
                if (bulletRb != null) bulletRb.AddForce(spawnPoint.forward * shotForce);
                else Debug.LogError("El prefab del proyectil no tiene un componente Rigidbody.");

                if (casingPrefab != null && casingEjectionPoint != null)
                {
                    GameObject newCasing = Instantiate(casingPrefab, casingEjectionPoint.position, casingEjectionPoint.rotation);
                    Rigidbody casingRb = newCasing.GetComponent<Rigidbody>();
                    if (casingRb != null)
                    {
                        Vector3 ejectionDirection = casingEjectionPoint.right + casingEjectionPoint.up * 0.5f;
                        casingRb.AddForce(ejectionDirection * casingEjectionForce);
                        casingRb.angularVelocity = Random.insideUnitSphere * 5f;
                        Destroy(newCasing, 2f);
                    }
                    else Debug.LogError("El prefab del casquillo no tiene un componente Rigidbody.");
                }
                else
                {
                    if (casingPrefab == null) Debug.LogError("El prefab del casquillo no está asignado.");
                    if (casingEjectionPoint == null) Debug.LogError("El punto de eyección del casquillo no está asignado.");
                }

                if (hammerTransform != null)
                {
                    isHammerRotating = true;
                    hammerRotationTimer = 0f;
                }

                shotRateTime = Time.time + shotRate;
                if (muzzleFlash != null) muzzleFlash.Play();
                if (muzzleFlashLight != null)
                {
                    muzzleFlashLight.enabled = true;
                    isLightActive = true;
                    lightTimer = 0f;
                }
                Destroy(newBullet, 5);
                if (shootSound != null) shootSound.Play();
            }
        }

        if (isLightActive)
        {
            lightTimer += Time.deltaTime;
            if (lightTimer >= lightDuration)
            {
                muzzleFlashLight.enabled = false;
                isLightActive = false;
                lightTimer = 0f;
            }
        }

        // --- Rotación del Hammer en el eje local X ---
        if (isHammerRotating && hammerTransform != null)
        {
            hammerRotationTimer += Time.deltaTime;
            float rotationProgress = Mathf.Clamp01(hammerRotationTimer * hammerRotationSpeed);

            // Rotación en el eje local X
            Quaternion targetRotation = initialHammerRotation * Quaternion.Euler(hammerRotationAngleX * rotationProgress, 0f, 0f);
            hammerTransform.localRotation = targetRotation;

            // Retorno de la Rotación
            if (rotationProgress >= 1f)
            {
                float returnProgress = Mathf.Clamp01((hammerRotationTimer - (1f / hammerRotationSpeed)) * hammerReturnSpeed);
                hammerTransform.localRotation = Quaternion.Slerp(targetRotation, initialHammerRotation, returnProgress);
                if (returnProgress >= 1f)
                {
                    isHammerRotating = false;
                }
            }
        }
    }
}