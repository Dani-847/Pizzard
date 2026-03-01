using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterProjectile : MonoBehaviour
{
    public float speed = Pizzard.Core.GameBalance.Spells.Base.Speed;
    public float lifetime = Pizzard.Core.GameBalance.Spells.Base.Lifetime;
    protected Rigidbody2D rb;
    public float damage = Pizzard.Core.GameBalance.Spells.Base.Damage;
    protected Vector2 direction; // Añadimos esto

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";
        
        if (rb != null && speed > 0)
        {
            rb.velocity = transform.right * speed;
        }
        
        Destroy(gameObject, lifetime);
    }

    // AÑADIMOS ESTE MÉTODO VIRTUAL
    public virtual void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection;
        
        // Rotar el proyectil para que mire en la dirección del movimiento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Establecer velocidad
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // NO dañar al jugador ni aliados
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile"))
        {
            return;
        }

        // Ignorar los círculos del Phase 2 de P'blob — no deben bloquear hechizos
        if (other.gameObject.layer == LayerMask.NameToLayer("BossCircle"))
            return;
        
        // Dañar al BOSS
        if (other.CompareTag("Boss"))
        {
            float dmgMultiplier = 1f;
            if (other.GetComponent<Pizzard.Bosses.NiggelController>() != null)
                dmgMultiplier = Pizzard.Bosses.NiggelController.PlayerDamageMultiplier;

            Pizzard.Bosses.BossBase boss = other.GetComponent<Pizzard.Bosses.BossBase>();
            if (boss != null)
            {
                boss.TakeDamage((int)(damage * dmgMultiplier));
                Debug.Log($"@{GetType().Name} golpe al boss: {damage * dmgMultiplier} de daño");
            }
            else
            {
                PblobController pblob = other.GetComponent<PblobController>();
                if (pblob != null)
                {
                    pblob.TakeDamage(damage * dmgMultiplier);
                    Debug.Log($"@{GetType().Name} golpe al Pblob: {damage * dmgMultiplier} de daño");
                }
            }
            Destroy(gameObject);
            return;
        }

        // Evitar colisiones con partes del jugador
        if (other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;

        // Ignore Tilemaps and Background/Ground elements that could block spells meant for bosses
        if (other.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null)
            return;

        // Ignorar otros triggers (zonas, áreas de efecto) para que el hechizo no se destruya en el aire
        if (other.isTrigger)
        {
            // Except for Niggel's Black Dot Barriers!
            if (other.GetComponent<BlackDotBarrier>() != null)
            {
                // Let the barrier's own collision handle the destruction
                return;
            }
            
            // Otherwise ignore triggers
            return;
        }

        // Para cualquier otra colisión sólida, destruir el proyectil
        Debug.Log($"[{GetType().Name}] Impactó objeto sólido NO trigger: {other.name}, Destruyendo.");
        Destroy(gameObject);
    }
}