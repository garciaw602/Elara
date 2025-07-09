using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   // public TextMeshProUGUI ammoText;

    public static GameManager Instance { get; private set; }

    public int gunAmmo = 10;


    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
       // ammoText.text = gunAmmo.ToString();

    }


}
