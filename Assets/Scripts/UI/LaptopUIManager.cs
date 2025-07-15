using UnityEngine;

public class LaptopUIManager : MonoBehaviour
{
    public GameObject panel;

    public void OnOption1() // Unlock Door
    {
        Debug.Log("Option 1: Unlocking Door...");
        // Example: Find door and open it
    }

    public void OnOption2() // Give Ammo
    {
        Debug.Log("Option 2: Giving ammo...");
        GameManager.Instance.AddAmmo(30);
    }

    public void OnOption3() // Play video or trigger scene
    {
        Debug.Log("Option 3: Playing video...");
        // Your logic here
    }

    public void CloseLaptopUI()
    {
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLaptopUI();
        }
    }
}
