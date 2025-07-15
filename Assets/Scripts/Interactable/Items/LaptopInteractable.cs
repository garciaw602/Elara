using UnityEngine;

public class LaptopInteractable : MonoBehaviour, IInteractable
{
    public GameObject laptopUI;
    public string itemName = "Access Laptop";

    public void Interact()
    {
        if (laptopUI != null)
        {
            laptopUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public string GetName()
    {
        return itemName;
    }
}
