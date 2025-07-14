using UnityEngine;

public class MapInteractable : MonoBehaviour, IInteractable
{
    public GameObject mapUI;
    public string itemName = "Map";

    public void Interact()
    {
        mapUI.SetActive(true);
    }

    public string GetName()
    {
        return "View Map";
    }

    public void HideMap()
    {
        if (mapUI.activeSelf)
            mapUI.SetActive(false);
    }
}
