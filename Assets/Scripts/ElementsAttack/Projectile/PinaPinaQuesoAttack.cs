using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;

public class PinaPinaQuesoAttack : CharacterProjectile
{
    [Header("Subproyectiles")]
    public GameObject smallAbsorbingPrefab;
    public float smallProjectileSpeed = GameBalance.Spells.PinaPina.SmallProjectileSpeed;
    public float smallProjectileDamageMultiplier = GameBalance.Spells.PinaPina.SmallProjectileDamageMultiplier;
    public float smallProjectileLifetime = GameBalance.Spells.PinaPina.SmallProjectileLifetime;
    
    [Header("Spawn Settings")]
    public float spawnOffset = GameBalance.Spells.PinaPina.SpawnOffset;
    
    private bool hasSpawned = false;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("CastPoint") || other.CompareTag("Weapon") || other.CompareTag("CharacterProjectile"))
        {
            return;
        }

        if (!hasSpawned)
        {
            hasSpawned = true;
            SpawnDiagonalProjectiles();
        }

        base.OnTriggerEnter2D(other);
    }

    private void SpawnDiagonalProjectiles()
    {
        if (smallAbsorbingPrefab == null) return;

        Vector2[] directions =
        {
            new Vector2(1, 1).normalized,
            new Vector2(1, -1).normalized,
            new Vector2(-1, 1).normalized,
            new Vector2(-1, -1).normalized
        };

        foreach (var dir in directions)
        {
            Vector2 spawnPosition = (Vector2)transform.position + (dir * spawnOffset);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            GameObject proj = Instantiate(smallAbsorbingPrefab, spawnPosition, rotation);

            CharacterProjectile smallProj = proj.GetComponent<CharacterProjectile>();
            if (smallProj != null)
            {
                smallProj.damage = damage * smallProjectileDamageMultiplier;
                
                Rigidbody2D rbd = proj.GetComponent<Rigidbody2D>();
                if (rbd != null) rbd.velocity = dir * smallProjectileSpeed;

                if (smallProjectileLifetime > 0) Destroy(proj, smallProjectileLifetime);
            }
        }
    }
}
