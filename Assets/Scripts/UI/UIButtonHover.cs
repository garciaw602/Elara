using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text & Style")]
    public TMP_Text buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.85f, 1f, 0.85f); // subtle green tint
    public float normalFontSize = 36f;
    public float hoverFontSize = 42f;

    [Header("Audio")]
    public AudioSource audioSource;        // Main AudioSource (e.g., on SoundManager)
    public AudioClip hoverClip;            // The hover sound effect

    void Start()
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
            buttonText.fontSize = normalFontSize;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover works!");

        if (buttonText != null)
        {
            buttonText.color = hoverColor;
            buttonText.fontSize = hoverFontSize;
        }

        if (audioSource != null && hoverClip != null)
        {
            audioSource.PlayOneShot(hoverClip);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
            buttonText.fontSize = normalFontSize;
        }
    }
}
