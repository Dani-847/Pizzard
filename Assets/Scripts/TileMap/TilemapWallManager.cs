using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapWallManager : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap wallTilemap;
    
    [Header("Wall Properties")]
    public bool blocksPlayers = true;
    public bool reflectsProjectiles = true;
    public float bounceEffectiveness = 1.0f;

    void Start()
    {
        // Obtener referencia automáticamente si no está asignada
        if (wallTilemap == null)
            wallTilemap = GetComponent<Tilemap>();
        
        Debug.Log($"✅ Tilemap Wall configurado: {gameObject.name}");
        Debug.Log($"   - Bloques jugador: {blocksPlayers}");
        Debug.Log($"   - Rebota proyectiles: {reflectsProjectiles}");
    }

    // Método para verificar si una posición tiene tile de pared
    public bool IsWallAtPosition(Vector3 worldPosition)
    {
        if (wallTilemap == null) return false;
        
        Vector3Int cellPosition = wallTilemap.WorldToCell(worldPosition);
        return wallTilemap.HasTile(cellPosition);
    }

    // Para debugging visual
    void OnDrawGizmosSelected()
    {
        if (wallTilemap != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Bounds bounds = wallTilemap.localBounds;
            Gizmos.DrawCube(wallTilemap.transform.position + bounds.center, bounds.size);
        }
    }
}