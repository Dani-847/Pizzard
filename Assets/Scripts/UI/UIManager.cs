using UnityEngine;

/// <summary>
/// Gestiona los contextos y todas las UI del juego.
/// Es un singleton persistente entre escenas (DontDestroyOnLoad).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias UI - Flujo")]
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
    
    [Header("Referencias UI - Gameplay")]
    [Tooltip("UI del boss (barra de vida)")]
    public PblobUI pblobUI;
    [Tooltip("UI de pociones")]
    public PotionUI potionUI;
    [Tooltip("UI de combinaciones")]
    public CombinationsUI combinationsUI;

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
        
        // Ocultar todas las UIs al inicio para que GameFlowManager las controle
        HideAllUIsOnAwake();
    }
    
    /// <summary>
    /// Oculta todas las UIs durante Awake para evitar que aparezcan antes de que GameFlowManager inicie.
    /// </summary>
    private void HideAllUIsOnAwake()
    {
        // UIs de flujo - ocultar sus GameObjects o paneles
        if (dialogUI != null)
        {
            if (dialogUI.dialogPanel != null)
                dialogUI.dialogPanel.SetActive(false);
        }
        
        if (tiendaUI != null)
            tiendaUI.gameObject.SetActive(false);
            
        if (optionsUI != null)
            optionsUI.gameObject.SetActive(false);
            
        if (deathUI != null && deathUI.pantallaMuerte != null)
            deathUI.pantallaMuerte.SetActive(false);
            
        // UIs de gameplay - ocultar hasta que inicie el combate
        if (pblobUI != null)
            pblobUI.gameObject.SetActive(false);
            
        if (potionUI != null)
            potionUI.gameObject.SetActive(false);
            
        // Combinaciones - asegurar que el panel está oculto
        if (combinationsUI != null && combinationsUI.panelCombinations != null)
            combinationsUI.panelCombinations.SetActive(false);
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
        if (combinationsUI != null)
            combinationsUI.OcultarPanel();
    }
    
    /// <summary>
    /// Muestra las UIs de gameplay (durante BossFight).
    /// </summary>
    public void ShowGameplayUIs()
    {
        if (pblobUI != null)
            pblobUI.gameObject.SetActive(true);
        if (potionUI != null)
            potionUI.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Oculta las UIs de gameplay (fuera de BossFight).
    /// </summary>
    public void HideGameplayUIs()
    {
        if (pblobUI != null)
            pblobUI.gameObject.SetActive(false);
        if (potionUI != null)
            potionUI.gameObject.SetActive(false);
    }
}