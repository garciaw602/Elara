using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    
    public string itemName = "talk";

    public void Interact()
    {
        
    }

    public string GetName()
    {
        return "View Npc";
    }


}
