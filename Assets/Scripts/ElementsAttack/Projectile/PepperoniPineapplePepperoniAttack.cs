using UnityEngine;
using System.Collections;

public class PepperoniPineapplePepperoniAttack : CharacterProjectile
{
    [Header("Catapult Settings")]
    public float explosionRadius = 4f;
    public float explosionDamage = 35f;
    public float explosionForce = 25f;
    public float fuseTime = 1f; // Duración total de la catapulta (1f)

    [Header("Scale Animation")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("Fire Trail Effect")]
    public GameObject fireTrailPrefab;
    public float fireTrailRadius = 1.5f;
    public float fireTrailDuration = 4f;
    public int fireTrailSpiceCharges = 2;
    public float fireTrailSpiceDuration = 4f;
    public bool leaveFireTrail = true; // Control para crear o no el firetrail

    [Header("Visual Effects")]
    public GameObject explosionEffect;

    [Header("Debug")]
    public bool showDebug = true;

    protected bool hasExploded = false;
    protected Vector3 initialScale;
    protected float lifeTimer = 0f;

    protected override void Start()
    {
        // No llamar a base.Start() para evitar que la clase base programe un Destroy automático
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        initialScale = transform.localScale;
        lifeTimer = 0f;
        hasExploded = false;

        if (showDebug)
            Debug.Log($"🍕🍍🍕 Catapulta inicializada con fuseTime: {fuseTime} segundos");
    }

    public override void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    void Update()
    {
        if (hasExploded) return;

        lifeTimer += Time.deltaTime;

        float animationProgress = Mathf.Clamp01(lifeTimer / fuseTime);

        // Primera mitad: expandir; segunda mitad: reducir
        if (animationProgress <= 0.5f)
        {
            float t = animationProgress / 0.5f;
            transform.localScale = initialScale * Mathf.Lerp(minScale, maxScale, t);
        }
        else
        {
            float t = (animationProgress - 0.5f) / 0.5f;
            transform.localScale = initialScale * Mathf.Lerp(maxScale, minScale, t);
        }

        if (lifeTimer >= fuseTime)
        {
            if (showDebug) Debug.Log("💣 Fusible terminado");
            Explode();
        }
    }

    protected virtual void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (showDebug)
            Debug.Log($"💥 EXPLOSIÓN en {transform.position}");

        CreateExplosionEffect();
        ApplyExplosionDamage();

        if (leaveFireTrail)
            CreateSingleFireTrail();

        StartCoroutine(DestroyNextFrame());
    }

    private IEnumerator DestroyNextFrame()
    {
        yield return null;
        Destroy(gameObject);
    }

    protected virtual void CreateExplosionEffect()
    {
        if (explosionEffect == null) return;

        GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        explosion.transform.localScale = Vector3.one * explosionRadius * 0.5f;
        Destroy(explosion, 2f);
    }

    protected virtual void ApplyExplosionDamage()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Boss"))
            {
                PblobController boss = collider.GetComponent<PblobController>();
                if (boss != null && boss.IsVulnerable())
                {
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    float dmgMult = 1f - (distance / explosionRadius);
                    dmgMult = Mathf.Clamp(dmgMult, 0.1f, 1f);

                    boss.TakeDamage(explosionDamage * dmgMult);
                }

                StatusEffectSystem status = collider.GetComponent<StatusEffectSystem>();
                if (status != null)
                {
                    status.ApplyEffect(StatusType.picante, 2f, gameObject, 1);
                }
            }
        }
    }

    protected virtual void CreateSingleFireTrail()
    {
        if (fireTrailPrefab == null)
        {
            if (showDebug) Debug.LogError("❌ No hay prefab de FireTrail");
            return;
        }

        GameObject fireTrail = Instantiate(fireTrailPrefab, transform.position, Quaternion.identity);

        // Asegurar que no quede como hijo del proyectil
        fireTrail.transform.SetParent(null);
        fireTrail.transform.localScale = Vector3.one;

        FireTrail ft = fireTrail.GetComponent<FireTrail>();
        if (ft != null)
        {
            ft.Initialize(
                fireTrailSpiceCharges,
                0.4f,
                fireTrailDuration,
                fireTrailRadius,
                fireTrailSpiceDuration
            );
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        // Ignorar colisiones con entidades amigas, proyectiles y firetrails
        if (other.CompareTag("Player") ||
            other.CompareTag("CharacterProjectile") ||
            other.CompareTag("CastPoint") ||
            other.CompareTag("Weapon") ||
            other.CompareTag("FireTrail"))
        {
            return;
        }

        if (showDebug)
            Debug.Log($"💥 Colisión con {other.name}, explotando...");

        Explode();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}