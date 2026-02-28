using System.Collections;
using UnityEngine;

public class PepperoniQuesoPineappleAttack : CharacterProjectile
{
    [Header("Stick & Burn Settings")]
    public float stickDuration = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.StickDuration;
    public int burnStacksOnStick = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.BurnStacksOnStick;
    public float burnDuration = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.BurnDuration;
    public float tickInterval = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.TickInterval;

    [Header("Damage Scaling")]
    public float damageScaleMax = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.DamageScaleMax; // adicional máximo (1 => hasta 2x)

    [Header("Children Settings")]
    public GameObject childPrefab;
    public int childCount = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildCount;
    public float childDamageMultiplier = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildDamageMultiplier; // hijos hacen menos daño
    public float childSpawnRadius = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildSpawnRadius; // offset desde el centro
    public float childSpreadAngle = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildSpreadAngle; // grados totales de spread
    public float childStickDuration = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildStickDuration;
    public int childBurnStacks = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildBurnStacks;
    public float childBurnDuration = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildBurnDuration;
    public float childTickInterval = Pizzard.Core.GameBalance.Spells.PepperoniQuesoPineapple.ChildTickInterval;

    protected bool isStuck = false;
    protected float stuckElapsed = 0f;
    protected PblobController attachedBoss = null;
    protected StatusEffectSystem attachedStatus = null;
    protected Coroutine stickRoutine = null;

    protected override void Start()
    {
        // similar a otros sticky: no queremos que lifetime destruya antes
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

        // Solo pegarse a Boss
        if (other.CompareTag("Boss"))
        {
            PblobController boss = other.GetComponent<PblobController>();
            StatusEffectSystem status = other.GetComponent<StatusEffectSystem>();

            if (boss != null)
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

                // Aplicar quemado usando el sistema del boss (se stackea con otras fuentes)
                if (attachedStatus != null)
                {
                    attachedStatus.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacksOnStick);
                }

                stickRoutine = StartCoroutine(StickCoroutine());
                return;
            }
        }

        // resto por defecto
        base.OnTriggerEnter2D(other);
    }

    private IEnumerator StickCoroutine()
    {
        stuckElapsed = 0f;

        while (stuckElapsed < stickDuration)
        {
            yield return new WaitForSeconds(tickInterval);

            if (attachedBoss == null)
                break;

            stuckElapsed += tickInterval;
            float t = Mathf.Clamp01(stuckElapsed / stickDuration);
            float multiplier = 1f + (damageScaleMax * t);
            float appliedDamage = damage * multiplier;

            attachedBoss.TakeDamage(appliedDamage);
        }

        SpawnChildrenAndCleanup();
    }

    private void SpawnChildrenAndCleanup()
    {
        if (childPrefab != null && attachedBoss != null)
        {
            // Spread centrado
            float startAngle = -childSpreadAngle * 0.5f;
            float step = childCount > 1 ? childSpreadAngle / (childCount - 1) : 0f;

            for (int i = 0; i < childCount; i++)
            {
                float angle = startAngle + step * i;
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                Vector2 offset = rot * Vector2.right * childSpawnRadius;

                Vector3 spawnPos = attachedBoss.transform.position + (Vector3)offset;
                GameObject go = Instantiate(childPrefab, spawnPos, Quaternion.identity);

                SmallStickyProjectile child = go.GetComponent<SmallStickyProjectile>();
                if (child != null)
                {
                    float childDamage = damage * childDamageMultiplier;
                    int stacks = childBurnStacks > 0 ? childBurnStacks : Mathf.Max(1, burnStacksOnStick / 2);
                    child.InitializeAttached(attachedBoss, attachedStatus, childDamage, childStickDuration, stacks, childBurnDuration, childTickInterval);
                }
                else
                {
                    // Si el prefab no tiene componente, intentar inicializar como CharacterProjectile
                    CharacterProjectile cp = go.GetComponent<CharacterProjectile>();
                    if (cp != null)
                    {
                        // posicionarlo y destruirlo tras childStickDuration
                        Destroy(go, childStickDuration + 0.5f);
                    }
                }
            }
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