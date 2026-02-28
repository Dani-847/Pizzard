using UnityEngine;

namespace Pizzard.UI
{
    using Progression;

    /// <summary>
    /// Processes Shop UI purchasing logic, delegating currency checks to the ProgressionManager.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        [Header("Shop Pricing")]
        [SerializeField] private int wandUpgradeCost = 100;
        [SerializeField] private int healthCost = 50;

        /// <summary>
        /// Triggered by a "Buy Wand Upgrade" UI Button.
        /// </summary>
        public void OnBuyWandUpgrade()
        {
            if (ProgressionManager.Instance.SpendCurrency(wandUpgradeCost))
            {
                Debug.Log("[ShopController] Wand Upgrade purchased successfully.");
                
                // Usually, we'd notify the Player's WandController here to upgrade.
                // e.g. FindObjectOfType<Player.WandController>().UpgradeWandTier();
            }
            else
            {
                Debug.LogWarning("[ShopController] Failed to purchase Wand Upgrade. Not enough currency.");
            }
        }

        /// <summary>
        /// Triggered by a "Buy Health" UI Button.
        /// </summary>
        public void OnBuyHealth()
        {
            if (ProgressionManager.Instance.SpendCurrency(healthCost))
            {
                Debug.Log("[ShopController] Health purchased successfully.");
                
                // e.g. FindObjectOfType<Player.PlayerHealth>().Heal(1);
            }
            else
            {
                Debug.LogWarning("[ShopController] Failed to purchase Health. Not enough currency.");
            }
        }
    }
}
