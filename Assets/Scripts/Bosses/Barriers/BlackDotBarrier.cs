using UnityEngine;

/// <summary>
/// Niggel Enrage 1 barrier dot. Placed in rows across the arena.
/// - Player (Default layer) walks through — IgnoreLayerCollision at runtime.
/// - Enemy projectiles pass through — IgnoreLayerCollision at runtime.
/// - Player spells (PlayerProjectiles layer) are destroyed on trigger contact.
///
/// SETUP REQUIRED IN UNITY EDITOR:
///   1. Create "BossBarrier" layer in Edit > Project Settings > Tags and Layers.
///   2. Assign all BlackDotBarrier GameObjects to the "BossBarrier" layer.
///   3. The barrier collider must have "Is Trigger" = true.
/// </summary>
public class BlackDotBarrier : MonoBehaviour
{
private void Awake() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy any player spell that contacts this barrier, and the barrier itself
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerProjectiles") || other.CompareTag("CharacterProjectile"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerProjectiles") || collision.collider.CompareTag("CharacterProjectile"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else if (collision.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
