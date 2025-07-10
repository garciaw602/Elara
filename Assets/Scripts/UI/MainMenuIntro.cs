using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuIntro : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup backgroundImage;
    public CanvasGroup studioLogoCG;
    public CanvasGroup gameTitle;
    public CanvasGroup mainMenuUI;

    [Header("Audio")]
    public AudioSource musicSource;
    public float musicFadeInDuration = 2f;
    public AudioClip logoSfx;
    public AudioClip titleSfx;

    [Header("Timing")]
    public float fadeDuration = 1f;
    public float studioDisplayTime = 1.5f;
    public float titleDisplayTime = 2f;

    void Start()
    {
        // Initial states
        SetAlpha(studioLogoCG, 0);
        SetAlpha(gameTitle, 0);
        SetAlpha(backgroundImage, 0);
        SetAlpha(mainMenuUI, 0);

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // Fade in music
        if (musicSource != null)
        {
            musicSource.volume = 0;
            musicSource.Play();
            StartCoroutine(FadeInAudio(musicSource, musicFadeInDuration));
        }

        // Wait a moment with black screen
        yield return new WaitForSeconds(1f);

        // Logo sound effect
        if (logoSfx != null)
            AudioSource.PlayClipAtPoint(logoSfx, Camera.main.transform.position);

        // Fade in/out studio logo
        yield return FadeCanvasGroup(studioLogoCG, 0, 1, fadeDuration);
        yield return new WaitForSeconds(studioDisplayTime);
        yield return FadeCanvasGroup(studioLogoCG, 1, 0, fadeDuration);

        // Title sound effect
        if (titleSfx != null)
            AudioSource.PlayClipAtPoint(titleSfx, Camera.main.transform.position);

        // Fade in/out game title
        yield return FadeCanvasGroup(gameTitle, 0, 1, fadeDuration);
        yield return new WaitForSeconds(titleDisplayTime);
        yield return FadeCanvasGroup(gameTitle, 1, 0, fadeDuration);

        // Fade in background image
        yield return FadeCanvasGroup(backgroundImage, 0, 1, fadeDuration);

        // Fade in main menu UI
        yield return FadeCanvasGroup(mainMenuUI, 0, 1, fadeDuration);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        cg.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cg.alpha = to;
        if (to == 0) cg.gameObject.SetActive(false);
    }

    IEnumerator FadeInAudio(AudioSource audioSource, float duration)
    {
        float elapsed = 0f;
        float startVol = 0f;
        float targetVol = 1f;

        while (elapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVol;
    }

    void SetAlpha(CanvasGroup cg, float value)
    {
        cg.alpha = value;
        cg.gameObject.SetActive(value > 0);
    }
}
