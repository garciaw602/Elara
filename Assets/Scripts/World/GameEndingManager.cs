using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndingManager : MonoBehaviour
{
    // Define las posibles rutas de final
    public enum EndingChoice { None, GoodEnding, BadEnding, NeutralEnding }

    // Variable para almacenar la elecci�n final del jugador
    public EndingChoice currentEndingChoice = EndingChoice.None;

    // GameObjects para cinem�ticas
    // public GameObject goodEndingCinematic;
    // public GameObject badEndingCinematic;

    // Referencia al nombre de la escena de men� principal
    public string mainMenuSceneName = "MainMenu";

    // M�todo que ser� llamado cuando el jugador tome una decisi�n
    public void MakeEndingChoice(EndingChoice choice)
    {
        currentEndingChoice = choice;
        Debug.Log("Elecci�n de final realizada: " + currentEndingChoice);

        // Activar la l�gica de la cinem�tica
        // Por ahora para cargar el men� principal.
        // En el futuro tener una coroutine que:
        // 1. Active la cinem�tica correspondiente.
        // 2. Espere a que la cinem�tica termine.
        // 3. Luego llame a LoadMainMenu() o los creditos.
        // StartCoroutine(PlayEndingAndLoadMenu(choice));

        // Para prop�sito de prueba, cargamos el men� principal directamente
        LoadMainMenu();
    }

    // M�todo para cargar el men� principal
    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        // Desbloquea y muestra el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Carga la escena del men� principal
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Aqu� se puede a�adir una funci�n que se ejecute al final de una cinem�tica como los creditos
    // public void OnCinematicFinished()
    // {
    //     LoadMainMenu();
    // }
}
