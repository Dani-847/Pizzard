using UnityEngine;

public class ElementsRegister : MonoBehaviour
{
    [Header("Referencias")]
    public CombinationDatabase database;
    public CombinationsUI combinationsUI;

    public void RegistrarCombinacion(string combinationKey)
    {
        // Buscar si existe la entrada en la database
        var entry = database.GetByKey(combinationKey);

        if (entry == null)
        {
            Debug.LogError("❌ Combinación no encontrada en database: " + combinationKey);
            return;
        }

        // Si ya estaba desbloqueada, no hacer nada
        if (entry.isUnlocked)
        {
            Debug.Log("ℹ️ Combinación ya estaba desbloqueada: " + combinationKey);
            return;
        }

        // Marcar como desbloqueada
        entry.isUnlocked = true;
        Debug.Log("✅ Combinación desbloqueada: " + combinationKey);

        // Notificar a la UI para que actualice
        if (combinationsUI != null)
        {
            combinationsUI.MarcarComoDescubierta(combinationKey);
        }

        // Guardar el estado en PlayerPrefs
        PlayerPrefs.SetInt("Combination_" + combinationKey, 1);
        PlayerPrefs.Save();

        Debug.Log("💾 Combinación guardada: " + combinationKey);
    }

    // Método para cargar combinaciones desbloqueadas al inicio
    public void CargarCombinacionesDesbloqueadas()
    {
        foreach (var entry in database.GetAll())
        {
            string key = "Combination_" + entry.combinationKey; // ⬅️ CORREGIDO: entry.combinationKey
            if (PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) == 1)
            {
                entry.isUnlocked = true;
                Debug.Log("📂 Combinación cargada: " + entry.combinationKey); // ⬅️ CORREGIDO: entry.combinationKey
            }
        }
    }

    void Start()
    {
        CargarCombinacionesDesbloqueadas();
    }
}