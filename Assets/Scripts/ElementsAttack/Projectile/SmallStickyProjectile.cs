using System.Collections;
using UnityEngine;

public class SmallStickyProjectile : CharacterProjectile
{
    protected bool isStuck = false;
    protected float stuckElapsed = 0f;
    protected PblobController attachedBoss = null;
    protected StatusEffectSystem attachedStatus = null;
    protected Coroutine stickRoutine = null;

    // Estos valores se inicializan mediante InitializeAttached
    protected float localStickDuration = 1f;
    protected float localTickInterval = 0.25f;
    protected int localBurnStacks = 1;
    protected float localBurnDuration = 3f;

    protected override void Start()
    {
        // Evitar que CharacterProjectile inicialice velocidad por defecto
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";
        // Destruir si nadie lo pega pasados unos segundos
        float minLifetime = localStickDuration + 0.5f;
        if (lifetime < minLifetime) lifetime = minLifetime;
        Destroy(gameObject, lifetime);
    }

    // Inicializa ya pegado al boss (llamar después de Instantiate)
    public void InitializeAttached(PblobController boss, StatusEffectSystem status, float dmg, float stickDur, int burnStacks, float burnDur, float tickInt)
    {
        attachedBoss = boss;
        attachedStatus = status;
        damage = dmg;
        localStickDuration = stickDur;
        localBurnStacks = burnStacks;
        localBurnDuration = burnDur;
        localTickInterval = tickInt;

        isStuck = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (attachedBoss != null)
        {
            transform.SetParent(attachedBoss.transform, true);
        }

        if (attachedStatus != null)
        {
            attachedStatus.ApplyEffect(StatusType.picante, localBurnDuration, gameObject, localBurnStacks);
        }

        stickRoutine = StartCoroutine(StickCoroutine());
    }

    private IEnumerator StickCoroutine()
    {
        stuckElapsed = 0f;

        while (stuckElapsed < localStickDuration)
        {
            yield return new WaitForSeconds(localTickInterval);

            if (attachedBoss == null || !attachedBoss.IsVulnerable())
                break;

            stuckElapsed += localTickInterval;
            float t = Mathf.Clamp01(stuckElapsed / localStickDuration);
            // Los hijos no escalan tanto; usar 0.5 como ejemplo
            float multiplier = 1f + (0.5f * t);
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

    // En caso de colisiones por si el prefab se mueve antes de inicializarse
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
                // Si choca al boss, comportarse igual que InitializeAttached
                InitializeAttached(boss, status, damage, localStickDuration, localBurnStacks, localBurnDuration, localTickInterval);
                return;
            }
        }

        base.OnTriggerEnter2D(other);
    }
}