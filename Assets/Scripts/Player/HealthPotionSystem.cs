//HealthPotionSystem.cs

using UnityEngine;
using UnityEngine.InputSystem;

// Gestionar las pociones del jugador: cuántas tiene, 
// cuántas puede tener como máximo, cuándo se recargan y cuándo se usan.
public class HealthPotionSystem : MonoBehaviour
{
    [Header("Configuración de pociones")]
    // Múmero máximo actual (puede subir en tienda)
    public int maxPociones = Pizzard.Core.GameBalance.Potions.StartingMax;
    // Cuántas tiene disponibles
    public int pocionesActuales;
    // Medios corazones curados (2 = 1 corazón)
    public int curacionPorPocion = Pizzard.Core.GameBalance.Potions.HealPerPotion;

    [Header("Referencias")]
    public PlayerHPController playerHP;

    // Inicializa pocionesActuales = maxPociones al comenzar
    void Start()
    {
        pocionesActuales = maxPociones;
    }

    // Si hay pociones disponibles y el jugador no tiene vida llena:
    // - Resta 1 a pocionesActuales
    // - Llama a playerHP.RestaurarVida(curacionPorPocion)
    // - (opcional) Actualiza la UI de pociones, si existe
    public void UsarPocion()
    {
        if (!TienePociones() || playerHP.vidaActual == playerHP.vidaMaxima)
            return;

        pocionesActuales--;
        playerHP.RestaurarVida(curacionPorPocion);
        //! Aquí se podría actualizar una UI de pociones más adelante
    }

    // Se llama al inicio y fin de cada bossfight
    // Restaura pocionesActuales = maxPociones
    public void RecargarPociones()
    {
        pocionesActuales = maxPociones; // Recarga todas las pociones si se quiren recargar solo una se tendria que implementar otra funcion
    }

    // Se llama desde la tienda para subir el límite máximo de pociones
    // Ejemplo: maxPociones++ (hasta un límite)
    // Recarga también las pociones al subir capacidad
    public void MejorarCapacidad()
    {
        maxPociones++;
        Debug.Log("Pociones mejoradas a " + maxPociones);
        RecargarPociones();
        
        // --- WAVE 2: Sync to SaveData ---
        if (Pizzard.Progression.SaveManager.Instance != null)
        {
            Pizzard.Progression.SaveManager.Instance.CurrentSave.potionCount = maxPociones;
            Pizzard.Progression.SaveManager.Instance.SaveGame();
        }
    }
    
    public void onSelectPotion(InputAction.CallbackContext context)
    {
        if (pocionesActuales == 0)
            return;
        if (context.performed)
            UsarPocion();
    }

    // Devuelve true si pocionesActuales > 0
    public bool TienePociones()
    {
        return pocionesActuales > 0;
    }
    
    // Devuelve el número actual de pociones
    public int GetPocionesActuales()
    {
        return pocionesActuales;
    }
}