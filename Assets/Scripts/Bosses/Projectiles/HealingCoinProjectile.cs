using UnityEngine;
using Pizzard.Player;

/// <summary>
/// Niggel Enrage 2+ attack. A fast coin projectile that heals Niggel's CoinVault
/// by HealingCoinAmount when it contacts the player. Does NOT deal damage.
/// Pulses yellow-to-orange visually via SpriteRenderer lerp.
/// </summary>
public class HealingCoinProjectile : EnemyProjectile
{
    /// <summary>Set by NiggelController at spawn time.</summary>
    public Pizzard.Bosses.NiggelController boss;

    private float pulseTimer = 0f;
    private SpriteRenderer sr;
    private readonly Color baseColor = Color.yellow;
    private readonly Color pulseColor = new Color(1f, 0.5f, 0f); // orange

    /// <summary>Set to true for shield-burst coins — they heal Niggel AND damage the player.</summary>
    public bool dealsDamageOnHit = false;

    protected override void Start()
    {
        base.Start();
        damage = 0f; // default: no player damage (overridden in OnTriggerEnter2D when dealsDamageOnHit)
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Pulse yellow <-> orange
        pulseTimer += Time.deltaTime * 3f;
        if (sr != null)
            sr.color = Color.Lerp(baseColor, pulseColor, Mathf.PingPong(pulseTimer, 1f));
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss?.HealCoinVault(Pizzard.Core.GameBalance.Bosses.Niggel.HealingCoinAmount);
            if (dealsDamageOnHit)
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(1);
            }
            Destroy(gameObject);
            return;
        }
        if (other.CompareTag("Wall")) { Destroy(gameObject); return; }
        // Ignore Boss, EnemyProjectile tags (same as base logic — but no player damage)
        if (other.CompareTag("Boss") || other.CompareTag("EnemyProjectile")) return;
        // Ignore cast point / weapon
        if (other.CompareTag("CastPoint") || other.CompareTag("Weapon")) return;
        // Anything else: destroy
        Destroy(gameObject);
    }
}
