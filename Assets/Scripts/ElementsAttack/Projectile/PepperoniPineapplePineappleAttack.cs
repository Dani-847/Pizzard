using UnityEngine;

public class PepperoniPineapplePineappleAttack : PepperoniPineapplePepperoniAttack
{
    [Header("Ajustes especiales PiñaPiña")]
    public float extraRadiusMultiplier = 1.6f;
    public float extraDamageMultiplier = 1.5f;

    protected override void Start()
    {
        // Ajustar valores antes de la inicialización visual
        explosionRadius *= extraRadiusMultiplier;
        explosionDamage *= extraDamageMultiplier;

        // No dejar firetrail en este ataque
        leaveFireTrail = false;

        base.Start();

        if (showDebug)
            Debug.Log($"🍕🍍🍍 PiñaPiña inicializado: radius={explosionRadius}, damage={explosionDamage}, fireTrail={leaveFireTrail}");
    }

    protected override void ApplyExplosionDamage()
    {
        // Reusar lógica pero con posibilidad de cambios futuros
        base.ApplyExplosionDamage();
    }

    protected override void CreateSingleFireTrail()
    {
        // Intencionalmente vacío: este ataque no deja área de firetrail
        if (showDebug)
            Debug.Log("🍕🍍🍍 No se crea FireTrail en este ataque.");
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}