using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Pizzard.Bosses;
using Pizzard.Core;

public class NiggelUI : MonoBehaviour
{
    [Header("Health Bar System")]
    public DelayedHealthBar healthBar;
    
    [Header("Optional UI Elements")]
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI coinVaultText; // Added for textual HP feedback
    
    private NiggelController boss;
    private float maxHealth;
    
void OnEnable()
    {
        // Auto-create labels if not wired in Inspector
        if (bossNameText == null) bossNameText = MakeLabel("BossNameText", new Vector2(0f, 30f), 18);
        if (coinVaultText == null) coinVaultText = MakeLabel("CoinVaultText", new Vector2(0f, 0f), 14);

        boss = FindObjectOfType<NiggelController>();
        if (boss != null)
        {
            maxHealth = GameBalance.Bosses.Niggel.CoinVaultMax;
            if (bossNameText != null)
            {
                string name = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("boss_niggel") : "Niggel";
                bossNameText.text = name.StartsWith("[") ? "Niggel" : name;
            }
        }
    }

private TextMeshProUGUI MakeLabel(string goName, Vector2 anchoredPos, float fontSize)
    {
        var go = new GameObject(goName);
        // Parent to root Canvas so position is screen-relative and easy to move
        var canvas = GetComponentInParent<Canvas>(true);
        Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
        go.transform.SetParent(rootCanvas != null ? rootCanvas.transform : transform, false);
        var rt = go.AddComponent<RectTransform>();
        // Anchor to screen center
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 30f);
        rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (rootCanvas != null)
        {
            foreach (var t in rootCanvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t != tmp && t.font != null) { tmp.font = t.font; break; }
        }
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        Debug.Log($"[NiggelUI] Created '{goName}' font={(tmp.font != null ? tmp.font.name : "NULL")}");
        return tmp;
    }

    void Start()
    {
        if (boss == null)
            boss = FindObjectOfType<NiggelController>();
        if (boss != null)
            maxHealth = GameBalance.Bosses.Niggel.CoinVaultMax;
        // DelayedHealthBar.Start() automatically initializes visuals
    }
    
    void Update()
    {
        // Re-find boss if it spawned after this UI initialized
        if (boss == null)
        {
            boss = FindObjectOfType<NiggelController>();
            if (boss != null) maxHealth = GameBalance.Bosses.Niggel.CoinVaultMax;
        }

        // Poll boss HP every frame
        if (boss != null && maxHealth > 0f)
        {
            if (healthBar != null)
                healthBar.SetHealth((float)boss.CurrentHealthPublic / maxHealth);
            
            if (coinVaultText != null)
                coinVaultText.text = $"CoinVault: {boss.CurrentHealthPublic}";
        }
    }
}
