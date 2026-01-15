using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquesta el flujo del juego completo en una sola escena.
/// Controla las fases: MainMenu -> IntroDialog -> ShopBeforeBoss -> PreBossDialog -> BossFight -> 
/// (si muere: PlayerDeath, si gana: PostBossDialog -> ciclo siguiente boss).
/// Gestiona la pausa con Escape.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private UIManager uiManager;
    [Tooltip("Referencia al BossController abstracto (si se usa)")]
    [SerializeField] private BossController bossController;
    [Tooltip("Referencia al PblobController específico (si no hay BossController)")]
    [SerializeField] private PblobController pblobController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHPController playerHPController;

    [Header("Fase inicial")]
    [Tooltip("Fase inicial del juego (MainMenu para empezar con menú)")]
    [SerializeField] private GamePhase faseInicial = GamePhase.MainMenu;

    [Header("Configuración de Escenas")]
    [Tooltip("Nombre de la escena del siguiente boss (vacío si no hay más)")]
    [SerializeField] private string siguienteBossEscena = "";

    private GamePhase faseActual;
    private GamePhase faseAntesPausa;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
        if (bossController != null)
        {
            bossController.OnBossDefeated -= OnBossDefeated;
        }
        
        if (pblobController != null)
        {
            pblobController.OnBossDefeated.RemoveListener(OnBossDefeated);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        CambiarFase(faseInicial);
    }

    private void Update()
    {
        // Manejo de pausa con Escape (solo si no estamos en menú principal o muerte)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (faseActual == GamePhase.MainMenu || faseActual == GamePhase.PlayerDeath)
            {
                // No hacer nada en menú principal o pantalla de muerte
                return;
            }

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
            case GamePhase.MainMenu:
                MostrarMenuPrincipal();
                break;

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

            case GamePhase.PlayerDeath:
                MostrarPantallaMuerte();
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
            if (uiManager.deathUI != null) uiManager.deathUI.OcultarPantallaMuerte();
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
            case GamePhase.MainMenu:
                // Play fue presionado, ir al diálogo intro
                CambiarFase(GamePhase.IntroDialog);
                break;

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

    /// <summary>
    /// Llamado desde MenuUI cuando el jugador presiona "Jugar".
    /// Inicia el flujo de juego desde el diálogo intro.
    /// </summary>
    public void IniciarJuego()
    {
        Debug.Log("[GameFlowManager] Iniciando juego desde menú principal");
        CambiarFase(GamePhase.IntroDialog);
    }

    /// <summary>
    /// Llamado cuando el jugador muere.
    /// </summary>
    public void OnPlayerDeath()
    {
        Debug.Log("[GameFlowManager] Jugador ha muerto");
        CambiarFase(GamePhase.PlayerDeath);
    }

    /// <summary>
    /// Reinicia el combate del boss actual (desde pantalla de muerte).
    /// </summary>
    public void ReiniciarBossFight()
    {
        Debug.Log("[GameFlowManager] Reiniciando combate de boss");
        Time.timeScale = 1f;
        
        // Restaurar vida del jugador
        if (playerHPController != null)
        {
            playerHPController.RestaurarVidaCompleta();
        }
        
        CambiarFase(GamePhase.BossFight);
    }

    /// <summary>
    /// Vuelve al menú principal.
    /// </summary>
    public void VolverAlMenu()
    {
        Debug.Log("[GameFlowManager] Volviendo al menú principal");
        Time.timeScale = 1f;
        CambiarFase(GamePhase.MainMenu);
    }

    // ========================================
    // MÉTODOS DE MOSTRAR UIs
    // ========================================

    private void MostrarMenuPrincipal()
    {
        if (uiManager != null && uiManager.menuUI != null)
        {
            uiManager.menuUI.Show();
        }
        
        // Reproducir música de menú
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMenuMusic();
        }
    }

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
        else
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBossMusic();
            }
        }
        
        if (playerController != null) 
        {
            playerController.EnableInput(true);
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

    private void MostrarPantallaMuerte()
    {
        if (uiManager != null && uiManager.deathUI != null)
        {
            uiManager.deathUI.MostrarPantallaMuerte();
        }
    }

    private void CargarSiguienteBoss()
    {
        if (!string.IsNullOrEmpty(siguienteBossEscena))
        {
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == siguienteBossEscena)
                {
                    sceneExists = true;
                    break;
                }
            }

            if (sceneExists)
            {
                Debug.Log($"[GameFlowManager] Cargando siguiente boss: {siguienteBossEscena}");
                SceneManager.LoadScene(siguienteBossEscena);
            }
            else
            {
                Debug.LogError($"[GameFlowManager] La escena '{siguienteBossEscena}' no existe en Build Settings");
            }
        }
        else
        {
            Debug.Log("[GameFlowManager] No hay siguiente boss configurado. ¡Victoria!");
            // Volver al menú principal después de ganar
            VolverAlMenu();
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

    private void OnBossDefeated()
    {
        Debug.Log("[GameFlowManager] ¡Boss derrotado!");
        AvanzarFase();
    }

    public GamePhase GetFaseActual()
    {
        return faseActual;
    }
}
