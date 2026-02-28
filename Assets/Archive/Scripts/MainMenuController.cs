using UnityEngine;

namespace Pizzard.UI
{
    using Core;

    /// <summary>
    /// Handles the Main Menu logic, connecting UI buttons to the overarching GameFlowManager.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            // Ensure settings are hidden on load
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Triggered by the "Play" UI Button. Starts the game loop.
        /// </summary>
        public void OnPlayClicked()
        {
            Debug.Log("[MainMenuController] Play clicked. Transitioning to Intro Dialog.");
            GameFlowManager.Instance.ChangeState(GameState.Dialogue);
        }

        /// <summary>
        /// Triggered by the "Settings" UI Button. Shows the settings panel.
        /// </summary>
        public void OnSettingsClicked()
        {
            Debug.Log("[MainMenuController] Settings clicked.");
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Triggered by the "Exit" UI Button. Quits the application.
        /// </summary>
        public void OnExitClicked()
        {
            Debug.Log("[MainMenuController] Exit clicked.");
            Application.Quit();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
