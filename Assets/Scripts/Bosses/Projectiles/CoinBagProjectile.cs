using UnityEngine;

/// <summary>
/// Niggel's primary projectile. Yellow coin bag thrown toward the player.
/// Destroys on wall contact. Deals CoinBagDamage to the player.
/// Speed is set by NiggelController at spawn time (overrides Rigidbody2D velocity directly).
/// </summary>
public class CoinBagProjectile : EnemyProjectile
{
    protected override void Start()
    {
        base.Start();
        damage = Pizzard.Core.GameBalance.Bosses.Niggel.CoinBagDamage;
        // NiggelController sets rb.velocity after Instantiate, so we do NOT set it here.
        // base.Start() already applies rb.velocity = transform.right * speed,
        // but NiggelController overwrites it immediately after spawn.
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy on wall contact before any other logic
        if (other.CompareTag("Wall")) { Destroy(gameObject); return; }
        // Delegate player damage and other collision handling to base
        base.OnTriggerEnter2D(other);
    }
}
