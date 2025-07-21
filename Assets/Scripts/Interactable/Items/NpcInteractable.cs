using UnityEngine;
using TMPro;
using System.Collections;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public string[] dialogueLines;
    public AudioClip[] dialogueClips;
    public TextMeshProUGUI dialogueText;

    private AudioSource audioSource;
    private int currentLine = 0;
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false); // Hide initially
        }
    }

    public void Interact()
    {
        if (!isPlaying && currentLine < dialogueLines.Length && currentLine < dialogueClips.Length)
        {
            StartCoroutine(PlayDialogueLine());
        }
        else if (!isPlaying && currentLine >= dialogueLines.Length)
        {
            currentLine = 0; // Optional: restart from beginning
            StartCoroutine(PlayDialogueLine());
        }
    }

    private IEnumerator PlayDialogueLine()
    {
        isPlaying = true;

        while (currentLine < dialogueLines.Length && currentLine < dialogueClips.Length)
        {
            // Show current text
            dialogueText.text = dialogueLines[currentLine];
            dialogueText.gameObject.SetActive(true);

            // Play audio
            audioSource.clip = dialogueClips[currentLine];
            audioSource.Play();

            // Wait for audio to finish
            yield return new WaitForSeconds(audioSource.clip.length);

            // Hide text
            dialogueText.gameObject.SetActive(false);

            // Move to next
            currentLine++;

            // Optional small delay between lines
            yield return new WaitForSeconds(0.3f);
        }

        isPlaying = false;
        currentLine = 0; // Optional: reset if you want it to loop on next interaction
    }


    public string GetName()
    {
        return "Talk to Wilson";
    }
}
