using UnityEngine;
using System.Collections;
using Pizzard.Core;
using Pizzard.Bosses;

public class QuesoQuesoQuesoBlackHole : MonoBehaviour
{
    [Header("Settings")]
    public float absorbDuration = GameBalance.Spells.QuesoQuesoQueso.AbsorbDuration;
    public float absorbRadius = GameBalance.Spells.QuesoQuesoQueso.AbsorbRadius;
    public float returnDamageMultiplier = GameBalance.Spells.QuesoQuesoQueso.ReturnDamageMultiplier;
    public float returnProjectileSpeed = GameBalance.Spells.QuesoQuesoQueso.ReturnProjectileSpeed;

    private float absorbedDamage = 0f;
    private int absorbedCount = 0;
    private bool absorbing = true;

    public static GameObject CurrentActiveBlackHole;

    private void Start()
    {
        if (CurrentActiveBlackHole != null)
        {
            Destroy(CurrentActiveBlackHole);
        }
        CurrentActiveBlackHole = gameObject;

        // Configurar collider de absorción
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = absorbRadius;
            col.isTrigger = true;
        }

        StartCoroutine(BlackHoleRoutine());
    }

    private IEnumerator BlackHoleRoutine()
    {
        // Fase 1: Absorción
        yield return new WaitForSeconds(absorbDuration);

        // Fase 2: Devolver proyectiles
        absorbing = false;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null) col.enabled = false;

        if (absorbedCount > 0)
        {
            GameObject nearestBoss = FindNearestBoss();
            if (nearestBoss != null)
            {
                float totalReturnDamage = absorbedDamage * returnDamageMultiplier;
                float damagePerProjectile = totalReturnDamage / absorbedCount;

                for (int i = 0; i < absorbedCount; i++)
                {
                    Vector2 dir = ((Vector2)(nearestBoss.transform.position - transform.position)).normalized;
                    
                    // Agregar algo de dispersión para que no vayan todos al mismo punto
                    float spread = (i - absorbedCount / 2f) * 10f;
                    dir = Quaternion.Euler(0, 0, spread) * dir;

                    // Crear un proyectil de retorno simple
                    SpawnReturnProjectile(dir, damagePerProjectile);

                    // Pequeño delay entre cada proyectil, como ráfaga
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void SpawnReturnProjectile(Vector2 direction, float damage)
    {
        // Creamos un proyectil simple de retorno en lugar de depender de un prefab
        GameObject proj = new GameObject("ReturnProjectile");
        proj.transform.position = transform.position;
        proj.tag = "CharacterProjectile";
        proj.layer = LayerMask.NameToLayer("PlayerAttacks");

        SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.8f, 0f, 0.9f); // Amarillo dorado
        sr.sortingOrder = 5;
        proj.transform.localScale = Vector3.one * 0.3f;

        Rigidbody2D rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.velocity = direction * returnProjectileSpeed;

        CircleCollider2D col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        // Añadir script de daño al impacto
        ReturnProjectileDamage dmg = proj.AddComponent<ReturnProjectileDamage>();
        dmg.damage = damage;

        Destroy(proj, 5f); // Auto-destruir si no impacta en 5 segundos
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!absorbing) return;

        if (other.CompareTag("EnemyProjectile"))
        {
            // Absorber el proyectil
            absorbedDamage += 10f; // Daño base por proyectil absorbido
            absorbedCount++;

            // Efecto visual: escalar ligeramente el agujero negro
            float scaleBonus = 1f + (absorbedCount * 0.05f);
            transform.localScale = Vector3.one * Mathf.Min(scaleBonus, 2f);

            Destroy(other.gameObject);
            Debug.Log($"🕳️ Agujero negro absorbió proyectil #{absorbedCount}. Daño acumulado: {absorbedDamage}");
        }
    }

    private GameObject FindNearestBoss()
    {
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        if (bosses.Length == 0) return null;

        GameObject nearest = bosses[0];
        float minDist = Vector2.Distance(transform.position, nearest.transform.position);

        foreach (var b in bosses)
        {
            float d = Vector2.Distance(transform.position, b.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = b;
            }
        }

        return nearest;
    }

    private void OnDestroy()
    {
        if (CurrentActiveBlackHole == gameObject)
        {
            CurrentActiveBlackHole = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, absorbRadius);
    }
}
