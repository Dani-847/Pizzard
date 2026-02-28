using UnityEngine;
using System.Collections;

public class FireTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    public int spiceChargesPerTick = Pizzard.Core.GameBalance.FireTrail.SpiceChargesPerTick;
    public float damageInterval = Pizzard.Core.GameBalance.FireTrail.DamageInterval;
    public float duration = Pizzard.Core.GameBalance.FireTrail.Duration;
    public float radius = Pizzard.Core.GameBalance.FireTrail.Radius; // Radio aumentado para mejor cobertura
    public float spiceEffectDuration = Pizzard.Core.GameBalance.FireTrail.SpiceEffectDuration;

    [Header("Visual")]
    public ParticleSystem fireParticles;
    public SpriteRenderer fireSprite;

    [Header("Debug")]
    public bool showDebug = false;

    private CircleCollider2D areaCollider;
    private float remainingDuration;

    private void Awake()
    {
        // Asegurar escala base correcta
        transform.localScale = Vector3.one;
        
        // Asignar tag único para evitar colisiones con proyectiles
        gameObject.tag = "FireTrail";
        
        // Configurar collider inmediatamente
        areaCollider = GetComponent<CircleCollider2D>();
        if (areaCollider == null)
        {
            areaCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        areaCollider.isTrigger = true;
        areaCollider.radius = radius;
    }

    private void Start()
    {
        if (showDebug) Debug.Log($"🔥 FireTrail iniciado en: {transform.position}");

        // Configurar partículas
        if (fireParticles != null)
        {
            var main = fireParticles.main;
            main.startSize = radius * 0.6f;
            fireParticles.Play();
        }

        // Configurar sprite
        if (fireSprite != null)
        {
            fireSprite.transform.localScale = Vector3.one * (radius * 1.2f);
        }

        // Iniciar daño después de un pequeño delay para evitar colisiones iniciales
        StartCoroutine(StartDamageAfterDelay(0.1f));
        remainingDuration = duration;
    }

    public void Initialize(int charges, float interval, float trailDuration, float trailRadius, float effectDuration)
    {
        spiceChargesPerTick = charges;
        damageInterval = interval;
        duration = trailDuration;
        radius = trailRadius;
        spiceEffectDuration = effectDuration;

        // Actualizar collider
        if (areaCollider != null)
        {
            areaCollider.radius = radius;
        }

        // Actualizar partículas
        if (fireParticles != null)
        {
            var main = fireParticles.main;
            main.startSize = radius * 0.6f;
        }

        // Actualizar sprite
        if (fireSprite != null)
        {
            fireSprite.transform.localScale = Vector3.one * (radius * 1.2f);
        }

        if (showDebug)
        {
            Debug.Log($"🔥 FireTrail configurado: radio={radius}, duración={duration}");
        }
    }

    private IEnumerator StartDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(ApplyDamageOverTime());
    }

    void Update()
    {
        if (remainingDuration > 0)
        {
            remainingDuration -= Time.deltaTime;
            
            // Fade out hacia el final
            if (remainingDuration < 1f && fireSprite != null)
            {
                float alpha = remainingDuration;
                Color color = fireSprite.color;
                color.a = alpha;
                fireSprite.color = color;
            }
            
            if (remainingDuration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator ApplyDamageOverTime()
    {
        while (remainingDuration > 0)
        {
            // Buscar enemigos en el área
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("Boss"))
                {
                    ApplySpiceCharges(collider.gameObject);
                }
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void ApplySpiceCharges(GameObject target)
    {
        StatusEffectSystem statusEffectSystem = target.GetComponent<StatusEffectSystem>();
        if (statusEffectSystem != null)
        {
            statusEffectSystem.ApplyEffect(StatusType.picante, spiceEffectDuration, gameObject, spiceChargesPerTick);
        }
    }

    // Para evitar colisiones con el proyectil que lo creó
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar colisiones con proyectiles del jugador y con el pepperonipepperoni
        if (other.CompareTag("CharacterProjectile") || other.CompareTag("FireTrail"))
        {
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}