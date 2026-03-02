using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class PinaQuesoPepperoniAttack : PineappleCheeseProjectile
{
    public float burnDurationPerStack = GameBalance.Spells.PinaQuesoPepperoni.BurnDurationPerStack;

    protected override void ApplyImpactDamage()
    {
        base.ApplyImpactDamage();

        int stacks = Mathf.Max(1, absorbedProjectiles.Count);
        float burnDuration = stacks * burnDurationPerStack;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Enemy") || c.CompareTag("Boss"))
            {
                StatusEffectSystem status = c.GetComponent<StatusEffectSystem>();
                if (status != null)
                {
                    status.ApplyEffect(StatusType.picante, burnDuration, gameObject, stacks);
                }
            }
        }
    }
}
