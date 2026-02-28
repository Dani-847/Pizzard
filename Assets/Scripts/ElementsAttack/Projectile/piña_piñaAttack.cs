using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class piña_piñaAttack : CharacterProjectile
{
    [Header("Subproyectiles")]
    public GameObject smallProjectilePrefab;
    public float smallProjectileSpeed = Pizzard.Core.GameBalance.Spells.PinaPina.SmallProjectileSpeed;
    public float smallProjectileDamageMultiplier = Pizzard.Core.GameBalance.Spells.PinaPina.SmallProjectileDamageMultiplier;
    public float smallProjectileLifetime = Pizzard.Core.GameBalance.Spells.PinaPina.SmallProjectileLifetime;
    
    [Header("Spawn Settings")]
    public float spawnOffset = Pizzard.Core.GameBalance.Spells.PinaPina.SpawnOffset;
    
    private bool hasSpawned = false;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🍍 Piña colisionó con: {other.name} (Tag: {other.tag})");

        // ✅ EVITAR colisiones con el jugador y sus partes
        if (other.CompareTag("Player") || other.CompareTag("CastPoint") || other.CompareTag("Weapon") || other.CompareTag("CharacterProjectile"))
        {
            Debug.Log($"🛡️ Piña ignorada por: {other.tag}");
            return;
        }

        // ✅ GENERAR SUBPROYECTILES solo una vez cuando impacte con algo que no sea el jugador
        if (!hasSpawned)
        {
            hasSpawned = true;
            Debug.Log("🌀 Piña impactó - generando subproyectiles");
            SpawnDiagonalProjectiles();
        }

        // ✅ LUEGO: Llamar a la lógica base para el daño y destrucción
        base.OnTriggerEnter2D(other);
    }

    private void SpawnDiagonalProjectiles()
    {
        if (smallProjectilePrefab == null) 
        {
            Debug.LogError("❌ No hay prefab asignado para subproyectiles de piña");
            return;
        }

        Vector2[] directions =
        {
            new Vector2(1, 1).normalized,
            new Vector2(1, -1).normalized,
            new Vector2(-1, 1).normalized,
            new Vector2(-1, -1).normalized
        };

        int spawnedCount = 0;

        foreach (var dir in directions)
        {
            Vector2 spawnPosition = (Vector2)transform.position + (dir * spawnOffset);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            GameObject proj = Instantiate(smallProjectilePrefab, spawnPosition, rotation);

            // Configurar como CharacterProjectile
            SmallPiñaProjectile smallPiña = proj.GetComponent<SmallPiñaProjectile>();
            if (smallPiña != null)
            {
                smallPiña.damage = damage * smallProjectileDamageMultiplier;
                smallPiña.hasSpawned = true;
                
                Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = dir * smallProjectileSpeed;

                if (smallProjectileLifetime > 0)
                    Destroy(proj, smallProjectileLifetime);

                spawnedCount++;
            }
            else
            {
                Debug.LogWarning("⚠️ El prefab de subproyectil no tiene SmallPiñaProjectile");
                Destroy(proj);
            }
        }
        
        Debug.Log($"🌀 Piña generó {spawnedCount} subproyectiles");
    }
}