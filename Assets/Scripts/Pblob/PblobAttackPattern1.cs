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

    public override void StartPattern()
    {
        if (isPatternActive) return;

        rhythmManager = FindObjectOfType<PblobRhythmManager>();
        if (rhythmManager == null)
        {
            DebugPattern("No se encontró RhythmManager - usando timing por defecto");
            StartCoroutine(AttackRoutineFallback());
            return;
        }

        isPatternActive = true;

        if (useRhythm)
        {
            rhythmManager.OnBeat += OnMusicBeat;
            DebugPattern($"Iniciado - Ritmo: cada {beatsBetweenShots} beats - Puntos activos: {activeMustachePoints}");
        }
        else
        {
            attackCoroutine = StartCoroutine(AttackRoutineFallback());
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
                GameObject hairball = Instantiate(hairballPrefab, point.position, point.rotation);

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

                Vector3 direction = (player.transform.position - point.position).normalized;
                Rigidbody2D rb = hairball.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = direction * selectedSpeed;
                }

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                hairball.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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