using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Componente que actualiza automáticamente el texto según el idioma activo.
/// Se actualiza también cuando el idioma cambia.
/// </summary>
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;
    private TMP_Text tmpText;
    private Text uiText;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        uiText = GetComponent<Text>();

        // Arrancamos una corrutina para esperar a que LocalizationManager esté listo
        StartCoroutine(WaitForLocalizationManager());
    }

    private IEnumerator WaitForLocalizationManager()
    {
        // Espera hasta que LocalizationManager.Instance exista
        while (LocalizationManager.Instance == null)
            yield return null;

        // Nos suscribimos al evento
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;

        // Forzamos la primera actualización
        UpdateText();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText()
    {
        if (LocalizationManager.Instance == null)
            return;

        string localized = LocalizationManager.Instance.GetText(key);

        if (tmpText != null)
            tmpText.text = localized;
        else if (uiText != null)
            uiText.text = localized;
    }
}