using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;

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
    [SerializeField] private List<string> videoFileNames = new List<string>(); // .mp4 file names in StreamingAssets

    [Header("Audio")]
    public AudioSource mainMusic;
    public AudioSource narration1;
    public AudioSource narration2;
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
        Debug.Log($"Global Time: {globalTime:F2} s");
#endif
    }

    void PlayClip(int index)
    {
        if (index >= videoFileNames.Count)
        {
            SceneManager.LoadScene("Level1");
            return;
        }

        string fileName = videoFileNames[index];
        string videoPath = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_WEBGL && !UNITY_EDITOR
        // For WebGL, use relative path via UnityWebRequest
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
#else
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
#endif

        Debug.Log("Playing video from: " + videoPath);
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
    }

    void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        videoPlayer.Play();

        if (currentClipIndex == 1 && !audioStarted)
        {
            audioStarted = true;
            StartCoroutine(FadeIn(mainMusic, 2f, 0.5f));
            StartCoroutine(PlayNarrationWithFade());
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        totalElapsedTime += vp.length;
        currentClipIndex++;
        PlayClip(currentClipIndex);
    }

    IEnumerator PlayNarrationWithFade()
    {
        bool startedNarration1 = false;
        bool startedNarration2 = false;

        float narration1StartTime = 12f;
        float narration2StartTime = 68f;

        while (!startedNarration1 || !startedNarration2)
        {
            double globalTime = totalElapsedTime + videoPlayer.time;

            if (!startedNarration1 && globalTime >= narration1StartTime)
            {
                startedNarration1 = true;
                StartCoroutine(PlayAndFadeNarration(narration1));
            }

            if (!startedNarration2 && globalTime >= narration2StartTime)
            {
                startedNarration2 = true;
                StartCoroutine(PlayAndFadeNarration(narration2));
            }

            yield return null;
        }
    }

    IEnumerator PlayAndFadeNarration(AudioSource narration)
    {
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
            new SubtitleLine { startTime = 14f, endTime = 21f, text = "In the year 2038, Elena led a secret biotech project ELARA." },
            new SubtitleLine { startTime = 22f, endTime = 29f, text = "The goal: rewrite human DNA with Artificial intelligence… and erase terminal illness." },
            new SubtitleLine { startTime = 30f, endTime = 41f, text = "But the experiment failed. The virus, ELARA-X, learned... mutated... and awakened memories in the dead." },
            new SubtitleLine { startTime = 42f, endTime = 53f, text = "Now, the undead walk, but they remember. They cry… whisper names… search for their homes." },
            new SubtitleLine { startTime = 54f, endTime = 57f, text = "Some even avoid hurting the ones they loved." },
            new SubtitleLine { startTime = 58f, endTime = 64f, text = "The line between life and death… human and monster… is gone." },
            new SubtitleLine { startTime = 71f, endTime = 77f, text = "My name is Gabriel. I was happy... before all this" },
            new SubtitleLine { startTime = 78f, endTime = 90f, text = "Ex-military, now just a mechanic. I had everything I needed. Elena, my wife... Luna, our daughter. My world." },
            new SubtitleLine { startTime = 91f, endTime = 99f, text = "I was outside the city that day… fixing an old antenna. You know... Just another job" },
            new SubtitleLine { startTime = 100f, endTime = 114f, text = "Then… it happened. They call it the Awakening. When I returned home, everything was gone. I found blood… and a note from my wife." },
            new SubtitleLine { startTime = 115f, endTime = 127f, text = "Don’t trust anyone. Don’t try to save me. Save the world if you can… or Luna, if you still have a soul." },
            new SubtitleLine { startTime = 128f, endTime = 131f, text = "I lost them… but I won't lose my soul" },
            new SubtitleLine { startTime = 131f, endTime = 136f, text = "If Luna’s still out there… I’ll find her" }
        };
    }
}
