using UnityEngine;

public class PepperoniPineappleQuesoAttack : PepperoniPineapplePepperoniAttack
{
    [Header("Ajustes Queso")]
    public float extraFireRadiusMultiplier = Pizzard.Core.GameBalance.Spells.PepperoniPineappleQueso.ExtraFireRadiusMultiplier;      // fuego más grande
    public float extraFireDurationMultiplier = Pizzard.Core.GameBalance.Spells.PepperoniPineappleQueso.ExtraFireDurationMultiplier;
    public float burnDurationIfIgnited = Pizzard.Core.GameBalance.Spells.PepperoniPineappleQueso.BurnDurationIfIgnited;         // duración del efecto picante si el proyectil estaba en fuego
    public int burnStacksIfIgnited = Pizzard.Core.GameBalance.Spells.PepperoniPineappleQueso.BurnStacksIfIgnited;              // stacks aplicados si estaba en fuego

    protected bool isIgnited = false;

    protected override void Start()
    {
        // Aumentar parámetros del firetrail antes de la inicialización
        fireTrailRadius *= extraFireRadiusMultiplier;
        fireTrailDuration *= extraFireDurationMultiplier;

        if (showDebug)
            Debug.Log($"🍕🍍🧀 QuesoCatapulta: fireRadius={fireTrailRadius}, fireDuration={fireTrailDuration}");

        base.Start();
    }

    // Si el proyectil entra en un FireTrail, no explotará ni se destruirá: se marca como encendido
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        if (other.CompareTag("FireTrail"))
        {
            isIgnited = true;
            if (showDebug) Debug.Log("🔥 Proyectil se ha encendido al entrar en FireTrail");
            // No retornar a base para evitar destrucción; simplemente ignorar/registrar
            return;
        }

        // Resto de colisiones usar comportamiento por defecto (explotar)
        base.OnTriggerEnter2D(other);
    }

    // Aplicar daño y efectos; si el proyectil estaba encendido, aplicar quemado más fuerte
    protected override void ApplyExplosionDamage()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Boss"))
            {
                PblobController boss = collider.GetComponent<PblobController>();
                if (boss != null)
                {
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    float dmgMult = 1f - (distance / explosionRadius);
                    dmgMult = Mathf.Clamp(dmgMult, 0.1f, 1f);

                    boss.TakeDamage(explosionDamage * dmgMult);
                }

                StatusEffectSystem status = collider.GetComponent<StatusEffectSystem>();
                if (status != null)
                {
                    if (isIgnited)
                    {
                        // Quemado más potente si el proyectil venía de fuego
                        status.ApplyEffect(StatusType.picante, burnDurationIfIgnited, gameObject, burnStacksIfIgnited);
                        if (showDebug) Debug.Log($"🔥 Proyectil encendido: aplicando {burnStacksIfIgnited} stacks por {burnDurationIfIgnited}s");
                    }
                    else
                    {
                        // Comportamiento por defecto
                        status.ApplyEffect(StatusType.picante, 2f, gameObject, 1);
                    }
                }
            }
        }
    }

    // Dibujar gizmo con el radio de explosión (mantener magenta si se desea diferenciar)
    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}