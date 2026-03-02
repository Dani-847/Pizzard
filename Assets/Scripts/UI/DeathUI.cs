using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pizzard.Core;

/// <summary>
/// Gestiona la pantalla de muerte: mostrar/ocultar y botones de acciones.
/// Se integra con GameFlowManager para controlar el flujo del juego.
/// </summary>
public class DeathUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Panel raíz de la pantalla de muerte (contiene el mensaje y los botones).")]
    [SerializeField] public GameObject pantallaMuerte;

    [Tooltip("Full-screen dark overlay behind death content.")]
    [SerializeField] public Image darkOverlay;

    [Tooltip("Death image placeholder (centered upper half).")]
    [SerializeField] public Image deathImage;

    [Tooltip("Botón de reintentar (reinicia el combate del boss).")]
    [SerializeField] public Button botonReintentar;

    [Tooltip("Botón de salir al menú principal.")]
    [SerializeField] public Button botonSalirAlMenu;

    private bool _estaActiva = false;

    private void Awake()
    {
        // Asegura que la pantalla de muerte empieza oculta
        if (pantallaMuerte != null)
            pantallaMuerte.SetActive(false);

        // Registra eventos de los botones
        if (botonReintentar != null)
            botonReintentar.onClick.AddListener(OnClickReintentar);

        if (botonSalirAlMenu != null)
            botonSalirAlMenu.onClick.AddListener(OnClickSalirAlMenu);
    }

    /// <summary>
    /// Muestra la pantalla de muerte y pausa el juego.
    /// Llamar a este método cuando el jugador muera.
    /// </summary>
    public void MostrarPantallaMuerte()
    {
        if (_estaActiva) return;

        _estaActiva = true;

        // Force this exact GameObject to be active
        gameObject.SetActive(true);

        if (pantallaMuerte != null)
            pantallaMuerte.SetActive(true);
        if (darkOverlay != null)
            darkOverlay.gameObject.SetActive(true);
        if (deathImage != null)
            deathImage.gameObject.SetActive(true);
            
        // Explicitly activate buttons just in case
        if (botonReintentar != null)
            botonReintentar.gameObject.SetActive(true);
        if (botonSalirAlMenu != null)
            botonSalirAlMenu.gameObject.SetActive(true);

        // Pausar el juego
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Oculta la pantalla de muerte y reanuda el juego.
    /// </summary>
    public void OcultarPantallaMuerte()
    {
        _estaActiva = false;

        if (pantallaMuerte != null)
            pantallaMuerte.SetActive(false);
        if (darkOverlay != null)
            darkOverlay.gameObject.SetActive(false);
        if (deathImage != null)
            deathImage.gameObject.SetActive(false);

        // Reanudar el juego
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Botón de reintentar: devuelve al jugador a la tienda del boss actual.
    /// </summary>
    private void OnClickReintentar()
    {
        OcultarPantallaMuerte();
        
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.VolverATiendaTrasMuerte();
        }
        else
        {
            // Fallback: reiniciar la escena actual
            Time.timeScale = 1f;
            Scene escenaActual = SceneManager.GetActiveScene();
            SceneManager.LoadScene(escenaActual.buildIndex);
        }
    }

    /// <summary>
    /// Botón de salir al menú principal.
    /// </summary>
    private void OnClickSalirAlMenu()
    {
        OcultarPantallaMuerte();
        
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.VolverAlMenu();
        }
        else
        {
            Debug.LogError("[DeathUI] GameFlowManager.Instance no encontrado");
        }
    }
}
