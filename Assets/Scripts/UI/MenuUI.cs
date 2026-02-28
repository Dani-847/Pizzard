using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pizzard.Core;

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
    [Tooltip("Continue from last save (visible only when save exists)")]
    public Button botonContinuar;
    [Tooltip("Botón para reanudar el juego (solo visible en pausa)")]
    public Button botonReanudar;
    [Tooltip("Botón para volver al menú principal desde la pausa")]
    public Button botonVolverMenu;

    [Header("Referencias externas")]
    [Tooltip("Panel de ajustes (activar/desactivar)")]
    public GameObject optionsUIPanel;

    [Header("Estado")]
    private GameFlowManager flowManager;

    void Start()
    {
        if (botonJugar != null)
            botonJugar.onClick.AddListener(OnClickJugar);
        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(OnClickContinuar);
        if (botonAjustes != null)
            botonAjustes.onClick.AddListener(OnClickAjustes);
        if (botonSalir != null)
            botonSalir.onClick.AddListener(OnClickSalir);
        if (botonReanudar != null)
            botonReanudar.onClick.AddListener(OnClickReanudar);
        if (botonVolverMenu != null)
            botonVolverMenu.onClick.AddListener(OnClickVolverMenu);

        // Show/hide Continue based on saved progress
        RefreshContinueButton();

        // Ocultar botones de pausa inicialmente
        SetPauseButtonsVisible(false);
    }

    /// <summary>
    /// Shows or hides the Continue button based on saved game state.
    /// </summary>
    public void RefreshContinueButton()
    {
        if (botonContinuar == null) return;
        bool hasSave = GameFlowManager.Instance != null && GameFlowManager.Instance.HasSavedGame();
        botonContinuar.gameObject.SetActive(hasSave);
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
    /// Continue from last saved game state.
    /// </summary>
    public void OnClickContinuar()
    {
        Debug.Log("Continuing from saved game");
        Hide();

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ContinuarJuego();
        }
        else
        {
            Debug.LogError("[MenuUI] GameFlowManager.Instance not found");
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
        if (botonContinuar != null)
            botonContinuar.gameObject.SetActive(visible);
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
