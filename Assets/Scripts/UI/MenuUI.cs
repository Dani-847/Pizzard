using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Opcional si el helper reinicia la escena

// Gestionar botones principales del menú (Jugar, Ajustes, Salir) 
// y lanzar el flujo jugable hacia el primer diálogo.
public class MenuUI : MonoBehaviour
{
    [Header("Referencias UI")]
    // Inicia el bucle jugable -> primer diálogo
    public Button botonJugar;
    // Abre OptionsUI
    public Button botonAjustes;
    // Cierra la aplicación
    public Button botonSalir;

    [Header("Referencias externas")]
    // Panel de ajustes (activar/desactivar)
    public GameObject optionsUIPanel;
    // (Opcional) helper para cargar escenas o iniciar juego
    // public SceneLoader sceneLoader;
    // (Opcional placeholder) punto de entrada a diálogos
    // public DialogueManager dialogueManager;

    // Asignar listeners a los botones
    void Start()
    {
        botonJugar.onClick.AddListener(OnClickJugar);
        botonAjustes.onClick.AddListener(OnClickAjustes);
        botonSalir.onClick.AddListener(OnClickSalir);
    }

    // Iniciar flujo jugable:
    // - Cerrar menú principal
    // - Preparar game state (GameManager)
    // - Llamar a dialogueManager.StartFirstDialogue()
    public void OnClickJugar()
    {
        Debug.Log("Iniciando flujo jugable");

        // Cerrar menú principal
        gameObject.SetActive(false);

        /*
        Prueba para luego si hacemos distintas escenas para los
        diálogos y los bosses, para que el bucle esté bien hecho

            int sceneIndex = SaveSystem.LoadLastSceneIndex();
            SceneLoader.LoadScene(sceneIndex);
        */
    }

    // Mostrar optionsUIPanel (setActive true)
    public void OnClickAjustes()
    {
        Debug.Log("Abriendo el panel de ajustes");

        Hide();
        UIManager.Instance.OpenOptions(UIContext.Menu);
    }

    // Application.Quit() y/o mostrar confirmación en editor
    public void OnClickSalir()
    {
        Debug.Log("Saliendo del juego...");
        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
    
    public void Show(){
        gameObject.SetActive(true);
    } 
    public void Hide(){
        gameObject.SetActive(false);
    } 
}
