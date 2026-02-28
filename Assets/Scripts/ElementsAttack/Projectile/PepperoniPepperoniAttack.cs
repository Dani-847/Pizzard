using UnityEngine;
using System.Collections;

public class PepperoniPepperoniAttack : CharacterProjectile
{
    [Header("Spice Settings")]
    public int initialSpiceCharges = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.InitialSpiceCharges;
    public int trailSpiceCharges = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.TrailSpiceCharges;
    public float trailDamageInterval = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.TrailDamageInterval;
    public float trailDuration = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.TrailDuration;
    public float trailRadius = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.TrailRadius;
    public float spiceEffectDuration = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.SpiceEffectDuration;

    [Header("Trail Settings")]
    public float trailSpawnInterval = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.TrailSpawnInterval; // Cada cuánto spawnear una estela
    public GameObject fireTrailEffect;

    [Header("Visual Effects")]
    public GameObject impactEffect;

    private bool hasImpacted = false;
    private float trailTimer = 0f;
    private Vector3 lastTrailPosition;
    private float minDistanceBetweenTrails = Pizzard.Core.GameBalance.Spells.PepperoniPepperoni.MinDistanceBetweenTrails; // Distancia mínima entre estelas

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