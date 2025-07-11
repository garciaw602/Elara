using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string sceneName = "Level2"; // Change to your target scene name
    public KeyCode triggerKey = KeyCode.I; // Press 'I' to change scene

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
