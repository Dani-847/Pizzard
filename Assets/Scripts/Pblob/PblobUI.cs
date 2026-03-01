using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PblobUI : MonoBehaviour
{
    [Header("Health Bar System")]
    public DelayedHealthBar healthBar;
    
    [Header("Optional UI Elements")]
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI phase2TimerText;
    
    private PblobController boss;
    private float maxHealth;
    
    void OnEnable()
    {
        boss = FindObjectOfType<PblobController>();
        if (boss != null)
        {
            maxHealth = boss.maxHealth;
            
            if (boss.OnHealthChanged != null)
            {
                boss.OnHealthChanged.AddListener(UpdateHealthBar);
            }
        }
    }

    void Start()
    {
        if (boss == null)
            boss = FindObjectOfType<PblobController>();
        if (boss != null)
            maxHealth = boss.maxHealth;
        // Note: DelayedHealthBar.Start() builds bars and sets them full — no call needed here
    }
    
    void OnDisable()
    {
        if (boss != null && boss.OnHealthChanged != null)
        {
            boss.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
    
    void UpdateHealthBar(float healthPercentage)
    {
        if (healthBar != null)
            healthBar.SetHealth(healthPercentage);
    }
    
    void Update()
    {
        // Re-find boss if it spawned after this UI initialized
        if (boss == null)
        {
            boss = FindObjectOfType<PblobController>();
            if (boss != null) maxHealth = boss.maxHealth;
        }

        // Poll boss HP every frame — same pattern as ManaUI
        if (boss != null && healthBar != null && maxHealth > 0f)
            healthBar.SetHealth(boss.CurrentHealth / maxHealth);

        if (boss != null && phase2TimerText != null)
        {
            if (boss.IsPhase2TimerActive())
            {
                phase2TimerText.gameObject.SetActive(true);
                phase2TimerText.text = $"Mini-Game Timer: {boss.GetPhase2TimeRemaining():F1}s";
            }
            else
            {
                phase2TimerText.gameObject.SetActive(false);
            }
        }
    }
}