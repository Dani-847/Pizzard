using System.Collections;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class PepperoniQuesoAttack : CharacterProjectile
{
    [Header("Stick & Burn Settings")]
    public float stickDuration = GameBalance.Spells.PepperoniQueso.StickDuration;
    public float baseDamage = GameBalance.Spells.PepperoniQueso.BaseDamage;
    public float damagePerSecondStuck = GameBalance.Spells.PepperoniQueso.DamagePerSecondStuck;
    public float burnDuration = GameBalance.Spells.PepperoniQueso.BurnDuration;
    public int burnStacks = GameBalance.Spells.PepperoniQueso.BurnStacks;

    protected bool isStuck = false;
    protected float stuckElapsed = 0f;
    protected BossBase attachedBoss = null;
    protected PblobController attachedPblob = null;
    protected StatusEffectSystem attachedStatus = null;
    protected Coroutine stickRoutine = null;

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        if (rb != null && speed > 0)
            rb.velocity = transform.right * speed;

        float minLifetime = stickDuration + 0.5f;
        if (lifetime < minLifetime)
            lifetime = minLifetime;
        Destroy(gameObject, lifetime);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;

        bool hitBoss = false;
        if (other.CompareTag("Boss")) hitBoss = true;

        BossBase boss = other.GetComponent<BossBase>();
        if (boss == null) boss = other.GetComponentInParent<BossBase>();

        PblobController pblob = other.GetComponent<PblobController>();

        if (hitBoss || boss != null || pblob != null)
        {
            StatusEffectSystem status = other.GetComponent<StatusEffectSystem>();

            if (boss != null || pblob != null)
            {
                isStuck = true;
                attachedBoss = boss;
                attachedPblob = pblob;
                attachedStatus = status;

                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                }
                
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                transform.SetParent(other.transform, true);

                if (attachedStatus != null)
                {
                    attachedStatus.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacks);
                }

                stickRoutine = StartCoroutine(StickCoroutine());
                return;
            }
        }

        base.OnTriggerEnter2D(other);
    }

    private IEnumerator StickCoroutine()
    {
        stuckElapsed = 0f;
        float tickInterval = 0.25f;

        while (stuckElapsed < stickDuration)
        {
            yield return new WaitForSeconds(tickInterval);

            if (attachedBoss == null && attachedPblob == null) break;

            stuckElapsed += tickInterval;
            float appliedDamage = baseDamage + (stuckElapsed / stickDuration) * damagePerSecondStuck;

            if (attachedBoss != null) attachedBoss.TakeDamage((int)appliedDamage);
            else if (attachedPblob != null) attachedPblob.TakeDamage(appliedDamage);
        }

        CleanupAndDestroy();
    }

    protected void CleanupAndDestroy()
    {
        if (stickRoutine != null)
        {
            StopCoroutine(stickRoutine);
            stickRoutine = null;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
        }

        transform.SetParent(null);
        attachedStatus = null;
        attachedBoss = null;
        attachedPblob = null;

        Destroy(gameObject);
    }

    public void ForceRelease()
    {
        CleanupAndDestroy();
    }
}
