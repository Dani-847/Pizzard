using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pizzard.Core;

/// <summary>
/// Pantalla de victoria que se muestra al derrotar al último boss.
/// Construye su UI programáticamente si no tiene referencias asignadas.
/// </summary>
public class VictoryUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] public GameObject panelVictoria;
    [SerializeField] public Image darkOverlay;
    [SerializeField] public Image victoryImage;
    [SerializeField] public TextMeshProUGUI victoryText;
    [SerializeField] public Button botonMenu;
    [SerializeField] public Button botonSalir;

    private bool _estaActiva = false;
    private bool _uiBuilt = false;

    private void Awake()
    {
        // Auto-bind children if missing
        if (panelVictoria == null)
        {
            Transform dp = transform.Find("DialogPanel");
            if (dp != null) panelVictoria = dp.gameObject;
        }

        if (victoryText == null)
        {
            Transform txt = transform.Find("DialogText");
            if (txt != null) victoryText = txt.GetComponent<TextMeshProUGUI>();
        }

        if (victoryImage == null)
        {
            Transform logo = transform.Find("Logo");
            if (logo != null) victoryImage = logo.GetComponent<Image>();
        }

        if (botonMenu == null)
        {
            Transform btn = transform.Find("ButtonNext");
            if (btn == null) btn = transform.Find("ButtonMenu");
            
            if (btn != null)
            {
                btn.gameObject.name = "ButtonMenu";
                botonMenu = btn.GetComponent<Button>();
                
                // Set text
                var tm = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (tm != null) tm.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("victory_menu") : "Menu";
                
                // Shift left
                var rt = btn.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(-200, rt.anchoredPosition.y);
            }
        }

        if (botonSalir == null && botonMenu != null)
        {
            Transform exitCheck = transform.Find("ButtonExit");
            if (exitCheck == null)
            {
                GameObject exitObj = Instantiate(botonMenu.gameObject, transform);
                exitObj.name = "ButtonExit";
                botonSalir = exitObj.GetComponent<Button>();

                // Set text
                var tm = exitObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tm != null) tm.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("menu_exit") : "Exit";

                // Shift right
                var rt = exitObj.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(200, rt.anchoredPosition.y);
            }
            else
            {
                botonSalir = exitCheck.GetComponent<Button>();
            }
        }

        // Apply specific formatting
        if (victoryText != null)
        {
            victoryText.text = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("victory_title")
                : "You Win!!!";
            victoryText.alignment = TextAlignmentOptions.Center;
        }

        if (victoryImage != null)
        {
            var logoRt = victoryImage.GetComponent<RectTransform>();
            if (logoRt != null)
            {
                logoRt.anchorMin = new Vector2(0.5f, 1f);
                logoRt.anchorMax = new Vector2(0.5f, 1f);
                logoRt.pivot = new Vector2(0.5f, 1f);
                logoRt.anchoredPosition = new Vector2(0, -50);
            }
        }

        if (panelVictoria != null)
        {
            // Optional: remove inherited animation components from copy
            var sync = panelVictoria.GetComponent<SyncSpriteToImage>();
            if (sync != null) Destroy(sync);
            var anim = panelVictoria.GetComponent<Animator>();
            if (anim != null) Destroy(anim);

            panelVictoria.SetActive(false);
        }

        WireButtons();
    }

    private void WireButtons()
    {
        if (botonMenu != null)
            botonMenu.onClick.AddListener(OnClickMenu);

        if (botonSalir != null)
            botonSalir.onClick.AddListener(OnClickSalir);
    }

    /// <summary>
    /// Muestra la pantalla de victoria y pausa el juego.
    /// </summary>
    public void Show()
    {
        if (_estaActiva) return;
        _estaActiva = true;

        gameObject.SetActive(true);

        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.text = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("victory_title")
                : "You Win!!!";
        }

        if (victoryImage != null)
            victoryImage.gameObject.SetActive(true);

        if (botonMenu != null)
        {
            botonMenu.gameObject.SetActive(true);
            var txtMenu = botonMenu.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txtMenu != null)
                txtMenu.text = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("victory_menu")
                    : "Menu";
        }

        if (botonSalir != null)
        {
            botonSalir.gameObject.SetActive(true);
            var txtExit = botonSalir.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txtExit != null)
                txtExit.text = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("menu_exit")
                    : "Exit";
        }

        Time.timeScale = 0f;
        Debug.Log("[VictoryUI] ¡Victoria! Pantalla mostrada.");
    }

    /// <summary>
    /// Oculta la pantalla de victoria.
    /// </summary>
    public void Hide()
    {
        _estaActiva = false;

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        Time.timeScale = 1f;
    }

    private void OnClickMenu()
    {
        Hide();

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.VolverAlMenu();
        }
        else
        {
            Debug.LogError("[VictoryUI] GameFlowManager.Instance no encontrado");
        }
    }

    private void OnClickSalir()
    {
        Debug.Log("[VictoryUI] Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
