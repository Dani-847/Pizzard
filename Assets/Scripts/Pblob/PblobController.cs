using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PblobController : MonoBehaviour
{
    public enum PblobState { Idle, Phase1, Phase2Transition, Phase2, Phase3Transition, Phase3_Grid, Phase3_Combat, Defeated }

    [Header("Health & State")]
    public float maxHealth = Pizzard.Core.GameBalance.Bosses.Pblob.MaxHP;
    private float currentHealth;
    private bool isVulnerable = false;
    public PblobState currentState = PblobState.Idle;
    private bool battleActive = false;

    [Header("Attack Patterns")]
    public PblobAttackPattern[] attackPatterns;

    [Header("Events")]
    public UnityEvent OnBossBattleStart;
    public UnityEvent OnBossDefeated;
    public UnityEvent OnPhaseTransition;
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<bool> OnVulnerabilityChanged;

    [Header("DEBUG")]
    public bool debugMode = true;
    public KeyCode damageKey = KeyCode.T;
    public KeyCode nextPhaseKey = KeyCode.P;

    // References
    private PblobRhythmManager rhythmManager;
    private Coroutine stateCoroutine;
    private Transform playerTransform;
    private Vector3 centerPoint;

    private void Awake()
    {
        currentHealth = maxHealth;
        centerPoint = transform.position; // Store original spawn as center
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (rhythmManager == null)
        {
            rhythmManager = FindObjectOfType<PblobRhythmManager>();
        }
        
        ChangeState(PblobState.Idle);
        
        if (debugMode)
        {
            Debug.Log($"✔ P'blob Initialized - HP: {currentHealth}");
            Debug.Log("⚠️ Boss is IDLE. Hit him once to start the fight.");
        }
    }

    private void Update()
    {
        if (!debugMode) return;

        if (Input.GetKeyDown(damageKey))
        {
            // Note: debugging damage bypasses vulnerability checks
            ForceTakeDamage(100f);
        }

        if (Input.GetKeyDown(nextPhaseKey))
        {
            ForceNextPhase();
        }
    }

    public void ChangeState(PblobState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"[Pblob] State Change: {currentState} -> {newState}");
        currentState = newState;
        
        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
            stateCoroutine = null;
        }

        StopAllPatterns();

        switch (newState)
        {
            case PblobState.Idle:
                MakeInvulnerable();
                break;
            case PblobState.Phase1:
                MakeInvulnerable(); // Default to invuln, state coroutine toggles it
                stateCoroutine = StartCoroutine(Phase1Routine());
                break;
            case PblobState.Phase2Transition:
                MakeInvulnerable();
                OnPhaseTransition?.Invoke();
                stateCoroutine = StartCoroutine(MoveToCenterAndTransition(PblobState.Phase2));
                break;
            case PblobState.Phase2:
                MakeInvulnerable();
                stateCoroutine = StartCoroutine(Phase2PlaceholderRoutine());
                break;
            case PblobState.Phase3Transition:
                MakeInvulnerable();
                OnPhaseTransition?.Invoke();
                // TODO: Teleport logic in Phase 3
                ChangeState(PblobState.Phase3_Combat);
                break;
            case PblobState.Phase3_Combat:
                MakeVulnerable(); // Permanently vulnerable in final enrage
                if (attackPatterns.Length > 0 && attackPatterns[0] != null)
                {
                    // Full rage mode
                    attackPatterns[0].StartPattern();
                }
                break;
            case PblobState.Defeated:
                MakeInvulnerable();
                Defeat();
                break;
        }
    }

    public void StartBossBattle()
    {
        if (battleActive) return;

        Debug.Log("🎵 Starting Boss Battle mechanics...");
        battleActive = true;
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBossMusic();

        if (rhythmManager != null)
            rhythmManager.StartRhythm();

        OnBossBattleStart?.Invoke();
        ChangeState(PblobState.Phase1);
    }

    // --- PHASE 1 LOGIC ---
    // Alternates every Phase1AlternateTime between Moving (Vulnerable) and Shooting (Invulnerable)
    private IEnumerator Phase1Routine()
    {
        float alternateTime = Pizzard.Core.GameBalance.Bosses.Pblob.Phase1AlternateTime;
        float moveSpeed = Pizzard.Core.GameBalance.Bosses.Pblob.Phase1MoveSpeed;

        while (currentState == PblobState.Phase1)
        {
            // 1. Moving state (Vulnerable)
            MakeVulnerable();
            
            // Pick random target near center (to avoid wandering infinitely off-screen)
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 targetPos = centerPoint + (Vector3)(randomDir * Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius);
            
            float elapsed = 0f;
            while (elapsed < alternateTime && currentState == PblobState.Phase1)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (currentState != PblobState.Phase1) break;

            // 2. Standing Still state (Invulnerable + Attacking)
            MakeInvulnerable();
            if (attackPatterns.Length > 0 && attackPatterns[0] != null)
            {
                attackPatterns[0].StartPattern();
            }

            yield return new WaitForSeconds(alternateTime);

            if (attackPatterns.Length > 0 && attackPatterns[0] != null)
            {
                attackPatterns[0].StopPattern();
            }
        }
    }

    private IEnumerator MoveToCenterAndTransition(PblobState nextState)
    {
        float elapsed = 0f;
        float duration = 1.5f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, centerPoint, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ChangeState(nextState);
    }

    private IEnumerator Phase2PlaceholderRoutine()
    {
        Debug.Log("[Pblob] Phase 2 - Placeholders standing by");
        // We will inject the circle logic in Plan 18.2 here.
        yield return null;
    }

    // --- HEALTH & DAMAGE LOGIC ---
    public void TakeDamage(float damage)
    {
        if (currentState == PblobState.Idle)
        {
            Debug.Log("✂️ Boss hit for the first time! Starting battle!");
            StartBossBattle();
            // Automatically take this first hit
            isVulnerable = true; 
        }

        if (!isVulnerable)
        {
            Debug.Log("❌ Boss is INVULNERABLE - Damage ignored.");
            return;
        }

        ApplyDamageWithThresholds(damage);
    }

    private void ForceTakeDamage(float damage)
    {
        // Debug method to force damage bypassing vulnerability
        ApplyDamageWithThresholds(damage);
    }

    private void ApplyDamageWithThresholds(float damage)
    {
        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"💥 Boss hit! Damage: {damage} | HP: {previousHealth} -> {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        CheckPhaseThresholds();

        if (currentHealth <= 0 && currentState != PblobState.Defeated)
        {
            ChangeState(PblobState.Defeated);
        }
    }

    private void CheckPhaseThresholds()
    {
        float hpPercent = currentHealth / maxHealth;

        if (hpPercent <= 0.66f && currentState == PblobState.Phase1)
        {
            Debug.Log("🎯 66% HP Reached. Triggering Phase 2!");
            ChangeState(PblobState.Phase2Transition);
        }
        else if (hpPercent <= 0.33f && currentState == PblobState.Phase2)
        {
            Debug.Log("🎯 33% HP Reached. Triggering Phase 3!");
            ChangeState(PblobState.Phase3Transition);
        }
    }

    private void ForceNextPhase()
    {
        if (currentState == PblobState.Idle || currentState == PblobState.Phase1)
        {
            ForceTakeDamage((maxHealth * 0.35f)); // Drop below 66%
        }
        else if (currentState == PblobState.Phase2)
        {
            ForceTakeDamage((maxHealth * 0.35f)); // Drop below 33%
        }
    }

    // --- UTILS ---
    public bool IsVulnerable() { return isVulnerable; }

    public void MakeVulnerable()
    {
        if (isVulnerable) return;
        isVulnerable = true;
        OnVulnerabilityChanged?.Invoke(true);
        // Visual cue (placeholder, maybe color flash)
    }

    public void MakeInvulnerable()
    {
        if (!isVulnerable) return;
        isVulnerable = false;
        OnVulnerabilityChanged?.Invoke(false);
    }

    private void StopAllPatterns()
    {
        if (attackPatterns != null)
        {
            foreach (var pattern in attackPatterns)
            {
                if (pattern != null) pattern.StopPattern();
            }
        }
    }

    private void Defeat()
    {
        Debug.Log("🎊 BOSS DEFEATED!");
        battleActive = false;
        StopAllPatterns();
        if (rhythmManager != null) rhythmManager.StopRhythm();
        
        OnBossDefeated?.Invoke();
        
        if (Pizzard.Core.GameFlowManager.Instance != null)
        {
            Pizzard.Core.GameFlowManager.Instance.AvanzarFase();
        }
    }

    // --- GUI ---
    void OnGUI()
    {
        if (debugMode)
        {
            GUI.Box(new Rect(10, 10, 250, 140), "BOSS 1 STATE");
            
            float hp = (currentHealth / maxHealth) * 100f;
            GUI.Label(new Rect(20, 35, 230, 20), $"HP: {hp:F0}% ({currentHealth:F0}/{maxHealth})");
            
            string vulnText = isVulnerable ? "<color=green>VULNERABLE</color>" : "<color=red>INVULNERABLE</color>";
            GUI.Label(new Rect(20, 55, 230, 20), $"Status: {vulnText}");
            
            GUI.Label(new Rect(20, 75, 230, 20), $"State: {currentState}");
            
            if (GUI.Button(new Rect(20, 100, 100, 25), "KILL"))
            {
                ForceTakeDamage(currentHealth);
            }
            if (GUI.Button(new Rect(130, 100, 100, 25), "NEXT PHASE"))
            {
                ForceNextPhase();
            }
        }
    }
}