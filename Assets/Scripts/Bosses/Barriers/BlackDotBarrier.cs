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
    private void Awake()
    {
        int barrierLayer = LayerMask.NameToLayer("BossBarrier");
        int playerLayer = LayerMask.NameToLayer("Default");
        int playerProjLayer = LayerMask.NameToLayer("PlayerProjectiles");
        int enemyProjLayer = LayerMask.NameToLayer("EnemyProjectile");

        if (barrierLayer < 0)
        {
            Debug.LogWarning("[BlackDotBarrier] 'BossBarrier' layer not found. " +
                             "Create it in Edit > Project Settings > Tags and Layers.");
            return;
        }

        // Player walks through freely
        if (playerLayer >= 0)
            Physics2D.IgnoreLayerCollision(barrierLayer, playerLayer, true);

        // Enemy projectiles pass through (don't block Niggel's own shots)
        if (enemyProjLayer >= 0)
            Physics2D.IgnoreLayerCollision(barrierLayer, enemyProjLayer, true);

        // PlayerProjectiles DO collide — handled by OnTriggerEnter2D below
        // (layer collision between BossBarrier and PlayerProjectiles must remain enabled)
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy any player spell that contacts this barrier
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerProjectiles"))
        {
            Destroy(other.gameObject);
        }
    }
}
