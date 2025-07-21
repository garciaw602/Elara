using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EscToAdvanceTMP : MonoBehaviour
{
    public TextMeshProUGUI messageTMP;
    public string nextSceneName = "NextScene";
    public float fadeDuration = 2f;

    private bool firstInputReceived = false;
    private bool isFading = false;

    void Start()
    {
        if (messageTMP != null)
        {
            var color = messageTMP.color;
            color.a = 0f;
            messageTMP.color = color;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) )
        {
            if (!firstInputReceived)
            {
                firstInputReceived = true;

                if (messageTMP != null)
                {
                    messageTMP.text = "Press any Esc or enter again to continue";
                    StopAllCoroutines(); // Avoid fade conflict
                    StartCoroutine(FadeTMP(messageTMP, 0f, 1f, 0.3f)); // Fade in
                    StartCoroutine(FadeOutTMP(messageTMP, fadeDuration, 1.5f)); // Fade out after delay
                }
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    IEnumerator FadeTMP(TextMeshProUGUI tmp, float from, float to, float duration)
    {
        float t = 0f;
        Color originalColor = tmp.color;
        while (t < duration)
        {
            float alpha = Mathf.Lerp(from, to, t / duration);
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, to);
    }

    IEnumerator FadeOutTMP(TextMeshProUGUI tmp, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return FadeTMP(tmp, 1f, 0f, duration);
    }
}
