using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    [Header("Volumen (pizza)")]
    public Button botonMasVolumen;
    public Button botonMenosVolumen;
    public Image pizzaVisual;
    public int maxPizzaSlices = 10;

    [Header("Idioma")]
    public Dropdown languageDropdown;

    [Header("Botones")]
    public Button acceptButton;
    public Button botonCombinations;
    public Button botonDebugMode;
    public TextMeshProUGUI debugModeLabel;

    [Header("Referencias externas")]
    public CombinationsUI combinationsUIPanel;

    private int slicesActuales;

    void Start()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("❌ SoundManager.Instance es NULL en OptionsUI.Start()");
            return;
        }

        // ----------------------------
        // VOLUMEN
        // ----------------------------
        slicesActuales = Mathf.RoundToInt(SoundManager.Instance.GetMusicVolume() * maxPizzaSlices);
        ActualizarPizzaVisual();

        botonMasVolumen.onClick.AddListener(OnClickMasVolumen);
        botonMenosVolumen.onClick.AddListener(OnClickMenosVolumen);
        botonCombinations.onClick.AddListener(OnClickCombinations);
        acceptButton.onClick.AddListener(GuardarYAceptar);

        if (botonDebugMode != null)
        {
            botonDebugMode.onClick.AddListener(OnClickDebugMode);
            ActualizarDebugLabel();
        }

        // ----------------------------
        // IDIOMA
        // ----------------------------
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "Español" });

            // Cargar selección guardada
            int saved = PlayerPrefs.GetInt("Idioma", 0);
            languageDropdown.value = saved;

            // Listener
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (combinationsUIPanel != null)
        {
            combinationsUIPanel.OcultarPanel();
        }
    }

    // ----------------------------
    // IDIOMA CAMBIADO
    // ----------------------------
    private void OnLanguageChanged(int index)
    {
        Debug.Log("🌍 Cambiando idioma a " + (index == 0 ? "inglés" : "español"));

        LocalizationManager.Instance.SetLanguage(index);

        // Actualizar textos visibles de este panel
        LocalizedText[] texts = GetComponentsInChildren<LocalizedText>(true);
        foreach (var t in texts)
            t.SendMessage("UpdateText", SendMessageOptions.DontRequireReceiver);
    }

    // ----------------------------
    // PANEL
    // ----------------------------
    public void Show()
    {
        gameObject.SetActive(true);

        if (SoundManager.Instance == null)
        {
            Debug.LogError("[OptionsUI] SoundManager.Instance is NULL in Show() — skipping volume sync");
            return;
        }

        slicesActuales = Mathf.RoundToInt(SoundManager.Instance.GetMusicVolume() * maxPizzaSlices);
        ActualizarPizzaVisual();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ----------------------------
    // VOLUMEN
    // ----------------------------
    public void OnClickMasVolumen()
    {
        slicesActuales = Mathf.Clamp(slicesActuales + 1, 0, maxPizzaSlices);

        float nuevoVolumen = (float)slicesActuales / maxPizzaSlices;
        SoundManager.Instance.SetMusicVolume(nuevoVolumen);

        ActualizarPizzaVisual();
    }

    public void OnClickMenosVolumen()
    {
        slicesActuales = Mathf.Clamp(slicesActuales - 1, 0, maxPizzaSlices);

        float nuevoVolumen = (float)slicesActuales / maxPizzaSlices;
        SoundManager.Instance.SetMusicVolume(nuevoVolumen);

        ActualizarPizzaVisual();
    }

    private void ActualizarPizzaVisual()
    {
        if (pizzaVisual != null)
            pizzaVisual.fillAmount = (float)slicesActuales / maxPizzaSlices;
    }

    // ----------------------------
    // BOTÓN COMBINACIONES
    // ----------------------------
    public void OnClickCombinations()
    {
        if (combinationsUIPanel != null)
        {
            combinationsUIPanel.MostrarPanel();
            Hide(); // Hide OptionsUI to prevent overlap
        }
    }

    // ----------------------------
    // DEBUG MODE TOGGLE
    // ----------------------------
    public void OnClickDebugMode()
    {
        bool current = SaveSystem.GetDebugMode();
        SaveSystem.SetDebugMode(!current);
        ActualizarDebugLabel();
    }

    private void ActualizarDebugLabel()
    {
        if (debugModeLabel == null) return;
        debugModeLabel.text = SaveSystem.GetDebugMode() ? "Debug: ON" : "Debug: OFF";
    }

    private void GuardarYAceptar()
    {
        Hide();
        UIManager.Instance?.CloseOptions();
    }
}