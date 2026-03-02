using UnityEngine;
using System.Collections;

public class PineapplePepperoniAttack : MonoBehaviour
{
    [Header("Teleport Settings")]
    public float teleportDistance = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.TeleportDistance;
    public float teleportDelay = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.TeleportDelay;
    public LayerMask obstacleLayers;
    public float playerRadius = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.PlayerRadius; // Radio aproximado del jugador

    [Header("Explosion Settings")]
    public float explosionRadius = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.ExplosionRadius;
    public float explosionDamage = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.ExplosionDamage;
    public float explosionForce = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.ExplosionForce;
    public float explosionDelay = Pizzard.Core.GameBalance.Spells.PineapplePepperoni.ExplosionDelay;

    [Header("Visual Effects")]
    public GameObject teleportEffect;
    public GameObject explosionEffect;

    private Transform player;
    private PlayerAimAndCast aimRef;
    private Rigidbody2D playerRb;
    private bool hasTeleported = false;

    public void Initialize(Transform playerTransform, PlayerAimAndCast aim)
    {
        player = playerTransform;
        aimRef = aim;
        playerRb = player.GetComponent<Rigidbody2D>();
        StartCoroutine(TeleportAndExplode());
    }

    private IEnumerator TeleportAndExplode()
    {
        if (player == null || aimRef == null)
        {
            Debug.LogError("❌ Referencias del jugador no establecidas en PineapplePepperoniAttack");
            Destroy(gameObject);
            yield break;
        }

        // 1. Mostrar efecto de teletransporte inicial (opcional)
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.position, Quaternion.identity);
        }

        // 2. Calcular dirección y posición de destino
        Vector3 aimDirection = aimRef.GetCurrentAimDirection();
        Vector3 teleportTarget = CalculateTeleportPosition(player.position, aimDirection);

        // 3. Esperar un poco antes del teletransporte
        yield return new WaitForSeconds(teleportDelay);

        // 4. Verificar si la posición de destino es válida
        if (IsPositionValid(teleportTarget))
        {
            // Teletransportar al jugador USANDO RIGIDBODY
            if (playerRb != null)
            {
                playerRb.MovePosition(teleportTarget);
            }
            else
            {
                player.position = teleportTarget;
            }
            hasTeleported = true;
            Debug.Log("✅ Teletransporte exitoso a posición válida");
        }
        else
        {
            // Buscar una posición alternativa válida
            Vector3 validPosition = FindValidPositionNearby(teleportTarget);
            if (IsPositionValid(validPosition))
            {
                if (playerRb != null)
                {
                    playerRb.MovePosition(validPosition);
                }
                else
                {
                    player.position = validPosition;
                }
                hasTeleported = true;
                Debug.Log("✅ Teletransporte a posición alternativa válida");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró posición válida para teletransporte");
                // No teletransportar, pero igual crear la explosión
                teleportTarget = player.position;
            }
        }

        // 5. Mostrar efecto de llegada (opcional)
        if (hasTeleported && teleportEffect != null)
        {
            Instantiate(teleportEffect, teleportTarget, Quaternion.identity);
        }

        // 6. Esperar un poco antes de la explosión
        yield return new WaitForSeconds(explosionDelay);

        // 7. Crear explosión
        CreateExplosion(teleportTarget);

        // 8. Destruir este objeto
        Destroy(gameObject);
    }

    private Vector3 CalculateTeleportPosition(Vector3 startPosition, Vector3 direction)
    {
        // Hacer un raycast para detectar obstáculos
        RaycastHit2D hit = Physics2D.Raycast(
            startPosition, 
            direction, 
            teleportDistance, 
            obstacleLayers
        );

        Vector3 targetPosition;
        
        if (hit.collider != null)
        {
            // Si hay un obstáculo, teletransportarse justo antes de chocar
            // Añadimos un pequeño margen adicional
            targetPosition = hit.point - (Vector2)(direction * (0.5f + playerRadius));
            Debug.Log($"📍 Teletransporte bloqueado por: {hit.collider.name}. Nueva posición: {targetPosition}");
        }
        else
        {
            // Si no hay obstáculos, teletransportarse a la distancia máxima
            targetPosition = startPosition + (direction * teleportDistance);
        }

        return targetPosition;
    }

    private bool IsPositionValid(Vector3 position)
    {
        // Verificar si la posición está libre de colisiones
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, playerRadius, obstacleLayers);
        return colliders.Length == 0;
    }

    private Vector3 FindValidPositionNearby(Vector3 invalidPosition)
    {
        // Intentar encontrar una posición válida cerca de la posición inválida
        Vector3[] directions = {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.up + Vector3.left,
            Vector3.up + Vector3.right,
            Vector3.down + Vector3.left,
            Vector3.down + Vector3.right
        };

        foreach (Vector3 dir in directions)
        {
            Vector3 testPosition = invalidPosition + dir * (playerRadius * 2f);
            if (IsPositionValid(testPosition))
            {
                Debug.Log($"📍 Encontrada posición válida alternativa en dirección {dir}");
                return testPosition;
            }
        }

        // Si no se encuentra ninguna posición válida, devolver la posición original del jugador
        return player.position;
    }

    protected virtual void CreateExplosion(Vector3 position)
    {
        // Mostrar efecto visual de explosión
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, position, Quaternion.identity);
        }

        // Aplicar daño y fuerza a enemigos en el área
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, explosionRadius);
        
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Boss") || collider.CompareTag("Enemy"))
            {
                // Aplicar daño
                PblobController boss = collider.GetComponent<PblobController>();
                if (boss != null)
                {
                    boss.TakeDamage(explosionDamage);
                }
                
                var bossBase = collider.GetComponent<Pizzard.Bosses.BossBase>();
                if (bossBase != null)
                {
                    bossBase.TakeDamage((int)explosionDamage);
                }

                // Aplicar fuerza de explosión
                Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 explosionDir = (collider.transform.position - position).normalized;
                    rb.AddForce(explosionDir * explosionForce, ForceMode2D.Impulse);
                }
            }
        }

        Debug.Log($"💥 Explosión creada en {position}. Radio: {explosionRadius}, Daño: {explosionDamage}");
    }

    // Dibujar gizmos para debug en el editor
    private void OnDrawGizmosSelected()
    {
        if (hasTeleported && player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, explosionRadius);
            
            // Dibujar el radio del jugador para debug
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, playerRadius);
        }
    }
}