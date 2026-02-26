using UnityEngine;

namespace Pizzard.Progression
{
    /// <summary>
    /// Manages persistent player data across runs, such as boss currency (drops)
    /// and unlocked upgrades.
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public int BossCurrency { get; private set; }

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

        /// <summary>
        /// Adds currency, usually given after defeating a boss.
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            
            BossCurrency += amount;
            Debug.Log($"[ProgressionManager] Added {amount} Boss Currency. Total: {BossCurrency}");
        }

        /// <summary>
        /// Spends currency in the shop. Returns true if successful.
        /// </summary>
        public bool SpendCurrency(int amount)
        {
            if (amount <= 0 || BossCurrency < amount)
            {
                Debug.LogWarning($"[ProgressionManager] Not enough currency to spend {amount}. Current: {BossCurrency}");
                return false;
            }

            BossCurrency -= amount;
            Debug.Log($"[ProgressionManager] Spent {amount} Boss Currency. Remaining: {BossCurrency}");
            return true;
        }

        // For future: Unlocked Wand Tiers or other persistent stats can go here.
    }
}
