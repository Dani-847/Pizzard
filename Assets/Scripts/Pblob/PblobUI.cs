using UnityEngine;
using UnityEngine.UI;

public class PblobUI : MonoBehaviour
{
    [Header("Health Bar System")]
    public DelayedHealthBar healthBar;
    
    [Header("Optional UI Elements")]
    public Text healthText; // Para mostrar "1000/1000"
    public GameObject bossNameDisplay;
    
    private PblobController boss;
    private float maxHealth;
    
    void Start()
    {
        boss = FindObjectOfType<PblobController>();
        if (boss != null)
        {
            maxHealth = boss.maxHealth;
            boss.OnHealthChanged.AddListener(UpdateHealthBar);
            
            // Inicializar con salud completa
            UpdateHealthBar(1f);
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
}