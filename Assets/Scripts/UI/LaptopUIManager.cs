using UnityEngine;
using UnityEngine.SceneManagement; 

public class LaptopUIManager : MonoBehaviour
{
    public GameObject panel;

    public void OnOption1() 
    {
        Debug.Log("Option 1: Unlocking Door...");
        SceneManager.LoadScene("MainMenu");

    }

    public void OnOption2() 
    {
        Debug.Log("Option 2: Giving ammo...");

        SceneManager.LoadScene("MainMenu");
    }

    public void OnOption3() 
    {
        Debug.Log("Option 3: Playing video...");

        SceneManager.LoadScene("MainMenu");
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
