using UnityEngine;
using UnityEngine.UI;

public class PblobUI : MonoBehaviour
{
    [Header("Health Bar System")]
    public DelayedHealthBar healthBar;
    
    [Header("Optional UI Elements")]
    public Text healthText; // Para mostrar "1000/1000"
    public GameObject bossNameDisplay;
    public Text phase2TimerText; // Reference to the Phase 2 text under the Boss Bar
    
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
            
            UpdateHealthBar(1f);
        }
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
        // Actualizar el sistema de doble barra
        if (healthBar != null)
        {
            healthBar.SetHealth(healthPercentage);
        }
        
        // Actualizar texto de salud (opcional)
        if (healthText != null)
        {
            int currentHP = Mathf.RoundToInt(healthPercentage * maxHealth);
            healthText.text = $"{currentHP}/{maxHealth}";
        }
    }
    
    void Update()
    {
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