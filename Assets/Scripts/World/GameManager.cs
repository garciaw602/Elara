using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int gunAmmo = 10;
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (ammoText != null)
            ammoText.text = gunAmmo.ToString();
    }

    public void AddAmmo(int amount)
    {
        gunAmmo += amount;
    }
}


