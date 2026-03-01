using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PblobAttackPattern1 : PblobAttackPattern
{
    [Header("Hairball Settings")]
    public GameObject hairballPrefab;
    public Transform[] mustachePoints;
    public float[] speedVariations = { 5f, 7f, 10f };

    [Header("Rhythm Configuration")]
    public bool useRhythm = true;
    public int beatsBetweenShots = 4;
    public int startingBeat = 1;

    [Header("Bounce Settings")]
    public bool enableBouncing = true;
    public int bounceCount = 3;
    public float bounceSpeedMultiplier = 0.95f;

    [Header("Phase Difficulty")]
    public int activeMustachePoints = 4; // ✅ Comienza con 4 puntos activos

    private Coroutine attackCoroutine;
    private bool isPatternActive = false;
    private PblobRhythmManager rhythmManager;
    private int lastProcessedBeat = -1;

    protected override void Start()
    {
        base.Start();
        if (mustachePoints != null)
        {
            foreach (var point in mustachePoints)
            {
                if (point != null) point.SetParent(null);
            }
        }
    }

    public override void StartPattern()
    {
        if (isPatternActive) return;

        // Buscar RhythmManager si no está asignado
        if (rhythmManager == null)
        {
            rhythmManager = FindObjectOfType<PblobRhythmManager>();
        }
        
        lastProcessedBeat = -1; // Resetear el último beat procesado
        
        if (rhythmManager == null)
        {
            DebugPattern("No se encontró RhythmManager - usando timing por defecto");
            attackCoroutine = StartCoroutine(AttackRoutineFallback());
            if (attackCoroutine != null)
            {
                isPatternActive = true; // Solo marcar activo si la corrutina inició
            }
            return;
        }

        if (useRhythm)
        {
            rhythmManager.OnBeat += OnMusicBeat;
            isPatternActive = true; // Marcar activo después de suscribirse exitosamente
            DebugPattern($"Iniciado - Ritmo: cada {beatsBetweenShots} beats - Puntos activos: {activeMustachePoints}");
        }
        else
        {
            attackCoroutine = StartCoroutine(AttackRoutineFallback());
            if (attackCoroutine != null)
            {
                isPatternActive = true; // Solo marcar activo si la corrutina inició
            }
        }
    }

    public override void StopPattern()
    {
        if (!isPatternActive) return;

        isPatternActive = false;

        if (rhythmManager != null)
        {
            rhythmManager.OnBeat -= OnMusicBeat;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        DebugPattern("Patrón detenido");
    }

    // ✅ ACTUALIZADO: Aumentar dificultad según puntos objetivo
    public void IncreaseDifficulty(int targetPoints)
    {
        activeMustachePoints = Mathf.Min(targetPoints, mustachePoints.Length);
        DebugPattern($"Dificultad aumentada - Puntos activos: {activeMustachePoints}");
    }

    private void OnMusicBeat(int beatNumber)
    {
        if (!isPatternActive) return;
        if (beatNumber <= lastProcessedBeat) return;

        lastProcessedBeat = beatNumber;

        if (beatNumber >= startingBeat && (beatNumber - startingBeat) % beatsBetweenShots == 0)
        {
            ShootHairball();
            DebugPattern($"Disparo en beat: {beatNumber} - Puntos: {activeMustachePoints}");
        }
    }

    private IEnumerator AttackRoutineFallback()
    {
        float fallbackInterval = rhythmManager != null ? 
            rhythmManager.GetBeatDuration() * beatsBetweenShots : 2f;

        yield return new WaitForSeconds(1f);

        while (isPatternActive)
        {
            ShootHairball();
            yield return new WaitForSeconds(fallbackInterval);
        }
    }

    private void ShootHairball()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            DebugPattern("No se encontró al jugador");
            return;
        }

        // ✅ MODIFICADO: Usar solo los puntos activos según la fase
        for (int i = 0; i < activeMustachePoints; i++)
        {
            if (i >= mustachePoints.Length) 
            {
                Debug.LogWarning("No hay suficientes mustachePoints para la fase actual");
                break;
            }

            var point = mustachePoints[i];
            if (point != null && hairballPrefab != null)
            {
                // Clamp spawn position to arena bounds so hairballs never appear through walls
                Vector3 spawnPos = point.position;
                if (bossController != null)
                {
                    Vector3 c = bossController.ArenaCenter;
                    float cx = bossController.ArenaClampX;
                    float cy = bossController.ArenaClampY;
                    spawnPos.x = Mathf.Clamp(spawnPos.x, c.x - cx, c.x + cx);
                    spawnPos.y = Mathf.Clamp(spawnPos.y, c.y - cy, c.y + cy);
                }
                GameObject hairball = Instantiate(hairballPrefab, spawnPos, point.rotation);

                float selectedSpeed = speedVariations[Random.Range(0, speedVariations.Length)];

                // ✅ ACTUALIZADO: Compatibilidad con tu HairballProjectile existente
                HairballProjectile hairballProjectile = hairball.GetComponent<HairballProjectile>();
                if (hairballProjectile != null)
                {
                    // Asignar propiedades específicas de HairballProjectile
                    hairballProjectile.canBounce = enableBouncing;
                    hairballProjectile.maxBounces = bounceCount;
                    hairballProjectile.bounceSpeedMultiplier = bounceSpeedMultiplier;
                    
                    // ✅ NUEVO: Configurar velocidad base para los cálculos de rebote
                    EnemyProjectile enemyProjectile = hairball.GetComponent<EnemyProjectile>();
                    if (enemyProjectile != null)
                    {
                        enemyProjectile.speed = selectedSpeed;
                    }
                }
                else
                {
                    // Fallback para EnemyProjectile básico
                    EnemyProjectile enemyProjectile = hairball.GetComponent<EnemyProjectile>();
                    if (enemyProjectile != null)
                    {
                        enemyProjectile.speed = selectedSpeed;
                        enemyProjectile.damage = 10f;
                    }
                }

                // Hairballs are static obstacles — freeze in place
                Rigidbody2D rb = hairball.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                }

            }
        }
    }

    [ContextMenu("TEST - Disparar Una Vez")]
    public void TestShootOnce()
    {
        ShootHairball();
    }

    [ContextMenu("TEST - Aumentar Dificultad")]
    public void TestIncreaseDifficulty()
    {
        IncreaseDifficulty(activeMustachePoints + 1);
    }
}