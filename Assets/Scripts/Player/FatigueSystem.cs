using UnityEngine;
using Pizzard.Progression;

namespace Pizzard.Core
{
    /// <summary>
    /// Manages the player's Elemental Fatigue.
    /// Spells consume fatigue. Fatigue slowly recovers over time.
    /// Capacity can be upgraded in the shop via SaveManager.
    /// </summary>
    public class FatigueSystem : MonoBehaviour
    {
        public static FatigueSystem Instance { get; private set; }

        public float MaxFatigue { get; private set; } = 100f;
        public float CurrentFatigue { get; private set; } = 100f;

        [Header("Settings")]
        public float baseRecoveryRate = 5f; // Fatigue recovered per second
        public float recoveryDelayAfterCast = 1.5f; // Seconds to wait before recovering after a cast

        private float timeSinceLastCast = 0f;

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
            // Sync with SaveData if available
            if (SaveManager.Instance != null)
            {
                MaxFatigue = SaveManager.Instance.CurrentSave.maxFatigue;
                CurrentFatigue = SaveManager.Instance.CurrentSave.currentFatigue;
            }
        }

        private void Update()
        {
            timeSinceLastCast += Time.deltaTime;

            if (timeSinceLastCast >= recoveryDelayAfterCast && CurrentFatigue < MaxFatigue)
            {
                CurrentFatigue += baseRecoveryRate * Time.deltaTime;
                if (CurrentFatigue > MaxFatigue)
                {
                    CurrentFatigue = MaxFatigue;
                }
            }
        }

        public bool CanCast(float cost)
        {
            return CurrentFatigue >= cost;
        }

        public void ConsumeFatigue(float cost)
        {
            CurrentFatigue -= cost;
            if (CurrentFatigue < 0) CurrentFatigue = 0;
            timeSinceLastCast = 0f;
            
            // Sync to save memory
            if(SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentSave.currentFatigue = (int)CurrentFatigue;
            }
        }

        public void UpgradeMaxFatigue(int amount)
        {
            MaxFatigue += amount;
            CurrentFatigue += amount;
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentSave.maxFatigue = (int)MaxFatigue;
                SaveManager.Instance.CurrentSave.currentFatigue = (int)CurrentFatigue;
            }
            Debug.Log($"[FatigueSystem] Max Fatigue upgraded to {MaxFatigue}");
        }
    }
}
