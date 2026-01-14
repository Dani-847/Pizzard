using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la pantalla de muerte: mostrar/ocultar y botones de acciones.
/// </summary>
public class DeathUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Panel raíz de la pantalla de muerte (contiene el mensaje y los botones).")]
    [SerializeField] public GameObject pantallaMuerte;

    [Tooltip("Botón de reintentar (reinicia la escena actual).")]
    [SerializeField] public Button botonReintentar;

    [Tooltip("Botón de salir al menú principal.")]
    [SerializeField] public Button botonSalirAlMenu;

    [Header("Opciones")]
    [Tooltip("Nombre de la escena del menú principal. Déjalo vacío si aún no la tienes.")]
    [SerializeField] public MenuUI escenaMenuPrincipal;

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
    /// Botón de reintentar: reinicia la escena actual.
    /// </summary>
    private void OnClickReintentar()
    {
        Time.timeScale = 1f; // Asegura que el tiempo vuelve a la normalidad
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.buildIndex);
    }

    /// <summary>
    /// Botón de salir al menú principal.
    /// </summary>
    private void OnClickSalirAlMenu()
    {
        Time.timeScale = 1f;

        if (escenaMenuPrincipal == null)
        {
            Debug.Log("Falta la escena del menú principal. Por favor, configúrala en el inspector.");
            return;
        }
        else
        {
            //SceneManager.LoadScene(escenaMenuPrincipal);
            escenaMenuPrincipal.Show();
            gameObject.SetActive(false);
        }
    }
}
