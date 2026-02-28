using UnityEngine;
using UnityEngine.UI;
using Pizzard.Bosses;

namespace Pizzard.UI
{
    /// <summary>
    /// Generic, reusable Boss health bar UI.
    /// Attaches to any boss scene canvas and polls BossBase.
    /// Works for Bosses 1-4 without needing boss-specific scripts.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("Health Bar")]
        [Tooltip("Filled Image component (set fill type to Horizontal or Vertical)")]
        public Image healthFillBar;
        
        [Header("Optional")]
        public TMPro.TextMeshProUGUI healthText;
        public TMPro.TextMeshProUGUI bossNameText;
        [SerializeField] private string bossLocalizationKey = "";

        private BossBase trackedBoss;
        private int cachedMaxHealth;

        void OnEnable()
        {
            // Find the active boss in the scene
            trackedBoss = FindObjectOfType<BossBase>();
            if (trackedBoss != null)
            {
                cachedMaxHealth = trackedBoss.MaxHealthPublic;
                UpdateBossName();
                UpdateBar();
            }
            else
            {
                Debug.LogWarning("[BossHealthBarUI] No BossBase found in scene.");
            }

            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += UpdateBossName;
            }
        }

        void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= UpdateBossName;
            }
        }

        void UpdateBossName()
        {
            if (trackedBoss != null && bossNameText != null)
            {
                string displayName = trackedBoss.gameObject.name;
                if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(bossLocalizationKey))
                {
                    string loc = LocalizationManager.Instance.GetText(bossLocalizationKey);
                    if (!loc.StartsWith("[")) displayName = loc;
                }
                bossNameText.text = displayName;
            }
        }

        void Update()
        {
            if (trackedBoss != null)
            {
                UpdateBar();
            }
        }

        void UpdateBar()
        {
            if (trackedBoss == null) return;
            
            float percent = cachedMaxHealth > 0 
                ? (float)trackedBoss.CurrentHealthPublic / cachedMaxHealth 
                : 0f;

            if (healthFillBar != null)
                healthFillBar.fillAmount = percent;

            if (healthText != null)
                healthText.text = $"{trackedBoss.CurrentHealthPublic}/{cachedMaxHealth}";
        }
    }
}
