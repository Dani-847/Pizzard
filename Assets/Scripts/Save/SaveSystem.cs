using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    private const string KEY_LANGUAGE = "Idioma";
    private const string KEY_LAST_SCENE = "LastScene";

    // Idioma
    public static void SetLanguage(int index)
    {
        PlayerPrefs.SetInt(KEY_LANGUAGE, index);
        PlayerPrefs.Save();
    }

    public static int GetLanguage()
    {
        return PlayerPrefs.GetInt(KEY_LANGUAGE, 0);
    }

    // ❌ ELIMINAR métodos de volumen - ahora lo maneja SoundManager

    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public static void SaveLastScene()
    {
        PlayerPrefs.SetInt(KEY_LAST_SCENE, SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
    }

    public static int LoadLastSceneIndex()
    {
        return PlayerPrefs.GetInt(KEY_LAST_SCENE, 0);
    }

    public static void SaveOptions()
    {
        PlayerPrefs.Save(); // SoundManager ya guarda automáticamente
    }
}