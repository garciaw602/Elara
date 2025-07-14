using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;
    public float updateInterval = 0.5f;

    private float timeSinceLastUpdate = 0f;

    void Update()
    {
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            float fps = 1f / Time.unscaledDeltaTime;
            fpsText.text = Mathf.RoundToInt(fps) + " FPS";
            timeSinceLastUpdate = 0f;
        }
    }
}
