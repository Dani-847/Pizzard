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

        private BossBase trackedBoss;
        private int cachedMaxHealth;

        void OnEnable()
        {
            // Find the active boss in the scene
            trackedBoss = FindObjectOfType<BossBase>();
            if (trackedBoss != null)
            {
                cachedMaxHealth = trackedBoss.MaxHealthPublic;
                if (bossNameText != null)
                    bossNameText.text = trackedBoss.gameObject.name;
                    
                UpdateBar();
            }
            else
            {
                Debug.LogWarning("[BossHealthBarUI] No BossBase found in scene.");
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
