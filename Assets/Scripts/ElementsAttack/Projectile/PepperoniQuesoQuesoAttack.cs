using System.Collections;
using UnityEngine;

public class PepperoniQuesoQuesoAttack : CharacterProjectile
{
    [Header("Stick & Burn Settings")]
    public float stickDuration = 4f;
    public int burnStacksOnStick = 1;
    public float burnDuration = 4f;
    public float tickInterval = 0.25f;

    [Header("Damage Scaling")]
    public float damageScaleMax = 1f; // adicional máximo (1 => hasta 2x)

    protected bool isStuck = false;
    protected float stuckElapsed = 0f;
    protected PblobController attachedBoss = null;
    protected StatusEffectSystem attachedStatus = null;
    protected Coroutine stickRoutine = null;

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        if (rb != null && speed > 0)
            rb.velocity = transform.right * speed;

        float minLifetime = stickDuration + 0.5f;
        if (lifetime < minLifetime) lifetime = minLifetime;
        Destroy(gameObject, lifetime);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck)
            return;

        if (other.CompareTag("Boss"))
        {
            PblobController boss = other.GetComponent<PblobController>();
            StatusEffectSystem status = other.GetComponent<StatusEffectSystem>();

            if (boss != null && boss.IsVulnerable())
            {
                isStuck = true;
                attachedBoss = boss;
                attachedStatus = status;

                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                }

                transform.SetParent(other.transform, true);

                if (attachedStatus != null)
                {
                    attachedStatus.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacksOnStick);
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

        while (stuckElapsed < stickDuration)
        {
            yield return new WaitForSeconds(tickInterval);

            if (attachedBoss == null || !attachedBoss.IsVulnerable())
                break;

            stuckElapsed += tickInterval;
            float t = Mathf.Clamp01(stuckElapsed / stickDuration);
            float multiplier = 1f + (damageScaleMax * t);
            float appliedDamage = damage * multiplier;

            attachedBoss.TakeDamage(appliedDamage);
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

        Destroy(gameObject);
    }

    public void ForceRelease()
    {
        CleanupAndDestroy();
    }
}