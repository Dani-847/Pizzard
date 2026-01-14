using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheesePepperoniWall : MonoBehaviour
{
    [Header("Wall Settings")]
    public float wallDuration = 4f;
    public float knockbackForce = 3f;

    [Header("Projectile Reflection")]
    public float reflectionSpeedMultiplier = 1.2f;
    public float reflectionCooldown = 0.3f;

    [Header("Pepperoni Burn Effect")]
    public StatusType burnStatusEffect = StatusType.picante;
    public float burnEffectDuration = 7f;
    public int burnInitialStacks = 2;

    public static CheesePepperoniWall CurrentActiveWall { get; private set; }

    private bool isActive = true;
    private HashSet<GameObject> projectilesOnCooldown = new HashSet<GameObject>();

    void Start()
    {
        Debug.Log("🧀🔥 CheesePepperoniWall Start() llamado");

        // Gestionar instancia única
        if (CurrentActiveWall != null && CurrentActiveWall != this)
        {
            Debug.Log("🧀🔥 Reemplazando muro anterior por uno nuevo");
            CurrentActiveWall.ForceDestroy();
        }

        CurrentActiveWall = this;

        // Configurar componentes físicos
        gameObject.tag = "CharacterProjectile";

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) 
        {
            col.isTrigger = true;
            Debug.Log("🧀🔥 Collider configurado como trigger");
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) 
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Iniciar duración del muro
        StartCoroutine(WallLifetime());
        Debug.Log("🧀🔥 Muro de queso y pepperoni creado correctamente!");
    }

    public void InitializeAtPosition(Vector3 position)
    {
        Debug.Log($"🧀🔥 Inicializando muro en posición: {position}");
        transform.position = position;
        transform.rotation = Quaternion.identity;
    }

    IEnumerator WallLifetime()
    {
        yield return new WaitForSeconds(wallDuration);
        DestroyWall();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        Debug.Log($"🧀🔥 Trigger enter con: {other.tag} - {other.name}");

        // Reflejo de proyectiles enemigos
        if (other.CompareTag("EnemyProjectile"))
        {
            if (!projectilesOnCooldown.Contains(other.gameObject))
            {
                ReflectAndApplyBurnEffect(other.gameObject);
            }
            return;
        }

        // Ignorar colisiones con jugador y proyectiles aliados
        if (other.CompareTag("Player") || other.CompareTag("CharacterProjectile") ||
            other.CompareTag("CastPoint") || other.CompareTag("Weapon"))
            return;
    }

    private void ReflectAndApplyBurnEffect(GameObject projectile)
    {
        Debug.Log("🧀🔥 Reflejando proyectil y aplicando efecto de quemado");

        projectilesOnCooldown.Add(projectile);
        StartCoroutine(RemoveFromCooldown(projectile));

        EnemyProjectile enemyProj = projectile.GetComponent<EnemyProjectile>();
        if (enemyProj != null)
        {
            // Cambiar tag para que no dañe al jugador
            projectile.tag = "CharacterProjectile";

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Reflejar el proyectil
                Vector2 reflection = -rb.velocity.normalized;
                float speed = rb.velocity.magnitude;
                rb.velocity = reflection * speed * reflectionSpeedMultiplier;

                // Rotar el proyectil en la dirección del movimiento
                float angle = Mathf.Atan2(reflection.y, reflection.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Deshabilitar el componente EnemyProjectile
            enemyProj.enabled = false;

            // Verificar si ya tiene un CharacterProjectile
            CharacterProjectile existingCharProj = projectile.GetComponent<CharacterProjectile>();
            
            // Si no tiene CharacterProjectile, añadirlo
            if (existingCharProj == null)
            {
                existingCharProj = projectile.AddComponent<CharacterProjectile>();
                existingCharProj.damage = enemyProj.damage;
                existingCharProj.speed = rb.velocity.magnitude;
                Debug.Log("🧀🔥 Añadido CharacterProjectile básico al proyectil reflejado");
            }
            else
            {
                // Si ya tiene, actualizar el daño
                existingCharProj.damage = enemyProj.damage;
                existingCharProj.speed = rb.velocity.magnitude;
            }

            // Añadir o configurar componente PepperoniAttack
            PepperoniAttack pepperoniAttack = projectile.GetComponent<PepperoniAttack>();
            if (pepperoniAttack == null)
            {
                pepperoniAttack = projectile.AddComponent<PepperoniAttack>();
                
                // Configurar parámetros del quemado
                pepperoniAttack.statusEffect = burnStatusEffect;
                pepperoniAttack.effectDuration = burnEffectDuration;
                pepperoniAttack.initialStacks = burnInitialStacks;
                
                // Configurar propiedades heredadas de CharacterProjectile
                pepperoniAttack.damage = enemyProj.damage;
                pepperoniAttack.speed = rb.velocity.magnitude;
                
                Debug.Log($"🔥 Componente PepperoniAttack añadido: {burnInitialStacks} stacks de {burnStatusEffect} por {burnEffectDuration}s");
            }
            else
            {
                // Si ya tiene el componente, actualizar los parámetros
                pepperoniAttack.statusEffect = burnStatusEffect;
                pepperoniAttack.effectDuration = burnEffectDuration;
                pepperoniAttack.initialStacks = burnInitialStacks;
                Debug.Log($"🔥 Componente PepperoniAttack actualizado: {burnInitialStacks} stacks de {burnStatusEffect}");
            }

            Debug.Log($"🧀🔥 Proyectil convertido con efecto de quemado. Daño: {enemyProj.damage}, Velocidad: {rb.velocity.magnitude}");
        }
        else
        {
            Debug.LogWarning("🧀🔥 El proyectil no tiene componente EnemyProjectile");
        }
    }

    IEnumerator RemoveFromCooldown(GameObject proj)
    {
        yield return new WaitForSeconds(reflectionCooldown);
        if (proj != null) 
            projectilesOnCooldown.Remove(proj);
    }

    public void ForceDestroy()
    {
        DestroyWall();
    }

    private void DestroyWall()
    {
        if (!isActive) return;

        isActive = false;

        if (CurrentActiveWall == this)
            CurrentActiveWall = null;

        StopAllCoroutines();
        projectilesOnCooldown.Clear();

        Destroy(gameObject);
        Debug.Log("🧀🔥 Muro destruido");
    }

    void OnDestroy()
    {
        if (CurrentActiveWall == this)
            CurrentActiveWall = null;
    }

    public static bool IsWallActive => CurrentActiveWall != null;
}