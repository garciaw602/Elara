using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    public string sceneToLoad = "NextScene"; // Set this in the Inspector
    public string itemName = "Enter";

    public void Interact()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public string GetName()
    {
        return itemName;
    }
}
