using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class SubtitleLine
{
    public float startTime;
    public float endTime;
    [TextArea] public string text;
}

public class StoryPlayer : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public VideoClip[] clips;

    [Header("Audio")]
    public AudioSource mainMusic;
    public AudioSource narration;

    [Header("Subtitles")]
    public TextMeshProUGUI subtitleText;
    public List<SubtitleLine> globalSubtitles = new List<SubtitleLine>();

    private int currentClipIndex = 0;
    private bool audioStarted = false;
    private double totalElapsedTime = 0;

    void Start()
    {
        SetupSubtitles();

        videoPlayer.loopPointReached += OnVideoFinished;
        PlayClip(currentClipIndex);
    }

    void Update()
    {
        

        if (!videoPlayer.isPlaying)
            return;

        double globalTime = totalElapsedTime + videoPlayer.time;

        string subtitle = "";

        foreach (var line in globalSubtitles)
        {
            if (globalTime >= line.startTime && globalTime <= line.endTime)
            {
                subtitle = line.text;
                break;
            }
        }

        subtitleText.text = subtitle;
        subtitleText.enabled = !string.IsNullOrEmpty(subtitle);

        #if UNITY_EDITOR
                globalTime = totalElapsedTime + videoPlayer.time;
                Debug.Log($"Global Time: {globalTime:F2} s");
        #endif
    }

    void PlayClip(int index)
    {
        if (index < clips.Length)
        {
            videoPlayer.clip = clips[index];
            videoPlayer.Play();

            if (index == 1 && !audioStarted)
            {
                audioStarted = true;
                StartCoroutine(FadeIn(mainMusic, 2f, 0.5f));
                StartCoroutine(PlayNarrationWithFade());
            }
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        totalElapsedTime += vp.clip.length; // ⏱ accumulate full video length
        currentClipIndex++;
        PlayClip(currentClipIndex);
    }

    IEnumerator PlayNarrationWithFade()
    {
        float delayBeforeNarration = 2.2f;
        yield return new WaitForSeconds(delayBeforeNarration);

        narration.volume = 0;
        narration.Play();
        yield return StartCoroutine(FadeIn(narration, 2f, 1f));
        yield return new WaitForSeconds(narration.clip.length - 2f);
        yield return StartCoroutine(FadeOut(narration, 2f));
        narration.Stop();
    }

    IEnumerator FadeIn(AudioSource audio, float duration, float targetVolume)
    {
        float time = 0f;
        audio.volume = 0f;
        audio.Play();

        while (time < duration)
        {
            audio.volume = Mathf.Lerp(0f, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audio.volume = targetVolume;
    }

    IEnumerator FadeOut(AudioSource audio, float duration)
    {
        float start = audio.volume;
        float time = 0f;

        while (time < duration)
        {
            audio.volume = Mathf.Lerp(start, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audio.volume = 0f;
    }

    void SetupSubtitles()
    {
        globalSubtitles = new List<SubtitleLine>
        {
            new SubtitleLine { startTime = 12f, endTime = 18f, text = "In the year 2038, Elena led a secret biotech project ELARA." },
            new SubtitleLine { startTime = 19f, endTime = 25f, text = "The goal: rewrite human DNA with Artificial intelligence… and erase terminal illness." },
            new SubtitleLine { startTime = 26f, endTime = 35f, text = "But the experiment failed. The virus, ELARA-X, learned... mutated... and awakened memories in the dead." },
            new SubtitleLine { startTime = 36f, endTime = 44f, text = "Now, the undead walk, but they remember. They cry… whisper names… search for their homes." },
            new SubtitleLine { startTime = 45f, endTime = 49f, text = "Some even avoid hurting the ones they loved." },
            new SubtitleLine { startTime = 50f, endTime = 59f, text = "The line between life and death… human and monster… is gone." }
        };
    }
}
