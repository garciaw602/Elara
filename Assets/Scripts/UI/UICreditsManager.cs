using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UICreditsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject creditsPanel; // The panel to show/hide
    public TMP_Text creditsText;    // The full credits text
    public float scrollSpeed = 30f; // Units per second

    [Header("Scroll Config")]
    public float scrollEndY = 1000f; // End position Y for the scroll
    private Vector2 startPosition;

    private Coroutine scrollingCoroutine;

    void Start()
    {
        creditsPanel.SetActive(false);
        startPosition = creditsText.rectTransform.anchoredPosition;
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
        creditsText.rectTransform.anchoredPosition = startPosition;

        if (scrollingCoroutine != null)
            StopCoroutine(scrollingCoroutine);

        scrollingCoroutine = StartCoroutine(ScrollCredits());
    }

    private IEnumerator ScrollCredits()
    {
        while (creditsText.rectTransform.anchoredPosition.y < scrollEndY)
        {
            if (
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetMouseButtonDown(0)
            )
                break;

            creditsText.rectTransform.anchoredPosition += Vector2.up * (scrollSpeed * Time.deltaTime);
            yield return null;
        }

        creditsPanel.SetActive(false);
    }

    public void HideCredits() // Optional: external button to skip
    {
        if (scrollingCoroutine != null)
            StopCoroutine(scrollingCoroutine);

        creditsPanel.SetActive(false);
    }
}
