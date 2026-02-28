using UnityEngine;
using Pizzard.Progression;

namespace Pizzard.Core
{
    /// <summary>
    /// Manages the player's Mana (renamed from Elemental Fatigue).
    /// Spells consume mana. Mana slowly recovers over time.
    /// Capacity can be upgraded in the shop via SaveManager.
    /// </summary>
    public class ManaSystem : MonoBehaviour
    {
        public static ManaSystem Instance { get; private set; }

        public float MaxMana { get; private set; } = GameBalance.Mana.MaxMana;
        public float CurrentMana { get; private set; } = GameBalance.Mana.MaxMana;

        [Header("Settings")]
        public float baseRecoveryRate = GameBalance.Mana.BaseRecoveryRate;
        public float recoveryDelayAfterCast = GameBalance.Mana.RecoveryDelayAfterCast;

        private float timeSinceLastCast = 0f;

        /// <summary>
        /// Look up the mana cost for a given combo key via GameBalance.
        /// </summary>
        public static float GetSpellCost(string comboKey)
        {
            return GameBalance.Mana.GetSpellCost(comboKey);
        }

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
            // Initialized by SaveManager calling LoadMaxManaFromSave
        }

        public void LoadMaxManaFromSave(float savedMax)
        {
            MaxMana = savedMax;
            CurrentMana = MaxMana; // Reset on load
            Debug.Log($"[ManaSystem] Loaded max mana: {MaxMana}");
        }

        private void Update()
        {
            timeSinceLastCast += Time.deltaTime;

            if (timeSinceLastCast >= recoveryDelayAfterCast && CurrentMana < MaxMana)
            {
                CurrentMana += baseRecoveryRate * Time.deltaTime;
                if (CurrentMana > MaxMana)
                {
                    CurrentMana = MaxMana;
                }
            }
        }

        public bool CanCast(float cost)
        {
            return CurrentMana >= cost;
        }

        public void ConsumeMana(float cost)
        {
            CurrentMana -= cost;
            if (CurrentMana < 0) CurrentMana = 0;
            timeSinceLastCast = 0f;
        }

        public void UpgradeMaxMana(int amount)
        {
            // Upgrading preserves the percentage of mana remaining
            float oldMax = MaxMana;
            MaxMana = oldMax * GameBalance.Mana.UpgradeMultiplier;
            CurrentMana = (CurrentMana / oldMax) * MaxMana;
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentSave.manaMax = MaxMana;
                SaveManager.Instance.SaveGame();
            }
            Debug.Log($"[ManaSystem] Max Mana upgraded from {oldMax} to {MaxMana}");
        }
    }
}
