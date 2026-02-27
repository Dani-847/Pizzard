using UnityEngine;

namespace Pizzard.Core
{
    public enum GameState
    {
        MainMenu,
        Dialogue,
        Shop,
        PreBossDialogue,
        Combat,
        PostBossDialogue,
        Credits
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
            
            // Ensure ManaSystem persists — mana bar depends on this singleton
            if (ManaSystem.Instance == null && gameObject.GetComponent<ManaSystem>() == null)
            {
                gameObject.AddComponent<ManaSystem>();
                Debug.Log("[GameFlowManager] Auto-created ManaSystem singleton.");
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
                ChangeState(GameState.Combat);
            }
            else
            {
                ChangeState(GameState.MainMenu);
            }

            // Token logic moved to IniciarJuego and LoadGame handling
        }

        /// <summary>
        /// Transitions the game to a new state and triggers corresponding initialization logic.
        /// Load the mapped scene correctly.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            // --- STRICT TRANSITION GUARDS ---
            // Game Loop: MainMenu -> Dialogue -> Shop -> PreBossDialogue -> Combat -> PostBossDialogue -> Shop/Credits
            if (CurrentState != GameState.MainMenu) // MainMenu can go anywhere on load
            {
                bool valid = true;
                switch (CurrentState)
                {
                    case GameState.Dialogue: valid = (newState == GameState.Shop); break;
                    case GameState.Shop: valid = (newState == GameState.PreBossDialogue || newState == GameState.Combat); break; // Fallback to combat if dialogue missing
                    case GameState.PreBossDialogue: valid = (newState == GameState.Combat); break;
                    case GameState.Combat: valid = (newState == GameState.PostBossDialogue || newState == GameState.Shop); break; // Fallback to shop if dialogue missing
                    case GameState.PostBossDialogue: valid = (newState == GameState.Shop || newState == GameState.Credits); break;
                    default: valid = false; break; // Block transitions from unhandled states
                }
                if (!valid)
                {
                    Debug.LogError($"[GameFlowManager] INVALID TRANSITION ATTEMPT: {CurrentState} -> {newState}. Blocked.");
                    return;
                }
            }
            // --------------------------------

            Debug.Log($"[GameFlowManager] Changing state: {CurrentState} -> {newState}");
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
                    case GameState.Dialogue:
                        if (ui.dialogUI != null) ui.dialogUI.ShowIntroDialog(this); // Fallback for now
                        break;
                    case GameState.PreBossDialogue:
                        if (ui.dialogUI != null) ui.dialogUI.ShowPreBossDialog(this);
                        break;
                    case GameState.PostBossDialogue:
                        if (ui.dialogUI != null) ui.dialogUI.ShowPostBossDialog(this);
                        break;
                    case GameState.Shop:
                        if (ui.tiendaUI != null) ui.tiendaUI.Show(this);
                        break;
                    case GameState.Combat:
                        // Enable HUD elements during boss fights
                        Transform elementsUI = uiParent.Find("Elementos");
                        Transform bossUI = uiParent.Find("PblobUI"); // Boss proto name
                        Transform playerHP = uiParent.Find("HealthUI");
                        Transform potionUI = uiParent.Find("PotionUI");
                        Transform manaUI = uiParent.Find("ManaUI");
                        
                        if (elementsUI) elementsUI.gameObject.SetActive(true);
                        if (bossUI) bossUI.gameObject.SetActive(true);
                        if (playerHP) playerHP.gameObject.SetActive(true);
                        if (potionUI) potionUI.gameObject.SetActive(true);
                        if (manaUI) manaUI.gameObject.SetActive(true);
                        break;
                    case GameState.Credits:
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
                GameState.Dialogue => "IntroDialog", // Placeholder for actual new Dialogue logic
                GameState.Shop => "Shop",
                GameState.PreBossDialogue => "PreBossDialog",
                GameState.Combat => "BossArena_" + currentBossIndex,
                GameState.PostBossDialogue => "PostBossDialog",
                GameState.Credits => "Credits",
                _ => string.Empty
            };
        }

        public void IniciarJuego()
        {
            if (Progression.SaveManager.Instance != null)
            {
                Progression.SaveManager.Instance.ResetSave();
                
                // Clear active combinations
                var combiner = FindObjectOfType<ElementsCombiner>(true);
                if (combiner != null)
                {
                    combiner.ClearSelectedElements();
                }

                // Reset all selected elements from wands
                var equipSelector = FindObjectOfType<EquipSelectorUI>(true);
                if (equipSelector != null && equipSelector.availableEquipables != null)
                {
                    foreach(var wand in equipSelector.availableEquipables)
                    {
                        if (wand != null && wand.elements != null)
                        {
                            wand.elements.Clear();
                        }
                    }
                }
            }

            currentBossIndex = 1;

            if (Progression.ProgressionManager.Instance != null && Progression.ProgressionManager.Instance.BossCurrency == 0)
            {
                Progression.ProgressionManager.Instance.AddCurrency(1);
                Debug.Log("[GameFlowManager] Granted 1 starting token for Shop 1 mandatory purchase.");
                
                if (Progression.SaveManager.Instance != null)
                {
                    Progression.SaveManager.Instance.SaveGame();
                }
            }

            ChangeState(GameState.Dialogue);
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
            ChangeState(GameState.Combat);
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
                case GameState.Dialogue:
                    ChangeState(GameState.Shop);
                    break;
                case GameState.Shop:
                    // --- WAVE 2: SHOP 1 HARD-LOCK ---
                    if (currentBossIndex == 1 && Progression.SaveManager.Instance != null && Progression.SaveManager.Instance.CurrentSave.wandTier == 0)
                    {
                        Debug.LogWarning("[GameFlowManager] BACKEND BLOCK: Shop 1 is HARD-LOCKED until wandTier >= 1.");
                        return;
                    }
                    ChangeState(GameState.PreBossDialogue);
                    break;
                case GameState.PreBossDialogue:
                    ChangeState(GameState.Combat);
                    break;
                case GameState.Combat:
                    // Boss defeated
                    // Reward tokens via ProgressionManager
                    int tokensToReward = (currentBossIndex == 1) ? 1 : 2;
                    if (Progression.ProgressionManager.Instance != null)
                    {
                        Progression.ProgressionManager.Instance.AddCurrency(tokensToReward);
                    }
                    
                    // --- WAVE 3: AUTO-SAVE on boss defeat ---
                    if (Progression.SaveManager.Instance != null)
                    {
                        Progression.SaveManager.Instance.SaveGame();
                        Debug.Log("[GameFlowManager] Auto-saved after boss defeat.");
                    }
                    
                    ChangeState(GameState.PostBossDialogue);
                    break;
                case GameState.PostBossDialogue:
                    if (currentBossIndex < 4)
                    {
                        currentBossIndex++;
                        ChangeState(GameState.Shop);
                    }
                    else
                    {
                        // Win Condition: All bosses defeated.
                        Debug.Log("[GameFlowManager] All bosses defeated! Triggering Win Sequence.");
                        ChangeState(GameState.Credits);
                    }
                    break;
                case GameState.Credits:
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
