using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCheeseShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public float knockbackForce = 5f;
    public float reflectionSpeedMultiplier = 1.5f;
    public float shieldDuration = 3f;

    [Header("Contact Damage Over Time")]
    public float contactDamagePerTick = 5f;
    public float contactTickInterval = 0.5f;

    [Header("Reflection Cooldown")]
    public float reflectionCooldown = 0.5f;

    public static StaticCheeseShield CurrentActiveShield { get; private set; }

    private bool isActive = true;
    private HashSet<GameObject> projectilesOnCooldown = new HashSet<GameObject>();
    private Dictionary<GameObject, Coroutine> contactDamageCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        Debug.Log("🧀 StaticCheeseShield Start() llamado");

        if (CurrentActiveShield != null && CurrentActiveShield != this)
        {
            Debug.Log("🧀 Reemplazando escudo estático anterior por uno nuevo");
            CurrentActiveShield.ForceDestroy();
        }

        CurrentActiveShield = this;

        // Configurar componentes
        gameObject.tag = "CharacterProjectile";

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) 
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // Congelar completamente
        }

        StartCoroutine(ShieldLifetime());
        Debug.Log("🧀 Escudo de queso estático creado correctamente!");
    }

    // Método para inicializar el escudo en una posición específica
    public void InitializeAtPosition(Vector3 position)
    {
        Debug.Log($"🧀 Inicializando escudo estático en posición: {position}");
        transform.position = position;
        
        // El escudo estático no rota, mantiene rotación cero
        transform.rotation = Quaternion.identity;
    }

    IEnumerator ShieldLifetime()
    {
        yield return new WaitForSeconds(shieldDuration);
        DestroyShield();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // Reflejo de proyectiles enemigos
        if (other.CompareTag("EnemyProjectile"))
        {
            if (!projectilesOnCooldown.Contains(other.gameObject))
                ReflectProjectile(other.gameObject);
            return;
        }

        // Daño por contacto con jefes
        if (other.CompareTag("Boss"))
        {
            if (!contactDamageCoroutines.ContainsKey(other.gameObject))
            {
                Coroutine c = StartCoroutine(ApplyContactDamageOverTime(other.gameObject));
                contactDamageCoroutines.Add(other.gameObject, c);
            }
            return;
        }

        // Ignorar colisiones con el jugador y otros proyectiles aliados
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile") ||
            other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boss") && contactDamageCoroutines.ContainsKey(other.gameObject))
        {
            StopCoroutine(contactDamageCoroutines[other.gameObject]);
            contactDamageCoroutines.Remove(other.gameObject);
        }
    }

    private IEnumerator ApplyContactDamageOverTime(GameObject target)
    {
        while (target != null && isActive)
        {
            if (target.CompareTag("Boss"))
            {
                PblobController boss = target.GetComponent<PblobController>();
                if (boss != null && boss.IsVulnerable())
                    boss.TakeDamage(contactDamagePerTick);
            }

            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (target.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce * 0.1f, ForceMode2D.Impulse);
            }

            yield return new WaitForSeconds(contactTickInterval);
        }

        if (target != null && contactDamageCoroutines.ContainsKey(target))
            contactDamageCoroutines.Remove(target);
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
                // Para un escudo estático, reflejamos desde el punto de impacto
                Vector2 toProjectile = (projectile.transform.position - transform.position).normalized;
                rb.velocity = toProjectile * rb.velocity.magnitude * reflectionSpeedMultiplier;

                float angle = Mathf.Atan2(toProjectile.y, toProjectile.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            enemyProj.enabled = false;

            CharacterProjectile cp = projectile.GetComponent<CharacterProjectile>();
            if (cp == null) cp = projectile.AddComponent<CharacterProjectile>();
            cp.damage = enemyProj.damage;
            cp.speed = rb.velocity.magnitude;
        }
    }

    IEnumerator RemoveFromCooldown(GameObject proj)
    {
        yield return new WaitForSeconds(reflectionCooldown);
        if (proj != null) projectilesOnCooldown.Remove(proj);
    }

    public void ForceDestroy()
    {
        DestroyShield();
    }

    private void DestroyShield()
    {
        if (!isActive) return;

        isActive = false;

        if (CurrentActiveShield == this)
            CurrentActiveShield = null;

        StopAllCoroutines();
        projectilesOnCooldown.Clear();

        foreach (var c in contactDamageCoroutines.Values)
            if (c != null) StopCoroutine(c);

        contactDamageCoroutines.Clear();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (CurrentActiveShield == this)
            CurrentActiveShield = null;
    }

    public static bool IsShieldActive => CurrentActiveShield != null;
}