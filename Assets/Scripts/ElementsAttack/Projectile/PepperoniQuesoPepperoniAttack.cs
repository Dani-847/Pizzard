using System.Collections;
using UnityEngine;

public class PepperoniQuesoPepperoniAttack : CharacterProjectile
{
    [Header("Stick & Burn Settings")]
    public float stickDuration = 2f;
    public int burnStacksOnStick = 4;
    public float burnDuration = 4f;
    public float tickInterval = 0.5f; // cada cuánto aplica daño mientras está pegado

    [Header("Damage Scaling")]
    // El daño base viene de `damage` (heredado). El daño aplicado en cada tick será:
    // damage * (1 + elapsed/stickDuration) — llega hasta 2x al final.
    public float damageScaleMax = 1f; // multiplicador adicional máximo (1 => hasta 2x)

    protected bool isStuck = false;
    protected float stuckElapsed = 0f;
    protected PblobController attachedBoss = null;
    protected StatusEffectSystem attachedStatus = null; // referencia al sistema de estados del boss
    protected Coroutine stickRoutine = null;

    protected override void Start()
    {
        // No llamar base.Start() para controlar vida si se desea,
        // pero usar sus valores por defecto está bien si lifetime >= stickDuration.
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        if (rb != null && speed > 0)
            rb.velocity = transform.right * speed;

        // Asegurar que el lifetime no destruya antes de tiempo
        float minLifetime = stickDuration + 0.5f;
        if (lifetime < minLifetime)
            lifetime = minLifetime;
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

            if (boss != null && boss.IsVulnerable())
            {
                // Marcar pegado
                isStuck = true;
                attachedBoss = boss;
                attachedStatus = status; // guardar referencia para poder stackear o reutilizar después

                // Parar movimiento y adherirse visualmente
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                }
                transform.SetParent(other.transform, true);

                // Aplicar quemado inmediato en stacks (se sumará con otras aplicaciones gracias al StatusEffectSystem del boss)
                if (attachedStatus != null)
                {
                    attachedStatus.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacksOnStick);
                }

                // Empezar rutina de daño escalado
                stickRoutine = StartCoroutine(StickCoroutine());
                return;
            }
        }

        // Resto de colisiones usan comportamiento por defecto
        base.OnTriggerEnter2D(other);
    }

    private IEnumerator StickCoroutine()
    {
        stuckElapsed = 0f;

        // Esperar y aplicar daño periódicamente mientras el boss sea vulnerable
        while (stuckElapsed < stickDuration)
        {
            yield return new WaitForSeconds(tickInterval);

            if (attachedBoss == null || !attachedBoss.IsVulnerable())
                break;

            stuckElapsed += tickInterval;
            float t = Mathf.Clamp01(stuckElapsed / stickDuration);
            float multiplier = 1f + (damageScaleMax * t); // si damageScaleMax=1 => 1..2x
            float appliedDamage = damage * multiplier;

            attachedBoss.TakeDamage(appliedDamage);
        }

        CleanupAndDestroy();
    }

    protected void CleanupAndDestroy()
    {
        // Desanclar y destruir
        if (stickRoutine != null)
        {
            StopCoroutine(stickRoutine);
            stickRoutine = null;
        }

        // Restaurar física por si acaso
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
        }

        transform.SetParent(null);

        // Limpiar referencia al sistema de estados
        attachedStatus = null;
        attachedBoss = null;

        Destroy(gameObject);
    }

    // Si se desea cancelar manualmente (ej. al morir el boss), exponer método
    public void ForceRelease()
    {
        CleanupAndDestroy();
    }
}