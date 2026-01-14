using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BossController bossController; // tu script de boss
    [SerializeField] private PlayerController playerController; // tu script de jugador

    [Header("Fase inicial")]
    [SerializeField] private GamePhase faseInicial = GamePhase.IntroDialog;

    private GamePhase faseActual;

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = UIManager.Instance;
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

    private void CambiarFase(GamePhase nuevaFase)
    {
        faseActual = nuevaFase;

        // Por defecto, desactivar combate
        if (bossController != null) bossController.enabled = false;
        if (playerController != null) playerController.EnableInput(false);

        // Ocultar todas las UIs de flujo (ajusta a tus métodos reales)
        if (uiManager != null)
        {
            uiManager.menuUI.Hide();
            uiManager.optionsUI.Hide();
            uiManager.tiendaUI.Hide();
            uiManager.dialogUI.Hide();
        }

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

            case GamePhase.Paused:
                // Aquí puedes mostrar tu menú de pausa
                MostrarMenuPausa();
                break;
        }
    }

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
                // Aquí podrías cargar la escena del siguiente boss
                // SceneManager.LoadScene("Escena_Boss2");
                break;
        }
    }

    private void MostrarDialogoIntro()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowIntroDialog(this);
        }
    }

    private void MostrarTiendaInicial()
    {
        if (uiManager != null && uiManager.tiendaUI != null)
        {
            uiManager.tiendaUI.Show(this);
        }
    }

    private void MostrarDialogoPreBoss()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowPreBossDialog(this);
        }
    }

    private void IniciarBossFight()
    {
        if (bossController != null) bossController.enabled = true;
        if (playerController != null) playerController.EnableInput(true);
        // Aquí puedes mostrar HUD de combate, etc.
    }

    private void MostrarDialogoPostBoss()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowPostBossDialog(this);
        }
    }

    private void MostrarTiendaDespuesBoss()
    {
        if (uiManager != null && uiManager.tiendaUI != null)
        {
            uiManager.tiendaUI.Show(this);
        }
    }

    private void MostrarDialogoAntesSiguienteBoss()
    {
        if (uiManager != null && uiManager.dialogUI != null)
        {
            uiManager.dialogUI.ShowPreNextBossDialog(this);
        }
    }

    private void EntrarPausa()
    {
        Time.timeScale = 0f;
        CambiarFase(GamePhase.Paused);
    }

    private void SalirPausa()
    {
        Time.timeScale = 1f;
        // Decide a qué fase vuelves: por simplicidad aquí volvemos a combate
        CambiarFase(GamePhase.BossFight);
    }

    private void MostrarMenuPausa()
    {
        if (uiManager != null && uiManager.menuUI != null)
        {
            uiManager.menuUI.ShowPauseMenu(this);
        }
    }
}
