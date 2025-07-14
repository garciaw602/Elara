using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonTest : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("✅ Button clicked: " + gameObject.name);
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("🎯 Hover on: " + gameObject.name);
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
}
