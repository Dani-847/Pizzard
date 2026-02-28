using UnityEngine;
using Pizzard.UI; // Access to ShopController if needed

namespace Pizzard.Core
{
    /// <summary>
    /// Manages the Shop Game Loop phase, providing access to Shop functionalities and
    /// eventually allowing the player to exit the Shop and continue to the Pre-Boss dialog.
    /// </summary>
    public class ShopPhaseManager : MonoBehaviour
    {
        [Header("Shop Setup")]
        [SerializeField] private ShopController shopController;

        private void Start()
        {
            Debug.Log("[ShopPhaseManager] Shop session started. Welcome!");
            if (shopController == null)
            {
                Debug.LogWarning("[ShopPhaseManager] ShopController missing. The UI won't function.");
            }
        }

        /// <summary>
        /// Triggered by a standard "Leave Shop" UI Button.
        /// Transitions to the next phase in the Main Loop.
        /// </summary>
        public void OnLeaveShop()
        {
            Debug.Log("[ShopPhaseManager] Leaving Shop. Transitioning to PreBossDialog...");
            GameFlowManager.Instance.ChangeState(GameState.PreBossDialogue);
        }
    }
}
