using UnityEngine;
using Pizzard.Core;

/// <summary>
/// Gestiona los contextos y todas las UI del juego.
/// Es un singleton persistente entre escenas (DontDestroyOnLoad).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("Panel del menú principal/pausa")]
    public MenuUI menuUI;
    [Tooltip("Panel de opciones")]
    public OptionsUI optionsUI;
    [Tooltip("Panel de la tienda")]
    public ShopUI tiendaUI;
    [Tooltip("Panel de diálogos")]
    public DialogUI dialogUI;
    [Tooltip("Panel de muerte")]
    public DeathUI deathUI;

    private UIContext lastContext = UIContext.None;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // --- Fix for UI overlap on startup ---
        // GameFlowManager.Start() may have already called ChangeState before 
        // UIManager.Instance was set (Awake order race), leaving all panels visible.
        // Force a re-apply of the current state's visibility.
        if (GameFlowManager.Instance != null)
        {
            ForceApplyUIForState(GameFlowManager.Instance.CurrentState);
        }
        else
        {
            // Fallback: hide everything except menu
            HideAllUIs();
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            if (menuUI != null) menuUI.Show();
        }
    }

    /// <summary>
    /// Forcefully applies the UI visibility for a given GameState.
    /// Called from Start() to fix the race condition, and can be called externally.
    /// </summary>
    public void ForceApplyUIForState(GameState state)
    {
        HideAllUIs();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        switch (state)
        {
            case GameState.MainMenu:
                if (menuUI != null) menuUI.Show();
                break;
            case GameState.Shop:
                if (tiendaUI != null) tiendaUI.Show();
                break;
            case GameState.Dialogue:
            case GameState.PreBossDialogue:
            case GameState.PostBossDialogue:
                if (dialogUI != null) dialogUI.gameObject.SetActive(true);
                break;
            case GameState.Combat:
                // Enable HUD elements
                Transform elementsUI = transform.Find("Elementos");
                Transform bossUI = transform.Find("PblobUI");
                Transform playerHP = transform.Find("HealthUI");
                Transform potionUI = transform.Find("PotionUI");
                Transform manaUI = transform.Find("ManaUI");
                
                if (elementsUI) elementsUI.gameObject.SetActive(true);
                if (bossUI) bossUI.gameObject.SetActive(true);
                if (playerHP) playerHP.gameObject.SetActive(true);
                if (potionUI) potionUI.gameObject.SetActive(true);
                if (manaUI) manaUI.gameObject.SetActive(true);
                break;
        }
        
        Debug.Log($"[UIManager] ForceApplyUIForState: {state}");
    }

    /// <summary>
    /// Abre el panel de opciones guardando el contexto actual.
    /// </summary>
    /// <param name="context">Contexto desde el que se abren las opciones.</param>
    public void OpenOptions(UIContext context)
    {
        lastContext = context;
        if (optionsUI != null)
            optionsUI.Show();
    }

    /// <summary>
    /// Cierra el panel de opciones y vuelve al contexto anterior.
    /// </summary>
    public void CloseOptions()
    {
        if (optionsUI != null)
            optionsUI.Hide();

        switch (lastContext)
        {
            case UIContext.Menu:
                if (menuUI != null)
                    menuUI.Show();
                break;
            case UIContext.Tienda:
                if (tiendaUI != null)
                    tiendaUI.Show();
                break;
            case UIContext.Dialogo:
                if (dialogUI != null)
                    dialogUI.Show();
                break;
            case UIContext.BossFight:
                // No mostrar ninguna UI, volvemos al combate
                break;
        }

        lastContext = UIContext.None;
    }

    /// <summary>
    /// Oculta todas las UIs gestionadas.
    /// </summary>
    public void HideAllUIs()
    {
        if (menuUI != null)
            menuUI.Hide();
        if (optionsUI != null)
            optionsUI.Hide();
        if (tiendaUI != null)
            tiendaUI.Hide();
        if (dialogUI != null)
            dialogUI.Hide();
        if (deathUI != null)
            deathUI.OcultarPantallaMuerte();
        
        // Hide child objects that aren't managed by these specific classes (e.g. HUD, Boss UI)
        // by disabling their specific scripts or canvases if we had references.
        // A safer trick for the prototype: we just specifically toggle the target UIs in GameFlowManager.
    }
}