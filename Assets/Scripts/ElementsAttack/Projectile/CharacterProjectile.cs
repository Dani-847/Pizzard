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
        Debug.Log($"CharacterProjectile collision con: {other.name} (Tag: {other.tag})");

        // NO dañar al jugador ni aliados
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile"))
        {
            Debug.Log($"Proyectil aliado ignorado por aliado: {other.tag}");
            return;
        }
        
        // Dañar al BOSS
        if (other.CompareTag("Boss"))
        {
            PblobController boss = other.GetComponent<PblobController>();
            if (boss != null)
            {
                // Always call TakeDamage — it handles vulnerability checks internally
                // and logs "Boss no vulnerable" when blocked
                boss.TakeDamage(damage);
                Debug.Log($"@{GetType().Name} golpe al boss: {damage} de daño");
            }
            Destroy(gameObject);
            return;
        }

        // Evitar colisiones con partes del jugador
        if (other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;

        // Para cualquier otra colisión, destruir el proyectil
        Debug.Log($"@{GetType().Name} impact con: {other.name} - Destruyendo");
        Destroy(gameObject);
    }
}