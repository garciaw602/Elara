using UnityEngine;

public class FoodItem : MonoBehaviour, IInteractable
{
    public string itemName = "Food";

    public void Interact()
    {
        Debug.Log("Picked up " + itemName);
        Destroy(gameObject); // or disable
    }

    public string GetName()
    {
        return itemName;
    }
}
