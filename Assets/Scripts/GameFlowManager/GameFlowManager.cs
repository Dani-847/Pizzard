using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquesta el flujo del juego por escena de boss.
/// Controla las fases: Diálogo -> Tienda -> Diálogo -> Boss -> (ciclo) 
/// y gestiona la pausa con Escape.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UIManager uiManager;
    [Tooltip("Referencia al BossController abstracto (si se usa)")]
    [SerializeField] private BossController bossController;
    [Tooltip("Referencia al PblobController específico (si no hay BossController)")]
    [SerializeField] private PblobController pblobController;
    [SerializeField] private PlayerController playerController;

    [Header("Fase inicial")]
    [SerializeField] private GamePhase faseInicial = GamePhase.IntroDialog;

    [Header("Configuración de Escenas")]
    [Tooltip("Nombre de la escena del siguiente boss")]
    [SerializeField] private string siguienteBossEscena = "";

    private GamePhase faseActual;
    private GamePhase faseAntesPausa; // Para volver a la fase correcta al reanudar

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = UIManager.Instance;
        }

        // Suscribirse al evento de boss derrotado si existe
        if (bossController != null)
        {
            bossController.OnBossDefeated += OnBossDefeated;
        }
        
        // Suscribirse al evento de PblobController si existe
        if (pblobController != null)
        {
            pblobController.OnBossDefeated.AddListener(OnBossDefeated);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        if (bossController != null)
        {
            bossController.OnBossDefeated -= OnBossDefeated;
        }
        
        if (pblobController != null)
        {
            pblobController.OnBossDefeated.RemoveListener(OnBossDefeated);
        }
    }

    private void Start()
    {
        CambiarFase(faseInicial);
    }

    private void Update()
    {
        // Manejo de pausa con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (faseActual != GamePhase.Paused)
            {
                EntrarPausa();
            }
            else
            {
                SalirPausa();
            }
        }
    }

    /// <summary>
    /// Cambia a una nueva fase del flujo de juego.
    /// </summary>
    private void CambiarFase(GamePhase nuevaFase)
    {
        faseActual = nuevaFase;

        // Por defecto, desactivar combate
        if (bossController != null) bossController.enabled = false;
        if (pblobController != null) pblobController.enabled = false;
        if (playerController != null) playerController.EnableInput(false);

        // Ocultar todas las UIs de flujo
        OcultarTodasLasUIs();

        switch (faseActual)
        {
            case GamePhase.IntroDialog:
                MostrarDialogoIntro();
                break;

            case GamePhase.ShopBeforeBoss:
                MostrarTiendaInicial();
                break;

            case GamePhase.PreBossDialog:
                MostrarDialogoPreBoss();
                break;

            case GamePhase.BossFight:
                IniciarBossFight();
                break;

            case GamePhase.PostBossDialog:
                MostrarDialogoPostBoss();
                break;

            case GamePhase.ShopAfterBoss:
                MostrarTiendaDespuesBoss();
                break;

            case GamePhase.PreNextBossDialog:
                MostrarDialogoAntesSiguienteBoss();
                break;

            case GamePhase.Paused:
                MostrarMenuPausa();
                break;
        }

        Debug.Log($"[GameFlowManager] Fase cambiada a: {faseActual}");
    }

    /// <summary>
    /// Oculta todas las UIs de flujo.
    /// </summary>
    private void OcultarTodasLasUIs()
    {
        if (uiManager != null)
        {
            if (uiManager.menuUI != null) uiManager.menuUI.Hide();
            if (uiManager.optionsUI != null) uiManager.optionsUI.Hide();
            if (uiManager.tiendaUI != null) uiManager.tiendaUI.Hide();
            if (uiManager.dialogUI != null) uiManager.dialogUI.Hide();
        }
    }

    /// <summary>
    /// Avanza a la siguiente fase del flujo.
    /// Llamado por las UIs cuando terminan su función.
    /// </summary>
    public void AvanzarFase()
    {
        switch (faseActual)
        {
            case GamePhase.IntroDialog:
                CambiarFase(GamePhase.ShopBeforeBoss);
                break;

            case GamePhase.ShopBeforeBoss:
                CambiarFase(GamePhase.PreBossDialog);
                break;

            case GamePhase.PreBossDialog:
                CambiarFase(GamePhase.BossFight);
                break;

            case GamePhase.BossFight:
                CambiarFase(GamePhase.PostBossDialog);
                break;

            case GamePhase.PostBossDialog:
                CambiarFase(GamePhase.ShopAfterBoss);
                break;

            case GamePhase.ShopAfterBoss:
                CambiarFase(GamePhase.PreNextBossDialog);
                break;

            case GamePhase.PreNextBossDialog:
                CargarSiguienteBoss();
                break;
        }
    }

    // ========================================
    // MÉTODOS DE MOSTRAR DIÁLOGOS/TIENDA
    // ========================================

    private void MostrarDialogoIntro()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowIntroDialog(this);
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] DialogUI no disponible, avanzando fase");
            AvanzarFase();
        }
    }

    private void MostrarTiendaInicial()
    {
        if (uiManager != null && uiManager.tiendaUI != null)
        {
            uiManager.tiendaUI.Show(this);
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] TiendaUI no disponible, avanzando fase");
            AvanzarFase();
        }
    }

    private void MostrarDialogoPreBoss()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowPreBossDialog(this);
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] DialogUI no disponible, avanzando fase");
            AvanzarFase();
        }
    }

    private void IniciarBossFight()
    {
        // Activar el boss controller correspondiente
        if (bossController != null)
        {
            bossController.enabled = true;
            bossController.StartBossFight();
        }
        else if (pblobController != null)
        {
            pblobController.enabled = true;
            pblobController.StartBossBattle();
        }
        
        if (playerController != null) 
        {
            playerController.EnableInput(true);
        }
        
        // Reproducir música de boss si está disponible
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossMusic();
        }

        Debug.Log("[GameFlowManager] ¡Combate de boss iniciado!");
    }

    private void MostrarDialogoPostBoss()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowPostBossDialog(this);
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] DialogUI no disponible, avanzando fase");
            AvanzarFase();
        }
    }

    private void MostrarTiendaDespuesBoss()
    {
        if (uiManager != null && uiManager.tiendaUI != null)
        {
            uiManager.tiendaUI.Show(this);
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] TiendaUI no disponible, avanzando fase");
            AvanzarFase();
        }
    }

    private void MostrarDialogoAntesSiguienteBoss()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowPreNextBossDialog(this);
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] DialogUI no disponible, avanzando fase");
            AvanzarFase();
        }
    }

    private void CargarSiguienteBoss()
    {
        if (!string.IsNullOrEmpty(siguienteBossEscena))
        {
            Debug.Log($"[GameFlowManager] Cargando siguiente boss: {siguienteBossEscena}");
            SceneManager.LoadScene(siguienteBossEscena);
        }
        else
        {
            Debug.Log("[GameFlowManager] No hay siguiente boss configurado. ¡Victoria!");
            // Aquí podrías mostrar una pantalla de victoria o volver al menú
        }
    }

    // ========================================
    // MANEJO DE PAUSA
    // ========================================

    private void EntrarPausa()
    {
        faseAntesPausa = faseActual;
        Time.timeScale = 0f;
        CambiarFase(GamePhase.Paused);
    }

    private void SalirPausa()
    {
        Time.timeScale = 1f;
        CambiarFase(faseAntesPausa);
    }

    /// <summary>
    /// Reanuda el juego desde el menú de pausa.
    /// Llamado por MenuUI.
    /// </summary>
    public void ReanudarJuego()
    {
        SalirPausa();
    }

    private void MostrarMenuPausa()
    {
        if (uiManager != null && uiManager.menuUI != null)
        {
            uiManager.menuUI.ShowPauseMenu(this);
        }
    }

    // ========================================
    // EVENTOS
    // ========================================

    /// <summary>
    /// Callback cuando el boss es derrotado.
    /// </summary>
    private void OnBossDefeated()
    {
        Debug.Log("[GameFlowManager] ¡Boss derrotado!");
        AvanzarFase();
    }

    /// <summary>
    /// Obtiene la fase actual del flujo.
    /// </summary>
    public GamePhase GetFaseActual()
    {
        return faseActual;
    }
}
