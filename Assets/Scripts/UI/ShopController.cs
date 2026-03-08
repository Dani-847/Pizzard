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

        private ITokenSource _tokenSource;

        /// <summary>
        /// Sets the token source for this controller. If not called, falls back to ProgressionManager.
        /// </summary>
        public void SetTokenSource(ITokenSource source)
        {
            _tokenSource = source;
        }

        /// <summary>
        /// Triggered by a "Buy Wand Upgrade" UI Button.
        /// </summary>
        public void OnBuyWandUpgrade()
        {
            ITokenSource src = _tokenSource ?? ProgressionManager.Instance as ITokenSource;
            if (src != null && src.SpendTokens(wandUpgradeCost))
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
            ITokenSource src = _tokenSource ?? ProgressionManager.Instance as ITokenSource;
            if (src != null && src.SpendTokens(healthCost))
            {
                Debug.Log("[ShopController] Health purchased successfully.");
                
                // e.g. FindObjectOfType<Player.PlayerHealth>().Heal(1);
            }
            else
            {
                Debug.LogWarning("[ShopController] Failed to purchase Health. Not enough currency.");
            }
        }
    

#if UNITY_EDITOR
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 160f, 10f, 150f, 70f));
            GUI.Box(new Rect(0, 0, 150f, 70f), "");
            ITokenSource dbgSrc = _tokenSource ?? Progression.ProgressionManager.Instance as ITokenSource;
            int current = dbgSrc != null ? dbgSrc.GetTokens() : 0;
            GUILayout.Label($"Tokens: {current}");
            if (GUILayout.Button("+1 Token"))
                Progression.ProgressionManager.Instance?.AddCurrency(1);
            if (GUILayout.Button("+50 Tokens"))
                Progression.ProgressionManager.Instance?.AddCurrency(50);
            GUILayout.EndArea();
        }
#endif
}
}
