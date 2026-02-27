using UnityEngine;

namespace Pizzard.Core
{
    public enum GameState
    {
        MainMenu,
        IntroDialog,
        Shop,
        PreBossDialog,
        BossFight,
        PostBossDialog,
        WinSequence // New State for Magic Pizza / Credits
    }

    /// <summary>
    /// Singleton Manager controlling the overarching state of the game loop.
    /// Handles transitions between Narrative, Shop, and Boss phases.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }
        private bool isInitialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure ProgressionManager persists on the same core object or is instantiated
            if (gameObject.GetComponent<Progression.ProgressionManager>() == null)
            {
                gameObject.AddComponent<Progression.ProgressionManager>();
            }
        }

        private void Start()
        {
            // Default initialization state
            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeSceneName.StartsWith("BossArena"))
            {
                string[] parts = activeSceneName.Split('_');
                if (parts.Length > 1 && int.TryParse(parts[1], out int index))
                {
                    currentBossIndex = index;
                }
                ChangeState(GameState.BossFight);
            }
            else
            {
                ChangeState(GameState.MainMenu);
            }
        }

        /// <summary>
        /// Transitions the game to a new state and triggers corresponding initialization logic.
        /// Load the mapped scene correctly.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (isInitialized && CurrentState == newState) return;
            isInitialized = true;

            Debug.Log($"[GameFlowManager] State Transition: {CurrentState} -> {newState}");
            CurrentState = newState;

            string targetScene = GetSceneForState(newState);

            // Toggle specific UI canvases by name based on the current state.
            // Our single UI prefab has all components active. We find them and disable/enable.
            UIManager ui = UIManager.Instance;
            if (ui != null)
            {
                // Deactivate everything
                ui.HideAllUIs();
                Transform uiParent = ui.transform;
                // Specific HUDs we need to control by name (assuming name based on prototype)
                foreach(Transform child in uiParent)
                {
                    // By default disable all raw canvases
                    if(child != uiParent)
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                // Enable exactly what is needed for the phase
                switch (newState)
                {
                    case GameState.MainMenu:
                        if (ui.menuUI != null) ui.menuUI.Show();
                        break;
                    case GameState.IntroDialog:
                        if (ui.dialogUI != null) ui.dialogUI.ShowIntroDialog(this);
                        break;
                    case GameState.PreBossDialog:
                        if (ui.dialogUI != null) ui.dialogUI.ShowPreBossDialog(this);
                        break;
                    case GameState.PostBossDialog:
                        if (ui.dialogUI != null) ui.dialogUI.ShowPostBossDialog(this);
                        break;
                    case GameState.Shop:
                        if (ui.tiendaUI != null) ui.tiendaUI.Show(this);
                        break;
                    case GameState.BossFight:
                        // Enable HUD elements during boss fights
                        Transform elementsUI = uiParent.Find("Elementos");
                        Transform bossUI = uiParent.Find("PblobUI"); // Boss proto name
                        Transform playerHP = uiParent.Find("HealthUI");
                        Transform potionUI = uiParent.Find("PotionUI");
                        
                        if (elementsUI) elementsUI.gameObject.SetActive(true);
                        if (bossUI) bossUI.gameObject.SetActive(true);
                        if (playerHP) playerHP.gameObject.SetActive(true);
                        if (potionUI) potionUI.gameObject.SetActive(true);
                        break;
                    case GameState.WinSequence:
                        // We leave all UI hidden, the Credits scene will have its own Canvas
                        break;
                }
            }

            // Force unpause when transitioning states
            Time.timeScale = 1f;

            if (!string.IsNullOrEmpty(targetScene))
            {
                SceneLoader.LoadScene(targetScene);
            }
        }

        public int currentBossIndex { get; private set; } = 1;

        private string GetSceneForState(GameState state)
        {
            return state switch
            {
                GameState.MainMenu => "MainMenu",
                GameState.IntroDialog => "IntroDialog",
                GameState.Shop => "Shop",
                GameState.PreBossDialog => "PreBossDialog",
                GameState.BossFight => "BossArena_" + currentBossIndex,
                GameState.PostBossDialog => "PostBossDialog",
                GameState.WinSequence => "Credits",
                _ => string.Empty
            };
        }

        public void IniciarJuego()
        {
            currentBossIndex = 1;
            ChangeState(GameState.IntroDialog);
        }

        public void VolverAlMenu()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.MainMenu);
        }

        public void ReanudarJuego()
        {
            Time.timeScale = 1f;
        }

        public void ReiniciarBossFight()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.BossFight);
        }

        public void VolverATiendaTrasMuerte()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Shop);
            
            var dialog = FindObjectOfType<DialogUI>(true);
            if (dialog != null)
            {
                dialog.ShowDeathShopDialog(this);
            }
        }

        public void AvanzarFase()
        {
            switch (CurrentState)
            {
                case GameState.IntroDialog:
                    ChangeState(GameState.Shop);
                    break;
                case GameState.Shop:
                    ChangeState(GameState.PreBossDialog);
                    break;
                case GameState.PreBossDialog:
                    ChangeState(GameState.BossFight);
                    break;
                case GameState.BossFight:
                    // Boss defeated
                    // Reward tokens via ProgressionManager
                    int tokensToReward = (currentBossIndex == 1) ? 1 : 2;
                    if (Progression.ProgressionManager.Instance != null)
                    {
                        Progression.ProgressionManager.Instance.AddCurrency(tokensToReward);
                    }
                    
                    ChangeState(GameState.PostBossDialog);
                    break;
                case GameState.PostBossDialog:
                    if (currentBossIndex < 4)
                    {
                        currentBossIndex++;
                        ChangeState(GameState.Shop);
                    }
                    else
                    {
                        // Win Condition: All bosses defeated.
                        Debug.Log("[GameFlowManager] All bosses defeated! Triggering Win Sequence.");
                        ChangeState(GameState.WinSequence);
                    }
                    break;
                case GameState.WinSequence:
                    VolverAlMenu(); // Used when exiting Credits
                    break;
                default:
                    Debug.LogWarning("[GameFlowManager] Cannot advance phase from " + CurrentState);
                    break;
            }
        }

        public void OnPlayerDeath()
        {
            var deathUI = FindObjectOfType<DeathUI>(true);
            if (deathUI != null)
            {
                deathUI.MostrarPantallaMuerte();
            }
            else
            {
                VolverAlMenu();
            }
        }
    }
}
