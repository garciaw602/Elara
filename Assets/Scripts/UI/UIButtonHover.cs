using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage; // Image component of the button
    public Sprite defaultSprite;
    public Sprite hoverSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null && hoverSprite != null)
            targetImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null && defaultSprite != null)
            targetImage.sprite = defaultSprite;
    }

    void Start()
    {
        // Ensure default is set at start
        if (targetImage != null && defaultSprite != null)
            targetImage.sprite = defaultSprite;
    }
}
