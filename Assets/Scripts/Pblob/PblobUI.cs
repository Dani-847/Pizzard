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
            
            // Suscribirse al evento de cambio de salud (con verificación de null)
            if (boss.OnHealthChanged != null)
            {
                boss.OnHealthChanged.AddListener(UpdateHealthBar);
            }
            
            // Inicializar con salud completa
            UpdateHealthBar(1f);
        }
        else
        {
            Debug.LogWarning("⚠️ PblobUI: PblobController no encontrado en la escena");
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse del evento al destruirse para evitar memory leaks
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
}