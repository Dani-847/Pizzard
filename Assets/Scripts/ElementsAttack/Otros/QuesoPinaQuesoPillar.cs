using UnityEngine;
using System.Collections;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoPinaQuesoPillar : MonoBehaviour
{
    [Header("Settings")]
    public float duration = GameBalance.Spells.QuesoPinaQueso.Duration;
    public float tickDamage = GameBalance.Spells.QuesoPinaQueso.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoPinaQueso.TickInterval;
    public float radius = GameBalance.Spells.QuesoPinaQueso.Radius;
    public float maxHP = GameBalance.Spells.QuesoPinaQueso.HP;
    public float projectileSlowMultiplier = GameBalance.Spells.QuesoPinaQueso.ProjectileSlowMultiplier;

    private float currentHP;

    public static GameObject CurrentActivePillar;

    private void Start()
    {
        if (CurrentActivePillar != null)
        {
            Destroy(CurrentActivePillar);
        }
        CurrentActivePillar = gameObject;

        currentHP = maxHP;

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

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0f)
        {
            Destroy(gameObject);
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyProjectile"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity *= projectileSlowMultiplier;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyProjectile"))
        {
            // Opcional: que los proyectiles dañen el HP del pilar.
            // Por defecto, si queremos que los proyectiles reduzcan su HP
            // TakeDamage(10f); // ejemplo constante o leer daño del proyectil.
            // Para mantenerlo simple, solo destruimos el pilar si el jefe lo golpea
        }
        else if (collision.CompareTag("Boss") || collision.CompareTag("Enemy"))
        {
            TakeDamage(10f); // Daño por colisión directa con ente grande
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
