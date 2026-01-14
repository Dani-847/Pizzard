using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Clase Singleton para usarlo entre escenas
public class SceneLoader : MonoBehaviour
{
    // Singleton
    public static SceneLoader Instance { get; private set; }

    // Se instancia y persiste entre escenas
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadScene(int buildIndex)
    {
        SaveSystem.SaveLastScene();
        SceneManager.LoadScene(buildIndex);
    }

    // Carga una escena por nombre (directa o con transición).
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneLoader] Cargando escena: {sceneName}");
        SaveSystem.SaveLastScene();
        SceneManager.LoadScene(sceneName);
    }

    // Carga asíncronamente una escena con transición opcional.
    public void LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[SceneLoader] Cargando escena async: {sceneName}");
        SaveSystem.SaveLastScene();
        StartCoroutine(LoadSceneAsyncRoutine(sceneName));
    }

    // Iniciará de forma asíncrona con animaciónd de carga
    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;
    }

    // Recarga la escena actual.
    public void ReloadCurrentScene()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index);
    }

    // Sale del juego (en editor o build).
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}