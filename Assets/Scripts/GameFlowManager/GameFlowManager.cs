using UnityEngine;

namespace Pizzard.Core
{
    public enum GameState
    {
        MainMenu,
        IntroDialog,
        Shop,
        PreBossDialog,
        BossFight
    }

    /// <summary>
    /// Singleton Manager controlling the overarching state of the game loop.
    /// Handles transitions between Narrative, Shop, and Boss phases.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

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
            ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Transitions the game to a new state and triggers corresponding initialization logic.
        /// Load the mapped scene correctly.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            Debug.Log($"[GameFlowManager] State Transition: {CurrentState} -> {newState}");
            CurrentState = newState;

            string targetScene = GetSceneForState(newState);
            if (!string.IsNullOrEmpty(targetScene))
            {
                SceneLoader.LoadScene(targetScene);
            }
        }

        private string GetSceneForState(GameState state)
        {
            return state switch
            {
                GameState.MainMenu => "MainMenu",
                GameState.IntroDialog => "IntroDialog",
                GameState.Shop => "Shop",
                GameState.PreBossDialog => "PreBossDialog",
                GameState.BossFight => "BossArena",
                _ => string.Empty
            };
        }
    }
}
