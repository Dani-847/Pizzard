using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoPepperoniArea : MonoBehaviour
{
    [Header("Settings")]
    public float duration = GameBalance.Spells.QuesoPepperoni.AreaDuration;
    public float tickDamage = GameBalance.Spells.QuesoPepperoni.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoPepperoni.TickInterval;
    public float burnDuration = GameBalance.Spells.QuesoPepperoni.BurnDuration;
    public int burnStacks = GameBalance.Spells.QuesoPepperoni.BurnStacks;
    public float damageScalePerSecond = GameBalance.Spells.QuesoPepperoni.DamageScalePerSecond;

    public static GameObject CurrentActiveArea;
    private Dictionary<int, float> timeInAreaCounters = new Dictionary<int, float>();

    private void Start()
    {
        if (CurrentActiveArea != null) Destroy(CurrentActiveArea);
        CurrentActiveArea = gameObject;

        StartCoroutine(AreaCoroutine());
    }

    private IEnumerator AreaCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("Boss"))
                {
                    int instanceID = collider.GetInstanceID();
                    
                    if (!timeInAreaCounters.ContainsKey(instanceID))
                        timeInAreaCounters[instanceID] = 0f;
                    
                    float timeInArea = timeInAreaCounters[instanceID];
                    float damage = tickDamage * (1f + damageScalePerSecond * timeInArea);
                    
                    timeInAreaCounters[instanceID] += tickInterval;

                    BossBase boss = collider.GetComponent<BossBase>();
                    if (boss == null) boss = collider.GetComponentInParent<BossBase>();
                    
                    if (boss != null)
                    {
                        boss.TakeDamage((int)damage);
                    }
                    else
                    {
                        PblobController pblob = collider.GetComponent<PblobController>();
                        if (pblob != null)
                        {
                            pblob.TakeDamage(damage);
                        }
                    }

                    StatusEffectSystem status = collider.GetComponent<StatusEffectSystem>();
                    if (status != null)
                    {
                        status.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacks);
                    }
                }
            }

            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }

        Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boss"))
        {
            int instanceID = other.GetInstanceID();
            if (timeInAreaCounters.ContainsKey(instanceID))
            {
                timeInAreaCounters.Remove(instanceID);
            }
        }
    }

    private void OnDestroy()
    {
        if (CurrentActiveArea == gameObject) CurrentActiveArea = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
