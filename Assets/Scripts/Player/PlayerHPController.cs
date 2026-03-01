//PlayerHPController.cs

using UnityEngine;
using Pizzard.Core;
using static Pizzard.Core.GameBalance;

/// <summary>
/// Gestiona la lógica de la vida del personaje, comunica los cambios a la UI 
/// y controla el flujo de muerte.
/// </summary>
public class PlayerHPController : MonoBehaviour
{
    [Header("Configuración de vida")]
    [Tooltip("6 medios corazones = 3 corazones completos")]
    public int vidaMaxima = GameBalance.Player.MaxHP;
    [Tooltip("Valor actual de vida del personaje (0 a 6)")]
    public int vidaActual;
    
    [Header("Referencias")]
    [Tooltip("Referencia al script que gestiona la UI")]
    public CharacterHPUI hpUI;
    [Tooltip("Referencia a la pantalla de muerte (opcional, usa GameFlowManager si no está asignada)")]
    public DeathUI deathUI;
    [Tooltip("Sistema de pociones vinculado al jugador")]
    public HealthPotionSystem potionSystem;
    [Tooltip("SpriteRenderer del jugador para efectos de daño")]
    public SpriteRenderer playerSprite;

    /// <summary>
    /// Inicializa vidaActual y actualiza la UI.
    /// </summary>
    void Start()
    {
        if (hpUI == null)
            hpUI = FindObjectOfType<CharacterHPUI>(true);

        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        vidaActual = vidaMaxima;
        if (hpUI != null)
            hpUI.ActualizarUI(vidaActual);
        if (potionSystem != null)
            potionSystem.playerHP = this;
    }

    private float invulnerabilityTimer = 0f;
    private float invulnerabilityDuration = GameBalance.Player.InvulnerabilityDuration;

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Aplica daño ignorando el timer de invulnerabilidad.
    /// </summary>
    public void ForceDamage(int cantidad)
    {
        float oldTimer = invulnerabilityTimer;
        invulnerabilityTimer = 0f;
        RecibirDaño(cantidad);
        invulnerabilityTimer = oldTimer;
    }

    /// <summary>
    /// Resta vida según la cantidad de daño (normalmente 1 = medio corazón).
    /// Actualiza la UI. Si llega a 0, llama a OnDeath().
    /// </summary>
    public void RecibirDaño(int cantidad)
    {
        if (EstaMuerto() || invulnerabilityTimer > 0f) return;

        Debug.Log($"❤️ Vida antes del daño: {vidaActual}");
        vidaActual -= cantidad;
        Debug.Log($"❤️ Vida después del daño: {vidaActual}");
        vidaActual = Mathf.Max(vidaActual, 0);
        
        invulnerabilityTimer = invulnerabilityDuration;
        StartCoroutine(DamageFlashRoutine());

        if (hpUI != null)
            hpUI.ActualizarUI(vidaActual);

        if (vidaActual <= 0)
            OnDeath();
    }

    private System.Collections.IEnumerator DamageFlashRoutine()
    {
        if (playerSprite == null) yield break;
        Color originalColor = Color.white;
        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            playerSprite.color = (Mathf.FloorToInt(elapsed * 10f) % 2 == 0) ? Color.yellow : originalColor;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerSprite.color = originalColor;
    }

    /// <summary>
    /// Suma vida actual sin pasar de vidaMaxima.
    /// Actualiza la UI.
    /// </summary>
    public void RestaurarVida(int cantidad)
    {
        if (EstaMuerto()) return;

        vidaActual += cantidad;
        vidaActual = Mathf.Min(vidaActual, vidaMaxima);
        
        if (hpUI != null)
            hpUI.ActualizarUI(vidaActual);
    }

    /// <summary>
    /// Restaura la vida al máximo. Usado al reiniciar el combate.
    /// </summary>
    public void RestaurarVidaCompleta()
    {
        vidaActual = vidaMaxima;
        if (hpUI != null)
            hpUI.ActualizarUI(vidaActual);
    }

    /// <summary>
    /// Controla el estado de muerte:
    /// - Bloquear movimiento y acciones
    /// - Mostrar DeathUI o notificar a GameFlowManager
    /// </summary>
    public void OnDeath()
    {
        // Notificar a GameFlowManager si existe
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnPlayerDeath();
        }
        else if (deathUI != null)
        {
            // Fallback: usar DeathUI directamente
            deathUI.MostrarPantallaMuerte();
        }
    }

    /// <summary>
    /// Devuelve true si vidaActual <= 0.
    /// </summary>
    public bool EstaMuerto()
    {
        return vidaActual <= 0;
    }
}
