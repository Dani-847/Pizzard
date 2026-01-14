using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la carga y el cambio de idioma desde archivos JSON en Resources/Languages.
/// Ejemplo de uso: LocalizationManager.Instance.GetText("menu_play");
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public event System.Action OnLanguageChanged;

    private Dictionary<string, string> currentDictionary = new Dictionary<string, string>();
    private string currentLanguage = "en";

    void Awake()
    {
        // Patrón singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar idioma guardado o inglés por defecto
        int idiomaIndex = PlayerPrefs.GetInt("Idioma", 0);
        currentLanguage = idiomaIndex == 0 ? "en" : "es";
        LoadLanguage(currentLanguage);
    }

    /// <summary>
    /// Carga el archivo JSON correspondiente al idioma.
    /// Los archivos deben estar en: Resources/Languages/en.json y es.json
    /// </summary>
    public void LoadLanguage(string langCode)
    {
        currentDictionary.Clear();

        TextAsset jsonFile = Resources.Load<TextAsset>($"Languages/{langCode}");
        if (jsonFile == null)
        {
            Debug.LogError($"❌ Archivo de idioma no encontrado: Resources/Languages/{langCode}.json");
            return;
        }

        // Parsear JSON
        LocalizationWrapper wrapper = JsonUtility.FromJson<LocalizationWrapper>(jsonFile.text);
        if (wrapper?.entries != null)
        {
            foreach (var entry in wrapper.entries)
                currentDictionary[entry.key] = entry.value;
        }

        Debug.Log($"🌍 Idioma cargado: {langCode} ({currentDictionary.Count} entradas)");
    }

    /// <summary>
    /// Devuelve el texto traducido de una clave.
    /// Si la clave no existe, devuelve [key].
    /// </summary>
    public string GetText(string key)
    {
        if (currentDictionary.TryGetValue(key, out string value))
            return value;
        return $"[{key}]";
    }

    /// <summary>
    /// Cambia el idioma entre inglés (0) y español (1).
    /// </summary>
    public void SetLanguage(int index)
    {
        currentLanguage = index == 0 ? "en" : "es";
        PlayerPrefs.SetInt("Idioma", index);
        PlayerPrefs.Save();

        LoadLanguage(currentLanguage);

        // Notificar a todos los LocalizedText
        OnLanguageChanged?.Invoke();
    }

    // -----------------------
    // Clases auxiliares
    // -----------------------
    [System.Serializable]
    private class LocalizationWrapper
    {
        public List<LocalizationEntry> entries;
    }

    [System.Serializable]
    private class LocalizationEntry
    {
        public string key;
        public string value;
    }
}