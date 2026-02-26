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
            
            // Attach SaveManager alongside this progression data script if missing
            if (gameObject.GetComponent<SaveManager>() == null)
            {
                gameObject.AddComponent<SaveManager>();
            }
        }

        /// <summary>
        /// Adds currency, usually given after defeating a boss.
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            
            BossCurrency += amount;
            Debug.Log($"[ProgressionManager] Added {amount} Boss Currency. Total: {BossCurrency}");
            
            SaveManager.Instance?.SaveGame();
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
            
            SaveManager.Instance?.SaveGame();
            return true;
        }

        /// <summary>
        /// Called by SaveManager to overwrite the in-memory currency upon load.
        /// </summary>
        public void SetCurrencyFromSave(int savedTokens)
        {
            BossCurrency = savedTokens;
            Debug.Log($"[ProgressionManager] Loaded {savedTokens} Boss Currency from JSON.");
        }
    }
}
