using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndingManager : MonoBehaviour
{
    // Define las posibles rutas de final
    public enum EndingChoice { None, GoodEnding, BadEnding, NeutralEnding }

    // Variable para almacenar la elección final del jugador
    public EndingChoice currentEndingChoice = EndingChoice.None;

    // GameObjects para cinemáticas
    // public GameObject goodEndingCinematic;
    // public GameObject badEndingCinematic;

    // Referencia al nombre de la escena de menú principal
    public string mainMenuSceneName = "MainMenu";

    // Método que será llamado cuando el jugador tome una decisión
    public void MakeEndingChoice(EndingChoice choice)
    {
        currentEndingChoice = choice;
        Debug.Log("Elección de final realizada: " + currentEndingChoice);

        // Activar la lógica de la cinemática
        // Por ahora para cargar el menú principal.
        // En el futuro tener una coroutine que:
        // 1. Active la cinemática correspondiente.
        // 2. Espere a que la cinemática termine.
        // 3. Luego llame a LoadMainMenu() o los creditos.
        // StartCoroutine(PlayEndingAndLoadMenu(choice));

        // Para propósito de prueba, cargamos el menú principal directamente
        LoadMainMenu();
    }

    // Método para cargar el menú principal
    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        // Desbloquea y muestra el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Carga la escena del menú principal
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Aquí se puede añadir una función que se ejecute al final de una cinemática como los creditos
    // public void OnCinematicFinished()
    // {
    //     LoadMainMenu();
    // }
}
