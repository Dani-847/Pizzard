using UnityEngine;
using System.Collections;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoPinaPepperoniPillar : MonoBehaviour
{
    [Header("Settings")]
    public float duration = GameBalance.Spells.QuesoPinaPepperoni.Duration;
    public float tickDamage = GameBalance.Spells.QuesoPinaPepperoni.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoPinaPepperoni.TickInterval;
    public float radius = GameBalance.Spells.QuesoPinaPepperoni.Radius;
    public float burnDuration = GameBalance.Spells.QuesoPinaPepperoni.BurnDuration;
    public int burnStacksPerTick = GameBalance.Spells.QuesoPinaPepperoni.BurnStacksPerTick;
    public float bonusDamagePerStack = GameBalance.Spells.QuesoPinaPepperoni.BonusDamagePerStack;

    public static GameObject CurrentActivePillar;

    private void Start()
    {
        if (CurrentActivePillar != null)
        {
            Destroy(CurrentActivePillar);
        }
        CurrentActivePillar = gameObject;

        StartCoroutine(DurationCoroutine());
        StartCoroutine(TickCoroutine());
    }

    private void OnDestroy()
    {
        if (CurrentActivePillar == gameObject)
        {
            CurrentActivePillar = null;
        }
    }

    private IEnumerator DurationCoroutine()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private IEnumerator TickCoroutine()
    {
        while (true)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("Boss") || collider.CompareTag("Enemy"))
                {
                    StatusEffectSystem status = collider.GetComponent<StatusEffectSystem>();
                    if (status == null) status = collider.GetComponentInParent<StatusEffectSystem>();
                    
                    int currentStacks = 0;
                    if (status != null)
                    {
                        // Se aplica la quemadura
                        status.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacksPerTick);
                        // Idealmente podríamos leer los stacks actuales. Por simplicidad, agregamos el daño extra
                        // asumiendo que el efecto se aplicó
                        currentStacks = status.GetEffectStacks(StatusType.picante);
                    }

                    float totalDamage = tickDamage + (currentStacks * bonusDamagePerStack);

                    BossBase boss = collider.GetComponent<BossBase>();
                    if (boss == null) boss = collider.GetComponentInParent<BossBase>();
                    
                    if (boss != null)
                    {
                        boss.TakeDamage((int)totalDamage);
                    }
                    else
                    {
                        PblobController pblob = collider.GetComponent<PblobController>();
                        if (pblob != null)
                        {
                            pblob.TakeDamage(totalDamage);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
