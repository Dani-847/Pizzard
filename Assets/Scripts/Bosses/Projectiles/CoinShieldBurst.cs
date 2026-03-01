using UnityEngine;

/// <summary>
/// Helper component placed on NiggelController's GameObject.
/// Called by NiggelController.CoinShieldRoutine() to fire N orange projectiles
/// evenly spread in a full circle from Niggel's position.
/// Uses the coinBagPrefab with an orange tint override.
/// </summary>
public class CoinShieldBurst : MonoBehaviour
{
    // No serialized fields — NiggelController calls Burst() directly with all needed data.

    /// <summary>
    /// Fires <paramref name="count"/> projectiles in a full-circle spread from this transform's position.
    /// </summary>
    /// <param name="coinBagPrefab">Prefab to instantiate (CoinBagProjectile).</param>
    /// <param name="speed">Velocity magnitude for each burst projectile.</param>
    /// <param name="count">Number of projectiles (evenly spread 360 degrees).</param>
    public void Burst(GameObject coinBagPrefab, float speed, int count)
    {
        if (coinBagPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            float angleDeg = (360f / count) * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            Quaternion rotation = Quaternion.Euler(0f, 0f, angleDeg);
            GameObject proj = Instantiate(coinBagPrefab, transform.position, rotation);

            // Override velocity (NiggelController also sets it but we control it here for burst)
            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * speed;

            // Tint orange to distinguish burst from regular coin bags
            var sr = proj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 0.5f, 0f);
        }
    }
}
