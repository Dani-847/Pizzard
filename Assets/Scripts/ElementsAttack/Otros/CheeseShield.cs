using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public float knockbackForce = Pizzard.Core.GameBalance.Spells.Shields.KnockbackForce;
    public float reflectionSpeedMultiplier = Pizzard.Core.GameBalance.Spells.Shields.ReflectionSpeedMultiplier;
    public float shieldDuration = Pizzard.Core.GameBalance.Spells.Shields.ShieldDuration;

    [Header("Contact Damage Over Time")]
    public float contactDamagePerTick = Pizzard.Core.GameBalance.Spells.Shields.ContactDamagePerTick;
    public float contactTickInterval = Pizzard.Core.GameBalance.Spells.Shields.ContactTickInterval;

    [Header("Reflection Cooldown")]
    public float reflectionCooldown = Pizzard.Core.GameBalance.Spells.Shields.ReflectionCooldown;

    public static CheeseShield CurrentActiveShield { get; private set; }

    private Transform playerTransform;
    private PlayerAimAndCast aimRef;

    private float shieldDistance = Pizzard.Core.GameBalance.Spells.Shields.ShieldDistance;
    private SpriteRenderer spriteRenderer;
    private bool isActive = true;

    private HashSet<GameObject> projectilesOnCooldown = new HashSet<GameObject>();
    private Dictionary<GameObject, Coroutine> contactDamageCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        Debug.Log("🧀 CheeseShield Start() llamado");

        if (CurrentActiveShield != null && CurrentActiveShield != this)
        {
            Debug.Log("🧀 Reemplazando escudo anterior por uno nuevo");
            CurrentActiveShield.ForceDestroy();
        }

        CurrentActiveShield = this;

        spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.tag = "CharacterProjectile";

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        StartCoroutine(ShieldLifetime());
        Debug.Log("🧀 Escudo de queso creado correctamente!");
    }

    void Update()
    {
        if (!isActive) 
        {
            Debug.Log("🧀 Escudo no activo en Update");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("🧀 playerTransform es NULL en Update");
            return;
        }

        if (aimRef == null)
        {
            Debug.LogWarning("🧀 aimRef es NULL en Update");
            return;
        }

        // Obtener dirección de apuntado
        Vector3 aimDir = aimRef.GetCurrentAimDirection();
        
        Debug.Log($"🧀 Aim Direction: {aimDir}, Magnitude: {aimDir.magnitude}");

        if (aimDir.sqrMagnitude < 0.01f)
        {
            // Fallback: usar la dirección actual del cuerpo
            if (playerTransform != null)
            {
                aimDir = playerTransform.right;
                Debug.Log("🧀 Usando dirección fallback del cuerpo: " + aimDir);
            }
            else
            {
                Debug.LogWarning("🧀 No hay dirección de apuntado válida");
                return;
            }
        }

        // POSICIÓN: calcular posición basada en dirección de apuntado
        Vector3 targetPosition = playerTransform.position + (aimDir.normalized * shieldDistance);
        transform.position = targetPosition;

        Debug.Log($"🧀 Posicionando escudo. Player: {playerTransform.position}, Target: {targetPosition}");

        // ROTACIÓN
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Debug.Log($"🧀 Rotación aplicada: {angle} grados");
    }

    public void SetPlayer(Transform player, PlayerAimAndCast aim)
    {
        Debug.Log($"🧀 SetPlayer llamado - Player: {player != null}, Aim: {aim != null}");

        playerTransform = player;
        aimRef = aim;
        
        if (player == null)
        {
            Debug.LogError("🧀 ERROR: Player es null en SetPlayer");
            return;
        }

        if (aim == null)
        {
            Debug.LogError("🧀 ERROR: Aim es null en SetPlayer");
            return;
        }

        // Posicionar inmediatamente al recibir las referencias
        Vector3 aimDir = aim.GetCurrentAimDirection();
        if (aimDir.sqrMagnitude < 0.01f)
        {
            aimDir = player.right;
            Debug.Log("🧀 SetPlayer: Usando dirección fallback del cuerpo");
        }
            
        transform.position = player.position + (aimDir.normalized * shieldDistance);

        // Rotación inmediata también
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"🧀 SetPlayer completado - Posición: {transform.position}, Rotación: {angle}");
    }

    IEnumerator ShieldLifetime()
    {
        yield return new WaitForSeconds(shieldDuration);
        DestroyShield();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // Reflejo
        if (other.CompareTag("EnemyProjectile"))
        {
            if (!projectilesOnCooldown.Contains(other.gameObject))
                ReflectProjectile(other.gameObject);
            return;
        }

        // SOLO usar tag "Boss" - eliminar "Enemy"
        if (other.CompareTag("Boss"))
        {
            if (!contactDamageCoroutines.ContainsKey(other.gameObject))
            {
                Coroutine c = StartCoroutine(ApplyContactDamageOverTime(other.gameObject));
                contactDamageCoroutines.Add(other.gameObject, c);
            }
            return;
        }

        // Ignorar colisiones con el jugador
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile") ||
            other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // SOLO usar tag "Boss" - eliminar "Enemy"
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
                if (boss != null)
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
                Vector2 reflection = -rb.velocity.normalized;
                float speed = rb.velocity.magnitude;
                rb.velocity = reflection * speed * reflectionSpeedMultiplier;

                float angle = Mathf.Atan2(reflection.y, reflection.x) * Mathf.Rad2Deg;
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