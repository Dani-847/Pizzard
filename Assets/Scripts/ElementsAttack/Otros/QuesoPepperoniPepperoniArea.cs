using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoPepperoniPepperoniArea : MonoBehaviour
{
    [Header("Settings")]
    public float duration = GameBalance.Spells.QuesoPepperoniPepperoni.AreaDuration;
    public float tickDamage = GameBalance.Spells.QuesoPepperoniPepperoni.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoPepperoniPepperoni.TickInterval;
    public float burnDuration = GameBalance.Spells.QuesoPepperoniPepperoni.BurnDuration;
    public int burnStacks = GameBalance.Spells.QuesoPepperoniPepperoni.BurnStacks;
    public float radius = 1.5f;

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
