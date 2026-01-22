using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestionar botones principales del menú (Jugar, Ajustes, Salir),
/// el menú de pausa y lanzar el flujo jugable hacia el primer diálogo.
/// </summary>
public class MenuUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Inicia el bucle jugable -> primer diálogo")]
    public Button botonJugar;
    [Tooltip("Abre OptionsUI")]
    public Button botonAjustes;
    [Tooltip("Cierra la aplicación")]
    public Button botonSalir;
    [Tooltip("Botón para reanudar el juego (solo visible en pausa)")]
    public Button botonReanudar;
    [Tooltip("Botón para volver al menú principal desde la pausa")]
    public Button botonVolverMenu;

    [Header("Referencias externas")]
    [Tooltip("Panel de ajustes (activar/desactivar)")]
    public GameObject optionsUIPanel;

    [Header("Estado")]
    private bool isPauseMenu = false;
    private GameFlowManager flowManager;

    void Awake()
    {
        // El MenuUI se activará por GameFlowManager en Start
        // No ocultamos aquí porque MenuUI debería ser el único visible al inicio
    }

    void Start()
    {
        if (botonJugar != null)
            botonJugar.onClick.AddListener(OnClickJugar);
        if (botonAjustes != null)
            botonAjustes.onClick.AddListener(OnClickAjustes);
        if (botonSalir != null)
            botonSalir.onClick.AddListener(OnClickSalir);
        if (botonReanudar != null)
            botonReanudar.onClick.AddListener(OnClickReanudar);
        if (botonVolverMenu != null)
            botonVolverMenu.onClick.AddListener(OnClickVolverMenu);
        
        // Ocultar botones de pausa inicialmente
        SetPauseButtonsVisible(false);
        
        // Mostrar botones del menú principal
        SetMainMenuButtonsVisible(true);
    }

    /// <summary>
    /// Iniciar flujo jugable: cierra menú principal e inicia el flujo del juego.
    /// </summary>
    public void OnClickJugar()
    {
        Debug.Log("Iniciando flujo jugable");
        Hide();

        // Usar GameFlowManager para iniciar el juego
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.IniciarJuego();
        }
        else
        {
            Debug.LogError("[MenuUI] GameFlowManager.Instance no encontrado");
        }
    }

    /// <summary>
    /// Mostrar optionsUIPanel (setActive true).
    /// </summary>
    public void OnClickAjustes()
    {
        Debug.Log("Abriendo el panel de ajustes");
        Hide();
        UIManager.Instance.OpenOptions(UIContext.Menu);
    }

    /// <summary>
    /// Application.Quit() y/o mostrar confirmación en editor.
    /// </summary>
    public void OnClickSalir()
    {
        Debug.Log("Saliendo del juego...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Reanuda el juego desde la pausa.
    /// </summary>
    public void OnClickReanudar()
    {
        Debug.Log("Reanudando juego desde pausa");
        Hide();
        flowManager?.ReanudarJuego();
    }

    /// <summary>
    /// Vuelve al menú principal desde la pausa.
    /// </summary>
    public void OnClickVolverMenu()
    {
        Debug.Log("Volviendo al menú principal");
        Hide();
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.VolverAlMenu();
        }
    }

    /// <summary>
    /// Muestra el menú principal.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        isPauseMenu = false;
        SetPauseButtonsVisible(false);
        SetMainMenuButtonsVisible(true);
    }

    /// <summary>
    /// Oculta el menú.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Muestra el menú en modo pausa.
    /// </summary>
    /// <param name="manager">GameFlowManager que gestiona el flujo.</param>
    public void ShowPauseMenu(GameFlowManager manager)
    {
        flowManager = manager;
        isPauseMenu = true;
        gameObject.SetActive(true);
        SetMainMenuButtonsVisible(false);
        SetPauseButtonsVisible(true);
    }

    /// <summary>
    /// Configura la visibilidad de los botones de menú principal.
    /// </summary>
    private void SetMainMenuButtonsVisible(bool visible)
    {
        if (botonJugar != null)
            botonJugar.gameObject.SetActive(visible);
        if (botonAjustes != null)
            botonAjustes.gameObject.SetActive(visible);
        if (botonSalir != null)
            botonSalir.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Configura la visibilidad de los botones de pausa.
    /// </summary>
    private void SetPauseButtonsVisible(bool visible)
    {
        if (botonReanudar != null)
            botonReanudar.gameObject.SetActive(visible);
        if (botonVolverMenu != null)
            botonVolverMenu.gameObject.SetActive(visible);
    }
}
