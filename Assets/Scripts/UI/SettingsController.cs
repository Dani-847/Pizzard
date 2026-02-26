using UnityEngine;

namespace Pizzard.UI
{
    /// <summary>
    /// Handles the basic behavior for the Settings Panel logic (e.g., closing it).
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        /// <summary>
        /// Triggered by the "Close" or "Back" UI Button on the Settings Panel.
        /// </summary>
        public void OnCloseClicked()
        {
            Debug.Log("[SettingsController] Close clicked. Hiding settings panel.");
            gameObject.SetActive(false);
        }

        // Additional Audio/Video logic placeholders can go here moving forward.
    }
}
