using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class SmallExplodingProjectile : CharacterProjectile
{
    [Header("Explosion Settings")]
    public float explosionRadius = GameBalance.Spells.PinaPinaPina.SubExplosionRadius;
    public float explosionDamage = GameBalance.Spells.PinaPinaPina.SubExplosionDamage;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Apply area damage before the base logic destroys the projectile
        Explode();
        base.OnTriggerEnter2D(other);
    }

    private void Explode()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Boss"))
            {
                BossBase boss = hit.GetComponent<BossBase>();
                if (boss == null) boss = hit.GetComponentInParent<BossBase>();

                if (boss != null)
                {
                    boss.TakeDamage((int)explosionDamage);
                }
                else
                {
                    PblobController pblob = hit.GetComponent<PblobController>();
                    if (pblob != null)
                    {
                        pblob.TakeDamage(explosionDamage);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
