using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoPepperoniPinaArea : MonoBehaviour
{
    [Header("Settings")]
    public float duration = GameBalance.Spells.QuesoPepperoniPina.AreaDuration;
    public float tickDamage = GameBalance.Spells.QuesoPepperoniPina.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoPepperoniPina.TickInterval;
    public float burnDuration = GameBalance.Spells.QuesoPepperoniPina.BurnDuration;
    public int burnStacks = GameBalance.Spells.QuesoPepperoniPina.BurnStacks;
    public float dotDamage = GameBalance.Spells.QuesoPepperoniPina.DoTDamage;
    public float dotInterval = GameBalance.Spells.QuesoPepperoniPina.DoTInterval;
    public float radius = 1.5f;

    public static GameObject CurrentActiveArea;
    private Dictionary<int, float> timeInAreaCounters = new Dictionary<int, float>();

    private void Start()
    {
        if (CurrentActiveArea != null) Destroy(CurrentActiveArea);
        CurrentActiveArea = gameObject;

        StartCoroutine(AreaCoroutine());
        StartCoroutine(DoTCoroutine());
    }

    private IEnumerator AreaCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("Boss") || collider.CompareTag("Enemy"))
                {
                    int instanceID = collider.GetInstanceID();
                    
                    if (!timeInAreaCounters.ContainsKey(instanceID))
                        timeInAreaCounters[instanceID] = 0f;
                    
                    timeInAreaCounters[instanceID] += tickInterval;

                    BossBase boss = collider.GetComponent<BossBase>();
                    if (boss == null) boss = collider.GetComponentInParent<BossBase>();
                    
                    if (boss != null)
                    {
                        boss.TakeDamage((int)tickDamage);
                    }
                    else
                    {
                        PblobController pblob = collider.GetComponent<PblobController>();
                        if (pblob != null)
                        {
                            pblob.TakeDamage(tickDamage);
                        }
                    }

                    StatusEffectSystem status = collider.GetComponent<StatusEffectSystem>();
                    if (status == null) status = collider.GetComponentInParent<StatusEffectSystem>();
                    
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

    private IEnumerator DoTCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("Boss") || collider.CompareTag("Enemy"))
                {
                    BossBase boss = collider.GetComponent<BossBase>();
                    if (boss == null) boss = collider.GetComponentInParent<BossBase>();
                    
                    if (boss != null)
                    {
                        boss.TakeDamage((int)dotDamage);
                    }
                    else
                    {
                        PblobController pblob = collider.GetComponent<PblobController>();
                        if (pblob != null)
                        {
                            pblob.TakeDamage(dotDamage);
                        }
                    }
                }
            }

            elapsed += dotInterval;
            yield return new WaitForSeconds(dotInterval);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
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
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
