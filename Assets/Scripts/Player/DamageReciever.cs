using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    private PlayerHPController playerHealth;

    void Start()
    {
        playerHealth = GetComponentInParent<PlayerHPController>();
        if (playerHealth == null)
            Debug.LogError("❌ DamageReceiver no encontró PlayerHPController");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ CAMBIO: Detectar EnemyProjectile en lugar de Projectile
        EnemyProjectile enemyProjectile = other.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null && playerHealth != null)
        {
            Debug.Log($"💥 Jugador golpeado por: {enemyProjectile.name}");
            Debug.Log($"💥 Daño del proyectil: {enemyProjectile.damage}");
            playerHealth.RecibirDaño((int)enemyProjectile.damage);
            Destroy(other.gameObject);
        }
        
        // ✅ OPCIÓN ALTERNATIVA: También detectar por tag por si acaso
        else if (other.CompareTag("EnemyProjectile") && playerHealth != null)
        {
            Debug.Log($"💥 Jugador golpeado por proyectil enemigo (por tag): {other.name}");
            playerHealth.RecibirDaño(1); // 1 medio corazón de daño
            Destroy(other.gameObject);
        }
    }
}