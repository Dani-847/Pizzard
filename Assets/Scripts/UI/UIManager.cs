using UnityEngine;

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
    }
}