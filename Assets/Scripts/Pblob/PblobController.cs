using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PblobController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 1000f;
    private float currentHealth;
    private bool isVulnerable = true;

    [Header("Phase Management")]
    public int currentPhase = 1;
    public int maxPhases = 3;
    private bool battleActive = false;
    private float[] phaseHealthThresholds;
    private float damageInCurrentPhase;

    [Header("Attack Patterns")]
    public PblobAttackPattern[] attackPatterns;
    private int currentPatternIndex = 0;

    [Header("Timing Settings")]
    public float vulnerableWindowDuration = 3f;
    public float patternDuration = 10.5f;

    [Header("Phase 2 Transition")]
    public GameObject phase2Door; // ✅ NUEVO: Referencia a la puerta de fase 2
    public bool phase2Unlocked = false; // ✅ NUEVO: Control de desbloqueo

    [Header("Events")]
    public UnityEvent OnBossBattleStart;
    public UnityEvent OnBossDefeated;
    public UnityEvent OnPhaseCompleted;
    public UnityEvent OnPhaseTransition;
    public UnityEvent OnPhase2Unlocked; // ✅ NUEVO: Evento cuando se desbloquea fase 2
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<bool> OnVulnerabilityChanged;
    public UnityEvent<int> OnPhaseChanged;

    [Header("DEBUG")]
    public bool debugMode = true;
    public KeyCode damageKey = KeyCode.T;
    public KeyCode toggleVulnerableKey = KeyCode.V;
    public KeyCode nextPhaseKey = KeyCode.P;
    public KeyCode unlockPhase2Key = KeyCode.Alpha2; // ✅ NUEVO: Debug para fase 2

    // Referencias
    private PblobRhythmManager rhythmManager;
    private Coroutine battleCoroutine;

    private void Awake()
    {
        // Inicializar valores en Awake para que estén listos antes de que otros scripts los necesiten
        currentHealth = maxHealth;
        InitializePhaseThresholds();
    }

    private void Start()
    {
        // Buscar RhythmManager solo si no está asignado manualmente
        if (rhythmManager == null)
        {
            rhythmManager = FindObjectOfType<PblobRhythmManager>();
        }
        
        if (debugMode)
        {
            Debug.Log("✔ Boss inicializado - Vida: " + currentHealth);
            Debug.Log("📞 Presiona T para aplicar daño (100)");
            Debug.Log("📞 Presiona V para toggle vulnerabilidad");
            Debug.Log("📞 Presiona P para avanzar fase (DEBUG)");
            Debug.Log("📞 Presiona 2 para desbloquear Fase 2 (DEBUG)");
            Debug.Log("⚠️ La batalla NO inicia automáticamente. Usa GameFlowManager o llama StartBossBattle()");
        }
    }

    private void InitializePhaseThresholds()
    {
        phaseHealthThresholds = new float[maxPhases + 1];
        for (int i = 0; i <= maxPhases; i++)
        {
            phaseHealthThresholds[i] = maxHealth * (1f - (i * 0.1f));
        }
        damageInCurrentPhase = 0f;
    }

    private void Update()
    {
        if (!debugMode) return;

        if (Input.GetKeyDown(damageKey))
        {
            TakeDamage(100f);
        }

        if (Input.GetKeyDown(toggleVulnerableKey))
        {
            ToggleVulnerability();
        }

        if (Input.GetKeyDown(nextPhaseKey))
        {
            DebugNextPhase();
        }

        // ✅ NUEVO: Debug para desbloquear fase 2
        if (Input.GetKeyDown(unlockPhase2Key))
        {
            UnlockPhase2();
        }
    }

    public void StartBossBattle()
    {
        if (battleActive)
        {
            Debug.Log("⚠️ La batalla ya está en curso");
            return;
        }

        Debug.Log("🎵 Iniciando batalla contra el boss...");
        battleActive = true;
        currentPhase = 1;
        damageInCurrentPhase = 0f;

        // Reproducir música del boss
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossMusic();
        }
        else
        {
            Debug.LogWarning("⚠️ SoundManager no encontrado - continuando sin música");
        }

        // Iniciar sistema de ritmo si existe
        if (rhythmManager != null)
        {
            rhythmManager.StartRhythm();
        }
        else
        {
            Debug.LogWarning("⚠️ RhythmManager no encontrado - continuando sin ritmo");
        }

        battleCoroutine = StartCoroutine(BattleSequence());

        OnBossBattleStart?.Invoke();
        OnPhaseChanged?.Invoke(currentPhase);
        Debug.Log("⚔️ Batalla del boss iniciada completamente - Fase " + currentPhase);
    }

    private IEnumerator BattleSequence()
    {
        while (battleActive && currentPhase <= maxPhases)
        {
            Debug.Log($"🔄 Iniciando Fase {currentPhase}");
            Debug.Log($"🎯 Objetivo de fase: reducir vida a {phaseHealthThresholds[currentPhase]}");
            
            // Fase de ataque (boss invulnerable)
            MakeInvulnerable();
            StartCurrentPattern();

            yield return new WaitForSeconds(patternDuration);

            // Detener patrón actual
            StopCurrentPattern();

            // Ventana vulnerable (3 beats)
            MakeVulnerable();
            Debug.Log("💡 VENTANA VULNERABLE - Ataca ahora!");
            Debug.Log($"💥 Daño permitido en esta fase: {maxHealth * 0.1f - damageInCurrentPhase}");

            yield return new WaitForSeconds(vulnerableWindowDuration);

            CheckPhaseProgress();

            if (currentHealth > phaseHealthThresholds[currentPhase] && currentPhase <= maxPhases)
            {
                Debug.Log($"🔄 Repitiendo Fase {currentPhase} - Vida actual: {currentHealth}, Objetivo: {phaseHealthThresholds[currentPhase]}");
            }
        }

        // ✅ ACTUALIZADO: Al terminar fase 1, desbloquear fase 2
        if (currentPhase > maxPhases)
        {
            UnlockPhase2();
        }
    }

    // ✅ NUEVO: Desbloquear fase 2
    private void UnlockPhase2()
    {
        if (phase2Unlocked) return;

        Debug.Log("🔓 FASE 2 DESBLOQUEADA! - Se ha abierto la puerta");
        phase2Unlocked = true;
        
        // Activar puerta de fase 2
        if (phase2Door != null)
        {
            phase2Door.SetActive(true);
            Debug.Log("🚪 Puerta de Fase 2 activada");
        }

        // Evento de desbloqueo
        OnPhase2Unlocked?.Invoke();

        // Detener batalla actual
        battleActive = false;
        StopAllPatterns();
        
        if (rhythmManager != null)
        {
            rhythmManager.StopRhythm();
        }

        Debug.Log("🎯 Ve a la puerta para iniciar la Fase 2");
    }

    // ✅ NUEVO: Iniciar fase 2 (llamado desde la puerta)
    public void StartPhase2()
    {
        if (!phase2Unlocked)
        {
            Debug.Log("❌ Fase 2 no desbloqueada todavía");
            return;
        }

        Debug.Log("🎵 INICIANDO FASE 2 - Patrón de círculos de movimiento");
        battleActive = true;
        currentPhase = 4; // Fase 2 empieza en 4

        // Iniciar patrón 2
        if (attackPatterns.Length > 1 && attackPatterns[1] != null)
        {
            attackPatterns[1].StartPattern();
            Debug.Log("🔄 Patrón 2 iniciado");
        }

        // Reanudar ritmo
        if (rhythmManager != null)
        {
            rhythmManager.StartRhythm();
        }

        OnPhaseChanged?.Invoke(currentPhase);
    }

    private void CheckPhaseProgress()
    {
        float currentPhaseTarget = phaseHealthThresholds[currentPhase];
        
        if (currentHealth <= currentPhaseTarget)
        {
            CompleteCurrentPhase();
        }
        else
        {
            Debug.Log($"📊 Progreso Fase {currentPhase}: {maxHealth - currentHealth} / {maxHealth * 0.1f * currentPhase} daño total");
        }
    }

    private void CompleteCurrentPhase()
    {
        Debug.Log($"✅ Fase {currentPhase} completada! Vida: {currentHealth}/{maxHealth}");
        OnPhaseCompleted?.Invoke();

        currentHealth = phaseHealthThresholds[currentPhase];
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentPhase < maxPhases)
        {
            currentPhase++;
            damageInCurrentPhase = 0f;
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"🔼 Avanzando a Fase {currentPhase} - Objetivo: {phaseHealthThresholds[currentPhase]}");
            
            IncreasePatternDifficulty();
        }
        else
        {
            Debug.Log("🎉 Todas las fases de la Fase 1 completadas!");
        }
    }

    private void IncreasePatternDifficulty()
    {
        if (attackPatterns.Length > 0 && attackPatterns[0] is PblobAttackPattern1 pattern1)
        {
            int targetPoints = 3 + currentPhase;
            pattern1.IncreaseDifficulty(targetPoints);
            Debug.Log($"📈 Dificultad aumentada para Fase {currentPhase} - Puntos: {targetPoints}");
        }
    }

    private void StartCurrentPattern()
    {
        if (attackPatterns != null && attackPatterns.Length > 0)
        {
            attackPatterns[0].StartPattern();
        }
    }

    private void StopCurrentPattern()
    {
        if (attackPatterns != null && attackPatterns.Length > 0)
        {
            attackPatterns[0].StopPattern();
        }
    }

    private void StopAllPatterns()
    {
        if (attackPatterns != null)
        {
            foreach (var pattern in attackPatterns)
            {
                if (pattern != null)
                    pattern.StopPattern();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isVulnerable)
        {
            Debug.Log("❌ Boss no vulnerable - daño ignorado");
            return;
        }

        float maxDamageInPhase = maxHealth * 0.1f;
        float remainingDamageAllowed = maxDamageInPhase - damageInCurrentPhase;
        
        if (remainingDamageAllowed <= 0)
        {
            Debug.Log("⏹️ Límite de daño de fase alcanzado - espera siguiente fase");
            return;
        }

        damage = Mathf.Min(damage, remainingDamageAllowed);
        damage = Mathf.Max(0, damage);

        float previousHealth = currentHealth;
        currentHealth -= damage;
        damageInCurrentPhase += damage;

        Debug.Log($"💥 Daño: {damage} | Vida: {previousHealth} -> {currentHealth}");
        Debug.Log($"📊 Daño en Fase {currentPhase}: {damageInCurrentPhase}/{maxDamageInPhase}");

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= phaseHealthThresholds[currentPhase])
        {
            Debug.Log($"🎯 Objetivo de Fase {currentPhase} alcanzado!");
        }

        if (currentHealth <= 0)
        {
            Defeat();
        }
    }

    public bool IsVulnerable()
    {
        return isVulnerable;
    }

    public void MakeVulnerable()
    {
        isVulnerable = true;
        OnVulnerabilityChanged?.Invoke(true);
        Debug.Log("💡 Boss ahora es VULNERABLE");
    }

    public void MakeInvulnerable()
    {
        isVulnerable = false;
        OnVulnerabilityChanged?.Invoke(false);
        Debug.Log("🛡️ Boss ahora es INVULNERABLE");
    }

    private void ToggleVulnerability()
    {
        isVulnerable = !isVulnerable;
        OnVulnerabilityChanged?.Invoke(isVulnerable);
        Debug.Log($"🔀 Vulnerabilidad: {(isVulnerable ? "ACTIVADA" : "DESACTIVADA")}");
    }

    private void Defeat()
    {
        Debug.Log("🎊 BOSS DERROTADO!");
        battleActive = false;
        StopAllPatterns();
        OnBossDefeated?.Invoke();
    }

    // DEBUG - Métodos para testing
    [ContextMenu("DEBUG - Aplicar 100 de daño")]
    public void DebugTakeDamage()
    {
        TakeDamage(100f);
    }

    [ContextMenu("DEBUG - Iniciar Batalla Completa")]
    public void DebugStartBattle()
    {
        StartBossBattle();
    }

    [ContextMenu("DEBUG - Detener Batalla")]
    public void DebugStopBattle()
    {
        battleActive = false;
        StopAllPatterns();
        if (rhythmManager != null) rhythmManager.StopRhythm();
    }

    [ContextMenu("DEBUG - Avanzar Fase")]
    public void DebugNextPhase()
    {
        CompleteCurrentPhase();
    }

    [ContextMenu("DEBUG - Desbloquear Fase 2")]
    public void DebugUnlockPhase2()
    {
        UnlockPhase2();
    }

    [ContextMenu("DEBUG - Iniciar Fase 2")]
    public void DebugStartPhase2()
    {
        StartPhase2();
    }

    [ContextMenu("DEBUG - Reiniciar Boss")]
    public void DebugResetBoss()
    {
        currentHealth = maxHealth;
        currentPhase = 1;
        damageInCurrentPhase = 0f;
        isVulnerable = true;
        battleActive = false;
        phase2Unlocked = false;
        InitializePhaseThresholds();
        OnHealthChanged?.Invoke(1f);
        OnPhaseChanged?.Invoke(1);
        Debug.Log("🔄 Boss reiniciado");
    }

    // Para debug visual en pantalla
    void OnGUI()
    {
        if (debugMode)
        {
            float phaseProgress = currentPhase <= maxPhases ? 
                (maxHealth * 0.1f * currentPhase - (maxHealth - currentHealth)) / (maxHealth * 0.1f) : 1f;
            
            GUI.Box(new Rect(10, 10, 320, 200), "DEBUG BOSS");
            GUI.Label(new Rect(20, 40, 300, 20), $"Vida: {currentHealth}/{maxHealth}");
            GUI.Label(new Rect(20, 60, 300, 20), $"Fase: {currentPhase}/4");
            GUI.Label(new Rect(20, 80, 300, 20), $"Fase 2: {(phase2Unlocked ? "DESBLOQUEADA" : "BLOQUEADA")}");
            GUI.Label(new Rect(20, 100, 300, 20), $"Objetivo Fase: {phaseHealthThresholds[currentPhase]}");
            GUI.Label(new Rect(20, 120, 300, 20), $"Daño en Fase: {damageInCurrentPhase}/{maxHealth * 0.1f}");
            GUI.Label(new Rect(20, 140, 300, 20), $"Progreso: {phaseProgress * 100:F1}%");
            GUI.Label(new Rect(20, 160, 300, 20), $"Estado: {(isVulnerable ? "VULNERABLE" : "INVULNERABLE")}");
            GUI.Label(new Rect(20, 180, 300, 20), "T=Daño(100) | V=Vulnerabilidad | 2=Fase2");
        }
    }
}