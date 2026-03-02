using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class PinaQuesoQuesoAttack : PineappleCheeseProjectile
{
    public float coneRadius = GameBalance.Spells.PinaQuesoQueso.ConeRadius;
    public float coneHalfAngle = GameBalance.Spells.PinaQuesoQueso.ConeHalfAngle;
    public float coneDamage = GameBalance.Spells.PinaQuesoQueso.ConeDamage;

    private Vector2 travelDir = Vector2.right;

    protected override void Start()
    {
        base.Start();
        if (direction != Vector2.zero)
            travelDir = direction.normalized;
    }

    protected override void Update()
    {
        if (!hasImpacted && rb != null && rb.velocity.sqrMagnitude > 0.1f)
        {
            travelDir = rb.velocity.normalized;
        }

        base.Update();
    }

    protected override void ApplyImpactDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, coneRadius);

        foreach (var c in hits)
        {
            if (c.CompareTag("Enemy") || c.CompareTag("Boss"))
            {
                Vector2 toTarget = (c.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(travelDir, toTarget);

                if (angle <= coneHalfAngle)
                {
                    float totalDamage = coneDamage + (absorbedProjectiles.Count * 2f);

                    var boss = c.GetComponent<PblobController>();
                    if (boss != null) boss.TakeDamage(totalDamage);
                    
                    var bossBase = c.GetComponent<BossBase>();
                    if (bossBase != null) bossBase.TakeDamage((int)totalDamage);

                    Rigidbody2D r = c.GetComponent<Rigidbody2D>();
                    if (r != null)
                    {
                        r.AddForce(toTarget * (totalDamage * 0.5f), ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}
