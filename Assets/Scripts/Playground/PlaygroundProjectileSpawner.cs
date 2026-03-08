// Assets/Scripts/Playground/PlaygroundProjectileSpawner.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a falling projectile hazard from a fixed position every 3 seconds.
/// Respects Time.timeScale — pauses when the shop is open (timeScale = 0).
/// The spawned projectile uses EnemyProjectile which damages the player on contact.
/// </summary>
public class PlaygroundProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab; // EnemyProjectile prefab
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float projectileSpeed = 5f;

    private void Start() => StartCoroutine(SpawnLoop());

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Do not spawn while shop is open (Time.timeScale = 0)
            if (Time.timeScale > 0f)
            {
                GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

                // Override EnemyProjectile's horizontal velocity with a straight-down velocity.
                // EnemyProjectile.Start() sets rb.velocity = transform.right * speed;
                // We override that in the next frame via a coroutine so our assignment wins.
                StartCoroutine(ApplyDownwardVelocity(proj));
            }
        }
    }

    /// <summary>
    /// Waits one frame for EnemyProjectile.Start() to run, then overrides with downward velocity.
    /// </summary>
    private IEnumerator ApplyDownwardVelocity(GameObject proj)
    {
        yield return null; // wait for EnemyProjectile.Start() to execute

        if (proj == null) yield break; // destroyed already

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.down * projectileSpeed;
    }
}
