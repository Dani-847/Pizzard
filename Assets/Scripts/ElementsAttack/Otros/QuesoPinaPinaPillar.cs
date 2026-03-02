using UnityEngine;
using System.Collections;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoPinaPinaPillar : MonoBehaviour
{
    [Header("Settings")]
    public float duration = GameBalance.Spells.QuesoPinaPina.Duration;
    public float tickDamage = GameBalance.Spells.QuesoPinaPina.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoPinaPina.TickInterval;
    public float radius = GameBalance.Spells.QuesoPinaPina.Radius;

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
                if (collider.CompareTag("Boss"))
                {
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
