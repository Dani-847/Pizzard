using UnityEngine;
using System.Collections;

public class PepperoniPepperoniAttack : CharacterProjectile
{
    [Header("Spice Settings")]
    public int initialSpiceCharges = 4;
    public int trailSpiceCharges = 2;
    public float trailDamageInterval = 0.2f;
    public float trailDuration = 3f;
    public float trailRadius = 2f;
    public float spiceEffectDuration = 5f;

    [Header("Trail Settings")]
    public float trailSpawnInterval = 0.1f; // Cada cuánto spawnear una estela
    public GameObject fireTrailEffect;

    [Header("Visual Effects")]
    public GameObject impactEffect;

    private bool hasImpacted = false;
    private float trailTimer = 0f;
    private Vector3 lastTrailPosition;
    private float minDistanceBetweenTrails = 0.5f; // Distancia mínima entre estelas

    protected override void Start()
    {
        base.Start();
        lastTrailPosition = transform.position;
    }

    public override void Initialize(Vector2 shootDirection)
    {
        base.Initialize(shootDirection);
    }

    void Update()
    {
        if (hasImpacted) return;

        // Dejar estela mientras se mueve
        trailTimer += Time.deltaTime;
        
        // Verificar distancia desde la última estela
        float distanceFromLastTrail = Vector3.Distance(transform.position, lastTrailPosition);
        
        if (trailTimer >= trailSpawnInterval && distanceFromLastTrail >= minDistanceBetweenTrails)
        {
            SpawnFireTrail();
            trailTimer = 0f;
            lastTrailPosition = transform.position;
        }
    }

    private void SpawnFireTrail()
    {
        if (fireTrailEffect != null)
        {
            GameObject fireTrail = Instantiate(fireTrailEffect, transform.position, Quaternion.identity);
            FireTrail trail = fireTrail.GetComponent<FireTrail>();
            if (trail != null)
            {
                trail.Initialize(trailSpiceCharges, trailDamageInterval, trailDuration, trailRadius, spiceEffectDuration);
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasImpacted) return;

        // Usar una sola condición compuesta para mayor claridad
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile") || 
            other.CompareTag("CastPoint") || other.CompareTag("Weapon") || other.CompareTag("FireTrail"))
            return;

        // Impactar con enemigos, boss o paredes
        if (other.CompareTag("Boss") || other.CompareTag("Wall"))
        {
            Impact(other);
        }
        else
        {
            // Para otras colisiones, usar el comportamiento base
            base.OnTriggerEnter2D(other);
        }
    }

    private void Impact(Collider2D other)
    {
        if (hasImpacted) return;
        hasImpacted = true;

        // Efecto visual de impacto
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Aplicar cargas de picante iniciales
        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
        {
            ApplySpiceCharges(other.gameObject, initialSpiceCharges);
        }

        // Destruir el proyectil
        Destroy(gameObject);
    }

    private void ApplySpiceCharges(GameObject target, int charges)
    {
        StatusEffectSystem statusEffectSystem = target.GetComponent<StatusEffectSystem>();
        if (statusEffectSystem != null)
        {
            statusEffectSystem.ApplyEffect(StatusType.picante, spiceEffectDuration, gameObject, charges);
            Debug.Log($"🌶️ Aplicadas {charges} cargas de picante a {target.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {target.name} no tiene StatusEffectSystem");
        }
    }
}