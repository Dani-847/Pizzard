using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class PinaQuesoPinaAttack : PineappleCheeseProjectile
{
    public float damagePerAbsorbed = GameBalance.Spells.PinaQuesoPina.DamagePerAbsorbed;

    protected override void ApplyImpactDamage()
    {
        float totalDamage = (absorbedProjectiles.Count == 0) ? impactDamage : (absorbedProjectiles.Count * damagePerAbsorbed);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Enemy") || c.CompareTag("Boss"))
            {
                var boss = c.GetComponent<PblobController>();
                if (boss != null) boss.TakeDamage(totalDamage);
                
                var bossBase = c.GetComponent<BossBase>();
                if (bossBase != null) bossBase.TakeDamage((int)totalDamage);

                Rigidbody2D r = c.GetComponent<Rigidbody2D>();
                if (r != null)
                {
                    Vector2 force = (c.transform.position - transform.position).normalized;
                    r.AddForce(force * (totalDamage * 0.5f), ForceMode2D.Impulse);
                }
            }
        }
    }
}
