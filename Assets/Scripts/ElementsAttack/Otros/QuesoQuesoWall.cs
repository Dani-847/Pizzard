using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoQuesoWall : MonoBehaviour
{
    [Header("Settings")]
    public float wallDuration = GameBalance.Spells.QuesoQueso.WallDuration;
    public float maxHP = GameBalance.Spells.QuesoQueso.WallHP;
    public float tickDamage = GameBalance.Spells.QuesoQueso.TickDamage;
    public float tickInterval = GameBalance.Spells.QuesoQueso.TickInterval;
    public float reflectionSpeedMultiplier = GameBalance.Spells.QuesoQueso.ReflectionSpeedMultiplier;
    public float reflectionCooldown = GameBalance.Spells.QuesoQueso.ReflectionCooldown;
    
    // Add WallProjectileDamage if not in GameBalance yet (fallback to 10f)
    public float projectileDamage = 10f; 

    public static QuesoQuesoWall CurrentActiveWall { get; private set; }

    private float currentHP;
    private bool isActive = true;
    private HashSet<GameObject> projectilesOnCooldown = new HashSet<GameObject>();

    private void Start()
    {
        if (CurrentActiveWall != null && CurrentActiveWall != this)
        {
            CurrentActiveWall.ForceDestroy();
        }

        CurrentActiveWall = this;
        currentHP = maxHP;

        gameObject.tag = "CharacterProjectile";

        StartCoroutine(WallLifetime());
        StartCoroutine(TickDamageCoroutine());
    }

    private IEnumerator WallLifetime()
    {
        yield return new WaitForSeconds(wallDuration);
        ForceDestroy();
    }

    private IEnumerator TickDamageCoroutine()
    {
        while (isActive)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("Boss"))
                {
                    BossBase boss = collider.GetComponent<BossBase>();
                    if (boss == null) boss = collider.GetComponentInParent<BossBase>();
                    
                    if (boss != null)
                    {
                        boss.TakeDamage((int)tickDamage);
                    }
                    else
                    {
                        PblobController pblob = collider.GetComponent<PblobController>();
                        if (pblob != null)
                        {
                            pblob.TakeDamage(tickDamage);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("EnemyProjectile") || other.GetComponent<EnemyProjectile>() != null)
        {
            if (!projectilesOnCooldown.Contains(other.gameObject))
            {
                ReflectProjectile(other.gameObject);
                TakeWallDamage(projectileDamage);
            }
        }
    }

    private void ReflectProjectile(GameObject projectile)
    {
        projectilesOnCooldown.Add(projectile);
        StartCoroutine(RemoveFromCooldown(projectile));

        EnemyProjectile enemyProj = projectile.GetComponent<EnemyProjectile>();
        if (enemyProj != null)
        {
            projectile.tag = "CharacterProjectile";

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 reflection = -rb.velocity.normalized;
                float speed = rb.velocity.magnitude;
                rb.velocity = reflection * speed * reflectionSpeedMultiplier;

                float angle = Mathf.Atan2(reflection.y, reflection.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            enemyProj.enabled = false;

            CharacterProjectile existingCharProj = projectile.GetComponent<CharacterProjectile>();
            if (existingCharProj == null)
            {
                existingCharProj = projectile.AddComponent<CharacterProjectile>();
                existingCharProj.damage = enemyProj.damage;
                existingCharProj.speed = rb != null ? rb.velocity.magnitude : 10f;
            }
            else
            {
                existingCharProj.damage = enemyProj.damage;
                existingCharProj.speed = rb != null ? rb.velocity.magnitude : 10f;
            }
        }
    }

    private IEnumerator RemoveFromCooldown(GameObject proj)
    {
        yield return new WaitForSeconds(reflectionCooldown);
        if (proj != null) 
            projectilesOnCooldown.Remove(proj);
    }

    public void TakeWallDamage(float amount)
    {
        currentHP -= amount;
        Debug.Log($"QuesoQuesoWall took {amount} damage. HP: {currentHP}/{maxHP}");
        if (currentHP <= 0)
        {
            ForceDestroy();
        }
    }

    public void ForceDestroy()
    {
        if (!isActive) return;
        isActive = false;

        if (CurrentActiveWall == this)
            CurrentActiveWall = null;

        StopAllCoroutines();
        projectilesOnCooldown.Clear();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (CurrentActiveWall == this)
            CurrentActiveWall = null;
    }
}
