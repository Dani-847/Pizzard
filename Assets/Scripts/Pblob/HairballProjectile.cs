using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairballProjectile : EnemyProjectile
{
    [Header("Hairball Visual Effects")]
    public ParticleSystem trailParticles;
    public ParticleSystem impactParticles;
    
    [Header("Hairball Sounds")]
    public AudioClip shootSound;
    public AudioClip bounceSound;
    
    [Header("Hairball Bounce Settings")]
    public bool canBounce = false;
    public int maxBounces = 3;
    public float bounceSpeedMultiplier = 0.8f;
    
    [Header("Collision Settings")]
    public float penetrationCorrection = 0.1f; // ✅ Cuánto puede penetrar antes de corregir
    public float colliderSizeMultiplier = 1.2f; // ✅ Aumentar tamaño del collider
    
    private int currentBounces = 0;
    private AudioSource audioSource;
    private Vector2 lastVelocity;
    private Vector2 previousPosition;
    private CircleCollider2D circleCollider;
    private bool isCorrectingPenetration = false;

    protected override void Start()
    {
        base.Start();
        
        // ✅ Obtener referencia al collider y aumentarlo
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.radius *= colliderSizeMultiplier;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (shootSound != null) audioSource.PlayOneShot(shootSound);
        if (trailParticles != null) trailParticles.Play();
        
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (rb != null) 
        {
            lastVelocity = rb.velocity;
            previousPosition = transform.position;
        }
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ NUEVO: No dañar al boss que lanzó el proyectil
        if (other.CompareTag("Boss"))
        {
            return;
        }
        
        if (canBounce && other.CompareTag("Wall") && currentBounces < maxBounces)
        {
            HandleBounce(other);
            return;
        }
        
        base.OnTriggerEnter2D(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // ✅ NUEVO: Corregir penetración si el hairball está dentro de la pared
        if (canBounce && other.CompareTag("Wall") && !isCorrectingPenetration)
        {
            StartCoroutine(CorrectPenetration(other));
        }
    }

    private void HandleBounce(Collider2D wall)
    {
        // ✅ MEJORADO: Usar método más robusto para calcular la normal
        Vector2 wallNormal = GetRobustWallNormal(wall);
        
        // Aplicar rebote
        Vector2 bounceDirection = Vector2.Reflect(lastVelocity.normalized, wallNormal);
        rb.velocity = bounceDirection * speed * bounceSpeedMultiplier;
        
        // ✅ NUEVO: Corregir posición para sacar de la pared
        StartCoroutine(CorrectPositionAfterBounce(wall, wallNormal));
        
        // Rotar el proyectil
        float angle = Mathf.Atan2(bounceDirection.y, bounceDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Efectos
        if (bounceSound != null) audioSource.PlayOneShot(bounceSound);
        if (impactParticles != null) Instantiate(impactParticles, transform.position, Quaternion.identity);
        
        currentBounces++;
        
        Debug.Log($"🏀 Hairball rebotó ({currentBounces}/{maxBounces}). Normal: {wallNormal}");
    }

    // ✅ NUEVO: Método robusto para calcular normal
    private Vector2 GetRobustWallNormal(Collider2D wall)
    {
        // Intentar con raycast primero
        Vector2 raycastNormal = GetWallNormalWithRaycast(wall);
        if (raycastNormal != Vector2.zero)
            return raycastNormal;
        
        // Fallback al método de cálculo geométrico
        return GetAccurateWallNormal(wall);
    }

    // ✅ NUEVO: Corregir posición después del rebote
    private IEnumerator CorrectPositionAfterBounce(Collider2D wall, Vector2 wallNormal)
    {
        yield return new WaitForFixedUpdate(); // Esperar al próximo frame de física
        
        // Mover el proyectil fuera de la pared
        float correctionDistance = penetrationCorrection;
        Vector2 correction = wallNormal * correctionDistance;
        rb.position += correction;
        
        Debug.Log($"📍 Posición corregida: {correction}");
    }

    // ✅ NUEVO: Corregir penetración continua
    private IEnumerator CorrectPenetration(Collider2D wall)
    {
        isCorrectingPenetration = true;
        
        // Calcular la dirección para sacar el proyectil de la pared
        Vector2 toCenter = wall.bounds.center - (Vector3)rb.position;
        Vector2 escapeDirection = -toCenter.normalized;
        
        // Mover el proyectil fuera de la pared
        rb.position += escapeDirection * penetrationCorrection;
        
        Debug.Log($"🔄 Corrigiendo penetración en pared: {escapeDirection}");
        
        yield return new WaitForSeconds(0.1f);
        isCorrectingPenetration = false;
    }

    private Vector2 GetWallNormalWithRaycast(Collider2D wall)
    {
        // Disparar múltiples raycasts en diferentes direcciones
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
            new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
        };
        
        foreach (Vector2 dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f, LayerMask.GetMask("Wall"));
            if (hit.collider != null && hit.collider == wall)
            {
                return hit.normal;
            }
        }
        
        return Vector2.zero;
    }

    private Vector2 GetAccurateWallNormal(Collider2D wall)
    {
        Vector2 hairballPos = transform.position;
        Vector2 wallCenter = wall.bounds.center;
        Vector2 toHairball = hairballPos - wallCenter;
        
        float wallWidth = wall.bounds.size.x;
        float wallHeight = wall.bounds.size.y;
        
        float horizontalRatio = Mathf.Abs(toHairball.x) / (wallWidth * 0.5f);
        float verticalRatio = Mathf.Abs(toHairball.y) / (wallHeight * 0.5f);
        
        // ✅ MEJORADO: Usar un umbral más estricto para esquinas
        float cornerThreshold = 0.7f; // Si está muy cerca de la esquina (70% de ambas direcciones)
        
        if (horizontalRatio > cornerThreshold && verticalRatio > cornerThreshold)
        {
            // Es una esquina - usar la dirección del movimiento invertida
            Debug.Log("🎯 Esquina detectada, usando dirección inversa del movimiento");
            return -lastVelocity.normalized;
        }
        else if (horizontalRatio > verticalRatio)
        {
            return new Vector2(Mathf.Sign(toHairball.x), 0f);
        }
        else
        {
            return new Vector2(0f, Mathf.Sign(toHairball.y));
        }
    }

    // ✅ NUEVO: Método para debug visual mejorado
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Dirección del movimiento
            Gizmos.color = Color.red;
            Gizmos.DrawLine(previousPosition, transform.position);
            
            // Raycasts en todas las direcciones
            Gizmos.color = Color.blue;
            Vector2[] directions = {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right
            };
            
            foreach (Vector2 dir in directions)
            {
                Gizmos.DrawRay(transform.position, dir * 0.5f);
            }
        }
    }
}