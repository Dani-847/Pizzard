using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    protected Rigidbody2D rb;
    public float damage = 1f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "EnemyProjectile";
        
        if (rb != null && speed > 0)
        {
            rb.velocity = transform.right * speed;
        }
        
        Destroy(gameObject, lifetime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"@ EnemyProjectile collision6 con: {other.name} (Tag: {other.tag})");

        // Si ya fue reflejado, usar lógica de CharacterProjectile
        if (gameObject.CompareTag("CharacterProjectile"))
        {
            // Buscar componente CharacterProjectile y usar su lógica
            CharacterProjectile charProj = GetComponent<CharacterProjectile>();
            if (charProj != null)
            {
                // Simular la lógica de CharacterProjectile aquí o permitir que CharacterProjectile maneje esto
                return;
            }
        }
        
        // Si todavía es enemigo, continuar con la lógica normal
        // NO dañar al BOSS (proyectiles enemigos no afectan al boss)
        if (other.CompareTag("Boss") || other.CompareTag("EnemyProjectile"))
        {
            Debug.Log($"@ Proyectil enemigo ignorado por boss: {other.tag}");
            return;
        }
        
        // Dañar al JUGADOR
        if (other.CompareTag("Player"))
        {
            PlayerHPController playerHealth = other.GetComponent<PlayerHPController>();
            if (playerHealth != null)
            {
                playerHealth.RecibirDaño((int)damage);
                Debug.Log($"@ {GetType().Name} golpe6 al jugador: {damage} de daño");
            }
            Destroy(gameObject);
            return;
        }
        
        // Evitar colisiones con partes del jugador
        if (other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;
            
        // Para cualquier otra collision, destruir el proyectil
        Debug.Log($"$ {GetType().Name} impact6 con: {other.name} - Destruyendo");  
        Destroy(gameObject);  
    }
}