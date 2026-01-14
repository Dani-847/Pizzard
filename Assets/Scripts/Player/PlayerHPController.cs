//PlayerHPController.cs

using UnityEngine;

// Gestiona la lógica de la vida del personaje, comunica los cambios a la UI 
// y controla el flujo de muerte
public class PlayerHPController : MonoBehaviour
{
    [Header("Configuración de vida")]
    // 6 medios corazones = 3 corazones completos
    public int vidaMaxima = 6;
    // Valor actual de vida del personaje (0 a 6)
    public int vidaActual;
    
    [Header("Referencias")]
    // Referencia al script que gestiona la UI
    public CharacterHPUI hpUI;
    // Referencia a la pantalla de muerte
    public DeathUI deathUI;
    // Sistema de pociones vinculado al jugador
    public HealthPotionSystem potionSystem;

    // Inicializa vidaActual y actualiza la UI
    void Start()
    {
        vidaActual = vidaMaxima;
        hpUI.ActualizarUI(vidaActual);
        potionSystem.playerHP = this; // Enlazar automáticamente si no se hizo en inspector
    }

    // Resta vida según la cantidad de daño (normalmente 1 = medio corazón)
    // Actualiza la UI
    // Si llega a 0, llama a OnDeath()
    public void RecibirDaño(int cantidad)
    {
        if (EstaMuerto()) return;

        Debug.Log($"❤️ Vida antes del daño: {vidaActual}");
        vidaActual -= cantidad;
        Debug.Log($"❤️ Vida antes del daño: {vidaActual}");
        vidaActual = Mathf.Max(vidaActual, 0);
        hpUI.ActualizarUI(vidaActual);

        if (vidaActual <= 0)
            OnDeath();
    }

    // Suma vida actual sin pasar de vidaMaxima
    // Actualiza la UI
    public void RestaurarVida(int cantidad)
    {
        if (EstaMuerto()) return;

        vidaActual += cantidad;
        vidaActual = Mathf.Min(vidaActual, vidaMaxima);
        hpUI.ActualizarUI(vidaActual);
    }

    // Controla el estado de muerte:
    // - Bloquear movimiento y acciones
    // - Mostrar DeathUI
    public void OnDeath()
    {
        deathUI.MostrarPantallaMuerte();
    }

    // Devuelve true si vidaActual <= 0
    public bool EstaMuerto()
    {
        return vidaActual <= 0;
    }
}
