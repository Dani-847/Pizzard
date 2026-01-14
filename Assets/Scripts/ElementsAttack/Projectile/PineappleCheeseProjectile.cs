using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PineappleCheeseProjectile : CharacterProjectile
{
    [Header("Absorption Settings")]
    public float absorptionRadius = 2f;
    public int maxAbsorbedProjectiles = 10;
    
    [Header("Growth Settings")]
    public bool enableGrowth = true;
    public float growthPerProjectile = 0.05f;
    public float maxGrowthMultiplier = 1.5f;

    [Header("Impact Damage Settings")]
    public float impactDamage = 15f;
    public float damageOverTime = 5f;
    public float damageTickInterval = 0.5f;
    public float damageDuration = 3f;
    public float impactRadius = 2.5f;

    [Header("Visual Effects")]
    public GameObject absorptionEffect;
    public GameObject impactEffect;
    public GameObject damageAreaEffect;

    private bool hasImpacted = false;
    private List<GameObject> absorbedProjectiles = new List<GameObject>();
    private Vector3 initialScale;

    protected override void Start()
    {
        // LLAMAR AL START DE LA CLASE BASE - ESTO ES CLAVE
        base.Start();
        
        // Configuración adicional específica de este proyectil
        initialScale = transform.localScale;
        
        // Cancelamos el Destroy automático de la clase base porque lo manejamos diferente
        CancelInvoke(); // Esto cancela cualquier Invoke pendiente
    }

    public override void Initialize(Vector2 shootDirection)
    {
        base.Initialize(shootDirection); // Llama al Initialize de la clase base
        
        // Configurar el lifetime específico para este proyectil
        // No necesitamos Destroy aquí porque lo manejamos en Impact()
    }

    void Update()
    {
        if (hasImpacted) return;

        AbsorbEnemyProjectiles();

        // Mantener movimiento constante - importante para evitar que se "caiga"
        if (rb != null && !hasImpacted)
        {
            rb.velocity = direction * speed;
        }
    }

    private void AbsorbEnemyProjectiles()
    {
        if (absorbedProjectiles.Count >= maxAbsorbedProjectiles) return;

        Collider2D[] nearbyProjectiles = Physics2D.OverlapCircleAll(transform.position, absorptionRadius);

        foreach (Collider2D collider in nearbyProjectiles)
        {
            if (collider.CompareTag("EnemyProjectile") && !absorbedProjectiles.Contains(collider.gameObject))
            {
                AbsorbProjectile(collider.gameObject);
            }
        }
    }

    private void AbsorbProjectile(GameObject projectile)
    {
        if (absorbedProjectiles.Count >= maxAbsorbedProjectiles) return;

        absorbedProjectiles.Add(projectile);

        if (absorptionEffect != null)
            Instantiate(absorptionEffect, projectile.transform.position, Quaternion.identity);

        Destroy(projectile);

        // Crecimiento controlado
        if (enableGrowth)
        {
            float scaleMultiplier = 1f + (absorbedProjectiles.Count * growthPerProjectile);
            scaleMultiplier = Mathf.Min(scaleMultiplier, maxGrowthMultiplier);
            transform.localScale = initialScale * scaleMultiplier;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasImpacted) return;

        // Ignorar colisiones con el jugador y sus proyectiles
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile") || 
            other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;

        // Impactar con enemigos, boss o paredes
        if (other.CompareTag("Boss") || other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            Impact();
            return; // Importante: return para evitar que la base también maneje la colisión
        }

        // Para otras colisiones, usar el comportamiento base
        base.OnTriggerEnter2D(other);
    }

    private void Impact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        if (damageAreaEffect != null)
        {
            GameObject area = Instantiate(damageAreaEffect, transform.position, Quaternion.identity);
            Destroy(area, damageDuration);
        }

        ApplyImpactDamage();
        StartCoroutine(ApplyDamageOverTime());

        Destroy(gameObject, damageDuration + 0.5f);
    }

    private void ApplyImpactDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);

        foreach (var c in hits)
        {
            if (c.CompareTag("Enemy") || c.CompareTag("Boss"))
            {
                float totalDamage = impactDamage + absorbedProjectiles.Count * 2f;

                var boss = c.GetComponent<PblobController>();
                if (boss != null && boss.IsVulnerable())
                    boss.TakeDamage(totalDamage);

                Rigidbody2D r = c.GetComponent<Rigidbody2D>();
                if (r != null)
                {
                    Vector2 force = (c.transform.position - transform.position).normalized;
                    r.AddForce(force * (impactDamage * 0.5f), ForceMode2D.Impulse);
                }
            }
        }
    }

    private IEnumerator ApplyDamageOverTime()
    {
        float timer = 0f;

        while (timer < damageDuration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);

            foreach (var c in hits)
            {
                if (c.CompareTag("Boss"))
                {
                    float tickDamage = damageOverTime + absorbedProjectiles.Count * 0.5f;

                    var boss = c.GetComponent<PblobController>();
                    if (boss != null && boss.IsVulnerable())
                        boss.TakeDamage(tickDamage);
                }
            }

            yield return new WaitForSeconds(damageTickInterval);
            timer += damageTickInterval;
        }
    }

    // Para debug en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, absorptionRadius);
        
        if (hasImpacted)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, impactRadius);
        }
    }
}