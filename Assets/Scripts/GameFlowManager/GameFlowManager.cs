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
        }

        private void Start()
        {
            // Default initialization state
            ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Transitions the game to a new state and triggers corresponding initialization logic.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            Debug.Log($"[GameFlowManager] State Transition: {CurrentState} -> {newState}");
            CurrentState = newState;

            // Optional: In the future, emit an event here so UI and other systems can react automatically.
        }
    }
}
