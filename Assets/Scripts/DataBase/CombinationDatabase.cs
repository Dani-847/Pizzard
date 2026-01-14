using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CombinationEntry
{
    // Ej: "fuego|agua|tierra" (se recomienda guardar en minúsculas)
    public string combinationKey;

    // Nombre visible de la combinación
    public string combinationName;

    // Sprite del resultado final
    public Sprite resultSprite;

    // Sprites de los elementos base (ordenados)
    public Sprite[] baseElementSprites;

    // Descripción de la combinación
    [TextArea] public string description;

    // Si esta combinación ya fue descubierta por el jugador
    public bool isUnlocked;

    // --- Nuevos campos para ejecutar ataques basados en datos ---
    // Prefab que define el comportamiento del ataque (projectil, shield, etc.)
    public GameObject attackPrefab;
}

[CreateAssetMenu(menuName = "Pizzard/CombinationDatabase")]
public class CombinationDatabase : ScriptableObject
{
    public List<CombinationEntry> combinations = new List<CombinationEntry>();

    // Buscar combinación por clave (case-insensitive)
    public CombinationEntry GetByKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        return combinations.Find(c =>
            string.Equals(c.combinationKey, key, System.StringComparison.OrdinalIgnoreCase)
        );
    }

    // Devolver todas
    public List<CombinationEntry> GetAll()
    {
        return combinations;
    }
}
