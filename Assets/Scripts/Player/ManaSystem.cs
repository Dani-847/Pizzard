using System.Collections.Generic;
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

        public float MaxMana { get; private set; } = 100f;
        public float CurrentMana { get; private set; } = 100f;

        [Header("Settings")]
        public float baseRecoveryRate = 5f; // Mana recovered per second
        public float recoveryDelayAfterCast = 1.5f; // Seconds to wait before recovering after a cast

        private float timeSinceLastCast = 0f;

        // ==============================
        // HARDCODED SPELL MANA COSTS
        // Edit these values to balance the game.
        // ==============================
        public static readonly Dictionary<string, float> SpellCosts = new Dictionary<string, float>
        {
            // Tier 1 — Single element (cheap)
            { "queso", 10f },
            { "pepperoni", 12f },
            { "piña", 11f },

            // Tier 2 — Two elements (moderate, order matters)
            { "queso|pepperoni", 20f },
            { "queso|piña", 18f },
            { "pepperoni|queso", 22f },
            { "pepperoni|piña", 20f },
            { "piña|queso", 19f },
            { "piña|pepperoni", 21f },

            // Tier 3 — Three elements (expensive, order matters)
            // Default fallback: 30f for any combo not listed
            { "queso|pepperoni|piña", 30f },
            { "queso|piña|pepperoni", 28f },
            { "pepperoni|queso|piña", 32f },
            { "pepperoni|piña|queso", 30f },
            { "piña|queso|pepperoni", 29f },
            { "piña|pepperoni|queso", 31f },
            { "pepperoni|pepperoni|pepperoni", 35f },
            { "queso|queso|piña", 25f },
            { "queso|queso|queso", 28f },
            { "piña|piña|piña", 30f },
        };

        /// <summary>
        /// Look up the mana cost for a given combo key.
        /// Returns 30f as default if the combo isn't in the dictionary.
        /// </summary>
        public static float GetSpellCost(string comboKey)
        {
            return SpellCosts.TryGetValue(comboKey, out float cost) ? cost : 30f;
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
            MaxMana = oldMax * 1.5f;
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
