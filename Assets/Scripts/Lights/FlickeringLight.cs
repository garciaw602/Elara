using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    private Light lightToFlicker;

    [SerializeField, Range(0f, 45f)] private float minIntensity = 0.0f;
    [SerializeField, Range(0f, 45f)] private float maxIntensity = 52f;
    [SerializeField, Min(0f)] private float timeBetweenIntensity = 0.05f;

    private float currentTimer;

    private void Awake()
    {
        lightToFlicker = GetComponent<Light>();
        ValidateIntensityBounds();
    }

    private void Update()
    {
        currentTimer += Time.deltaTime;

        if (currentTimer < timeBetweenIntensity) return;

        lightToFlicker.intensity = Random.Range(minIntensity, maxIntensity);
        currentTimer = 0f;
    }

    private void ValidateIntensityBounds()
    {
        if (minIntensity > maxIntensity)
        {
            Debug.LogWarning("Min Intensity is greater than Max Intensity. Swapping values!");
            (minIntensity, maxIntensity) = (maxIntensity, minIntensity);
        }
    }
}
