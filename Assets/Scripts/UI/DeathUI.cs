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

        if (pantallaMuerte != null)
            pantallaMuerte.SetActive(true);

        // Pausar el juego
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Oculta la pantalla de muerte y reanuda el juego.
    /// </summary>
    public void OcultarPantallaMuerte()
    {
        if (!_estaActiva) return;

        _estaActiva = false;

        if (pantallaMuerte != null)
            pantallaMuerte.SetActive(false);

        // Reanudar el juego
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Botón de reintentar: reinicia el combate del boss actual.
    /// </summary>
    private void OnClickReintentar()
    {
        OcultarPantallaMuerte();
        
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ReiniciarBossFight();
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
