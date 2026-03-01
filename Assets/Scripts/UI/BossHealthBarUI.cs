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

void Awake()
    {
        // Auto-create text labels as children if not wired in Inspector.
        // We defer to Start so FindObjectOfType<TextMeshProUGUI> can find scene TMPs for the font.
    }

private TMPro.TextMeshProUGUI CreateLabel(string objName, Vector2 anchoredPos, float fontSize)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        // Stretch full width, anchor to vertical center, pivot at center
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(4f, 0f);
        rt.offsetMax = new Vector2(-4f, 0f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(0f, 22f);

        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();

        // Grab font from any TMP in the scene (including inactive)
        var anyTmp = FindObjectOfType<TMPro.TextMeshProUGUI>(true);
        if (anyTmp != null && anyTmp != tmp) tmp.font = anyTmp.font;

        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontStyle = TMPro.FontStyles.Bold;

        Debug.Log($"[BossHealthBarUI] Created label '{objName}' font={(tmp.font != null ? tmp.font.name : "NULL")}");
        return tmp;
    }

void Start() { }

void OnEnable()
    {
        // Create labels here (not Start) — NiggelBossUI starts inactive, so Start
        // may fire inside BossArena_2 when no TMPs are yet active.
        // OnEnable fires every activation; guard with null check.
        if (bossNameText == null)
            bossNameText = CreateLabel("BossNameText", new Vector2(0f, 14f), 16);
        if (healthText == null)
            healthText   = CreateLabel("HealthText",   new Vector2(0f, -8f), 13);

        // Find the active boss in the scene
        trackedBoss = FindObjectOfType<BossBase>();
        if (trackedBoss != null)
        {
            UpdateBossName();
            UpdateBar();
        }
        else
        {
            Debug.LogWarning("[BossHealthBarUI] No BossBase found in scene.");
        }

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += UpdateBossName;
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
        if (trackedBoss == null || bossNameText == null) return;

        // 1. Explicit localization key set in Inspector
        if (!string.IsNullOrEmpty(bossLocalizationKey) && LocalizationManager.Instance != null)
        {
            string loc = LocalizationManager.Instance.GetText(bossLocalizationKey);
            if (!loc.StartsWith("[")) { bossNameText.text = loc; return; }
        }

        // 2. Auto-detect boss type → pick localization key
        string autoKey = null;
        if (trackedBoss is NiggelController) autoKey = "boss_niggel";
        // add other boss types here as needed

        if (autoKey != null && LocalizationManager.Instance != null)
        {
            string loc = LocalizationManager.Instance.GetText(autoKey);
            if (!loc.StartsWith("[")) { bossNameText.text = loc; return; }
        }

        // 3. Fallback: GO name
        bossNameText.text = trackedBoss.gameObject.name;
    }

        void Update()
        {
            // Retry finding the boss each frame if not found yet (boss scene may load after OnEnable)
            if (trackedBoss == null)
            {
                trackedBoss = FindObjectOfType<BossBase>();
                if (trackedBoss != null)
                {
                    UpdateBossName();
                }
            }

            if (trackedBoss != null)
                UpdateBar();
        }

        void UpdateBar()
        {
            if (trackedBoss == null) return;
            
            int maxH = trackedBoss.MaxHealthPublic;
            float percent = maxH > 0 
                ? (float)trackedBoss.CurrentHealthPublic / maxH 
                : 0f;

            if (healthFillBar != null)
                healthFillBar.fillAmount = percent;

            if (healthText != null)
                healthText.text = $"{trackedBoss.CurrentHealthPublic}/{maxH}";
        }
    

void OnGUI() { }
}
}
