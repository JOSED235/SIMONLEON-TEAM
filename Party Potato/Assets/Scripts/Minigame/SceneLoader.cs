using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Escena Principal")]
    public string defaultGameScene = "SampleScene";

    // Carga la escena configurada por defecto
    public void LoadGameScene()
    {
        SceneManager.LoadScene(defaultGameScene);
    }

    // Carga una escena específica por su nombre
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Carga una escena específica por su número (Build Index)
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Recarga la escena actual (Útil para un botón de "Reiniciar Nivel")
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Carga la siguiente escena en la lista de Build Settings
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // Sale del juego
    public void QuitGame()
    {
        Debug.Log("Quit Game Requested");
        Application.Quit();
    }
}
