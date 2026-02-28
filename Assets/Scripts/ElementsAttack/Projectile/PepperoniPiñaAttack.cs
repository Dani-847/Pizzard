using UnityEngine;
using System.Collections;

public class PepperoniPiñaAttack : CharacterProjectile
{
    [Header("Catapult Settings")]
    public float explosionRadius = Pizzard.Core.GameBalance.Spells.PepperoniPina.ExplosionRadius;
    public float explosionDamage = Pizzard.Core.GameBalance.Spells.PepperoniPina.ExplosionDamage;
    public float explosionForce = Pizzard.Core.GameBalance.Spells.PepperoniPina.ExplosionForce;
    public float explosionDelay = Pizzard.Core.GameBalance.Spells.PepperoniPina.ExplosionDelay;

    [Header("Scale Animation")]
    public float minScale = Pizzard.Core.GameBalance.Spells.PepperoniPina.MinScale;
    public float maxScale = Pizzard.Core.GameBalance.Spells.PepperoniPina.MaxScale;
    public float scaleAnimationSpeed = Pizzard.Core.GameBalance.Spells.PepperoniPina.ScaleAnimationSpeed;

    [Header("Visual Effects")]
    public GameObject explosionEffect;

    private bool hasExploded = false;
    private Vector3 initialScale;
    private float scaleTimer = 0f;

    protected override void Start()
    {
        base.Start(); // Esto es importante para la configuración base
        
        initialScale = transform.localScale;
        
        // Programar explosión automática
        StartCoroutine(ExplodeAfterDelay());
    }

    public override void Initialize(Vector2 shootDirection)
    {
        base.Initialize(shootDirection);
        
        // Configurar rotación inicial
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (hasExploded) return;

        // Animar la escala para simular el arco
        AnimateScale();
        
        // Mantener movimiento constante
        if (rb != null && !hasExploded)
        {
            rb.velocity = direction * speed;
        }
    }

    private void AnimateScale()
    {
        // Animación de escala para simular subida y bajada
        scaleTimer += Time.deltaTime * scaleAnimationSpeed;
        float scaleFactor = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(scaleTimer, 1f));
        transform.localScale = initialScale * scaleFactor;
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Efecto visual de explosión
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Aplicar daño en área
        ApplyExplosionDamage();

        // Destruir el proyectil
        Destroy(gameObject);
    }

    private void ApplyExplosionDamage()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Boss"))
            {
                // Aplicar daño
                PblobController boss = collider.GetComponent<PblobController>();
                if (boss != null)
                {
                    boss.TakeDamage(explosionDamage);
                }

                // Aplicar fuerza de explosión
                Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 explosionDir = (collider.transform.position - transform.position).normalized;
                    rb.AddForce(explosionDir * explosionForce, ForceMode2D.Impulse);
                }
            }
        }

        Debug.Log($"💥 Explosión de catapulta! Radio: {explosionRadius}, Daño: {explosionDamage}");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        // Ignorar colisiones durante el vuelo (proyectil intangible)
        if (other.CompareTag("Enemy") || 
            other.CompareTag("Player") || other.CompareTag("CharacterProjectile") || 
            other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
        {
            return;
        }

        // Si colisiona con una pared u obstáculo, explotar inmediatamente
        if (other.CompareTag("Boss") || other.CompareTag("Wall"))
        {
            Explode();
        }
    }

    // Para debug en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}