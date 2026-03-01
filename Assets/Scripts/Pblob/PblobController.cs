using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class PblobController : MonoBehaviour
{
    public enum PblobState { Idle, Phase1, Phase2Transition, Phase2, Phase3Transition, Phase3_Grid, Phase3_Combat, Defeated }

    [Header("Health & State")]
    public float maxHealth = Pizzard.Core.GameBalance.Bosses.Pblob.MaxHP;
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    private bool isVulnerable = false;
    public PblobState currentState = PblobState.Idle;
    private bool battleActive = false;

    [Header("Attack Patterns")]
    public PblobAttackPattern[] attackPatterns;

    [Header("Phase 2 Minigame")]
    public GameObject circlePrefab;
    private List<GameObject> activeCircles = new List<GameObject>();
    private float phase2MstTimer = 0f;
    private bool phase2TimerActive = false;

    [Header("Phase 3 Grid Puzzle")]
    public PblobGridPuzzle gridPuzzle;
    public Transform gridSpawnPoint;

    [Header("Arena Bounds")]
    public Vector3 arenaCenter;
    public float arenaClampX = 3.5f;
    public float arenaClampY = 2.0f;

    [Header("Events")]
    public UnityEvent OnBossBattleStart;
    public UnityEvent OnBossDefeated;
    public UnityEvent OnPhaseTransition;
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<bool> OnVulnerabilityChanged;

    [Header("DEBUG")]
    public bool debugMode = false;

    // References
    private PblobRhythmManager rhythmManager;
    private Coroutine stateCoroutine;
    private Transform playerTransform;

    private void Awake()
    {
        currentHealth = maxHealth;
        arenaCenter = transform.position; // Store original spawn as center
    }

    // Auto-detect arena bounds from the Tilemap if present
    private void AutoDetectArenaBounds()
    {
        Tilemap tilemap = FindObjectOfType<Tilemap>();
        if (tilemap == null) return;

        tilemap.CompressBounds();
        Bounds b = tilemap.localBounds;
        // Convert local bounds to world bounds accounting for scale
        Vector3 scale = tilemap.transform.lossyScale;
        float halfWidth  = b.extents.x * Mathf.Abs(scale.x);
        float halfHeight = b.extents.y * Mathf.Abs(scale.y);

        // Shrink by 1 unit to keep boss visually inside the walls — always positive
        arenaClampX = Mathf.Max(1f, halfWidth - 1f);
        arenaClampY = Mathf.Max(1f, halfHeight - 1f);

        // Use tilemap world center as arena center if it's near origin
        Vector3 worldCenter = tilemap.transform.TransformPoint(b.center);
        if (Vector3.Distance(worldCenter, arenaCenter) < 20f)
            arenaCenter = worldCenter;

        Debug.Log($"[Pblob] Arena bounds: ±{arenaClampX:F1}x ±{arenaClampY:F1}y  center={arenaCenter}");
    }

    private void Start()
    {
        debugMode = SaveSystem.GetDebugMode();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) 
        {
            var pmRoot = p.GetComponentInParent<Pizzard.Player.PlayerController>();
            playerTransform = pmRoot != null ? pmRoot.transform : p.transform.root;
        }

        if (rhythmManager == null)
            rhythmManager = FindObjectOfType<PblobRhythmManager>();

        if (gridPuzzle == null)
            gridPuzzle = FindObjectOfType<PblobGridPuzzle>();

        AutoDetectArenaBounds();
        ChangeState(PblobState.Idle);

        Debug.Log($"[Pblob] Initialized — HP: {currentHealth} | gridPuzzle: {(gridPuzzle != null ? "Found" : "MISSING!")}");
    }

    private void Update()
    {
        // Debug keys removed — use OnGUI buttons (KILL / NEXT PHASE) when debug mode is on
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
                stateCoroutine = StartCoroutine(Phase2Routine());
                break;
            case PblobState.Phase3Transition:
                MakeInvulnerable();
                OnPhaseTransition?.Invoke();
                stateCoroutine = StartCoroutine(Phase3TransitionRoutine());
                break;
            case PblobState.Phase3_Grid:
                MakeInvulnerable();
                if (gridPuzzle == null) gridPuzzle = FindObjectOfType<PblobGridPuzzle>();
                if (gridPuzzle != null)
                {
                    gridPuzzle.GenerateGrid(new Vector3(0f, 0.39f, 0f));
                }
                else
                {
                    Debug.LogError("[Pblob] Phase3_Grid: gridPuzzle is null! Add PblobGridPuzzle to scene.");
                }
                break;
            case PblobState.Phase3_Combat:
                stateCoroutine = StartCoroutine(Phase3CombatRoutine());
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
            
            // Pick random target near center but explicitly clamp it to a defined arena size 
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 targetPos = arenaCenter + (Vector3)(randomDir * Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius);

            targetPos.x = Mathf.Clamp(targetPos.x, arenaCenter.x - arenaClampX, arenaCenter.x + arenaClampX);
            targetPos.y = Mathf.Clamp(targetPos.y, arenaCenter.y - arenaClampY, arenaCenter.y + arenaClampY);
            
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
            transform.position = Vector3.Lerp(startPos, arenaCenter, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ChangeState(nextState);
    }

    private IEnumerator Phase3TransitionRoutine()
    {
        float elapsed = 0f;
        float duration = 1.5f;

        float gridOffset  = Pizzard.Core.GameBalance.Bosses.Pblob.GridSpawnOffsetY;  // e.g. -5
        Vector3 bossStart   = transform.position;
        // User requested boss to be exactly at Y=5 during phase 3 transition
        Vector3 bossTarget  = new Vector3(arenaCenter.x, 5f, 0f);

        Vector3 playerStart  = playerTransform != null ? playerTransform.position : Vector3.zero;
        
        // The grid has an array-sizing problem earlier which shifted coordinates. 
        // Now that it's 10x5 and 2.0 size:
        Vector3 gridCenter = gridSpawnPoint != null ? gridSpawnPoint.transform.position : arenaCenter + new Vector3(0, gridOffset, 0);
        float compactGridHeight = 5f;
        float compactTileSize = 2.0f;
        // Half the height (2.5 tiles) times the size (2.0 bounds) is 5.0 units down from center.
        float botYOffset = -(compactGridHeight - 1f) * compactTileSize / 2f; 
        Vector3 playerTarget = gridCenter + new Vector3(0, botYOffset, 0);

        // Disable player movement + freeze physics during cinematic
        Pizzard.Player.PlayerController pm = playerTransform != null ? playerTransform.GetComponent<Pizzard.Player.PlayerController>() : null;
        Rigidbody2D playerRb = playerTransform != null ? playerTransform.GetComponent<Rigidbody2D>() : null;
        if (pm != null) pm.enabled = false;
        if (playerRb != null) { playerRb.velocity = Vector2.zero; playerRb.isKinematic = true; }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(bossStart, bossTarget, t);
            if (playerTransform != null)
                playerTransform.position = Vector3.Lerp(playerStart, playerTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact target
        if (playerTransform != null) playerTransform.position = playerTarget;

        // Re-enable player after brief grid-reveal delay
        if (playerRb != null) playerRb.isKinematic = false;
        if (pm != null)
            StartCoroutine(ReenablePlayerAfter(pm, 2f));

        ChangeState(PblobState.Phase3_Grid);
    }

    private IEnumerator ReenablePlayerAfter(Pizzard.Player.PlayerController pm, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pm != null) pm.enabled = true;
    }

    private IEnumerator Phase3CombatRoutine()
    {
        // Vulnerability is contact-based (OnTriggerEnter2D/Exit2D), not automatic

        // Use phase-3-specific pattern if available, else pure chase
        if (attackPatterns != null && attackPatterns.Length > 1 && attackPatterns[1] != null)
            attackPatterns[1].StartPattern();

        // Ensure boss cannot be pushed by player collision
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        while (currentState == PblobState.Phase3_Combat)
        {
            // Boss remains static in the center while vulnerable
            yield return null;
        }

        if (attackPatterns != null && attackPatterns.Length > 1 && attackPatterns[1] != null)
            attackPatterns[1].StopPattern();
    }

    // --- PHASE 2 LOGIC (Circle Minigame) ---
    private IEnumerator Phase2Routine()
    {
        Debug.Log("[Pblob] Starting Phase 2 Circle Minigame!");
        
        // 1. Spawn Circles
        SpawnPhase2Circles();

        // 2. Random Movement for 5 seconds
        float moveTime = Pizzard.Core.GameBalance.Bosses.Pblob.Phase2CircleMoveTime;
        foreach (var c in activeCircles)
        {
            var controller = c.GetComponent<PblobCircleController>();
            if (controller != null)
                StartCoroutine(controller.MoveRandomly(moveTime, 6f));
        }

        yield return new WaitForSeconds(moveTime);

        // 3. Circles Stop. Start 30s vulnerability timer UI (console for now if no UI provided).
        float timerDuration = Pizzard.Core.GameBalance.Bosses.Pblob.Phase2Timer;
        phase2MstTimer = timerDuration;
        phase2TimerActive = true;
        
        Debug.Log($"[Pblob] Circles Stopped. You have {timerDuration}s to find the Green circle!");

        // The vulnerability is now implicitly handled by PblobCircleController OnTriggerEnter2D 
        // which calls MakeVulnerable() / MakeInvulnerable().

        while (currentState == PblobState.Phase2)
        {
            if (phase2TimerActive)
            {
                phase2MstTimer -= Time.deltaTime;
                
                // Show floating text here in a real game. For now we use GUI later.

                if (phase2MstTimer <= 0)
                {
                    phase2TimerActive = false;
                    Debug.Log("⏳ Phase 2 Time Out! Checking penalty...");

                    if (!isVulnerable) // player was not standing in green circle
                    {
                        int penaltyDamage = Pizzard.Core.GameBalance.Bosses.Pblob.Phase2TimeoutDamage;
                        Debug.Log($"❌ Player failed to stand in the Green circle! Taking {penaltyDamage} damage.");
                        if (playerTransform == null)
                        {
                            var p = GameObject.FindGameObjectWithTag("Player");
                            if (p != null) playerTransform = p.transform;
                        }
                        if (playerTransform != null)
                        {
                            var playerHP = playerTransform.GetComponentInChildren<PlayerHPController>();
                            if (playerHP == null) playerHP = FindObjectOfType<PlayerHPController>();

                            if (playerHP != null) 
                            {
                                playerHP.ForceDamage(penaltyDamage);
                            }
                        }
                    }

                    Debug.Log("⏳ Restarting minigame...");
                    CleanupPhase2Circles();
                    MakeInvulnerable(); // A quick pause
                    yield return new WaitForSeconds(1f);
                    
                    // Restart phase 2 mechanics immediately without leaving the main Coroutine loop
                    SpawnPhase2Circles();
                    foreach (var c in activeCircles)
                    {
                        var controller = c.GetComponent<PblobCircleController>();
                        if (controller != null)
                            StartCoroutine(controller.MoveRandomly(Pizzard.Core.GameBalance.Bosses.Pblob.Phase2CircleMoveTime, 6f));
                    }
                    yield return new WaitForSeconds(Pizzard.Core.GameBalance.Bosses.Pblob.Phase2CircleMoveTime);
                    
                    phase2MstTimer = timerDuration;
                    phase2TimerActive = true;
                    Debug.Log($"[Pblob] Circles Stopped. You have {timerDuration}s to find the Green circle!");
                    
                    // Loop naturally continues
                    continue;
                }
            }
            yield return null;
        }
    }
    
    // UI Helpers
    public bool IsPhase2TimerActive() => phase2TimerActive;
    public float GetPhase2TimeRemaining() => phase2MstTimer;

    private void SpawnPhase2Circles()
    {
        CleanupPhase2Circles();

        if (circlePrefab == null)
        {
            Debug.LogError("Circle Prefab missing! Please assign it to PblobController.");
            return;
        }

        // We need 1 Green, 2 Red
        List<PblobCircleController.CircleType> types = new List<PblobCircleController.CircleType>
        {
            PblobCircleController.CircleType.Green,
            PblobCircleController.CircleType.Red,
            PblobCircleController.CircleType.Red
        };

        // Shuffle types
        for (int i = 0; i < types.Count; i++)
        {
            var temp = types[i];
            int rnd = Random.Range(i, types.Count);
            types[i] = types[rnd];
            types[rnd] = temp;
        }

        // Ignore spells/enemies hitting circles — player (Default layer 0) must NOT be ignored
        int bossCircleLayer = LayerMask.NameToLayer("BossCircle");
        if (bossCircleLayer >= 0)
        {
            foreach (string layerName in new[] { "PlayerProjectiles", "EnemyProjectiles", "Wall" })
            {
                int l = LayerMask.NameToLayer(layerName);
                if (l >= 0) Physics2D.IgnoreLayerCollision(bossCircleLayer, l, true);
            }
        }

        float spawnRadius = Pizzard.Core.GameBalance.Bosses.Pblob.CircleSpawnRadius;
        float circleScale = Pizzard.Core.GameBalance.Bosses.Pblob.CircleScale;

        // Floor platform positions: spread across bottom half, below boss
        float floorY = Pizzard.Core.GameBalance.Bosses.Pblob.Phase2FloorY;
        float[] xOffsets = new float[] { -spawnRadius, 0f, spawnRadius };

        for (int i = 0; i < 3; i++)
        {
            float xJitter = Random.Range(-0.5f, 0.5f);
            Vector3 offset = new Vector3(xOffsets[i] + xJitter, floorY, 0f);
            GameObject newCircle = Instantiate(circlePrefab, arenaCenter + offset, Quaternion.identity);

            newCircle.transform.localScale = circlePrefab.transform.localScale * circleScale;
            if (bossCircleLayer >= 0) newCircle.layer = bossCircleLayer;

            var controller = newCircle.GetComponent<PblobCircleController>();
            if (controller != null)
            {
                controller.type = types[i];
                controller.StartMoving(
                    Pizzard.Core.GameBalance.Bosses.Pblob.Phase2CircleMoveTime,
                    Pizzard.Core.GameBalance.Bosses.Pblob.CircleMoveSpeed);
            }
            activeCircles.Add(newCircle);
        }
    }

    private void CleanupPhase2Circles()
    {
        foreach (var c in activeCircles)
        {
            if (c != null) Destroy(c);
        }
        activeCircles.Clear();
        phase2TimerActive = false;
        MakeInvulnerable(); // Ensure boss is invuln once circles depart
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
        // Debug method: bypasses vulnerability check
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

    // Called via a BossArea trigger volume usually, or a manual debug button
    public void FinishGridPuzzle()
    {
        if (currentState == PblobState.Phase3_Grid)
        {
            if (gridPuzzle != null) gridPuzzle.DestroyGrid();
            ChangeState(PblobState.Phase3_Combat);
            Debug.Log("🏁 Player finished the Grid! Final enrage combat starts.");
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
            CleanupPhase2Circles();
            ChangeState(PblobState.Phase3Transition);
        }
    }

    // --- UTILS ---

    private void ForceNextPhase()
    {
        if (currentState == PblobState.Idle || currentState == PblobState.Phase1)
            ForceTakeDamage(maxHealth * 0.35f);
        else if (currentState == PblobState.Phase2)
            ForceTakeDamage(maxHealth * 0.35f);
    }

    public bool IsVulnerable() { return isVulnerable; }
    public Vector3 ArenaCenter => arenaCenter;
    public float ArenaClampX => Mathf.Abs(arenaClampX);
    public float ArenaClampY => Mathf.Abs(arenaClampY);

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
        if (gridPuzzle != null) gridPuzzle.DestroyGrid();
        if (rhythmManager != null) rhythmManager.StopRhythm();
        
        OnBossDefeated?.Invoke();
        
        if (Pizzard.Core.GameFlowManager.Instance != null)
        {
            Pizzard.Core.GameFlowManager.Instance.AvanzarFase();
        }
    }

    // --- PHASE 3 CONTACT VULNERABILITY ---
    // --- PHASE 3 CONTACT VULNERABILITY ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other.gameObject);
    }

    private void HandlePlayerContact(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            if (currentState == PblobState.Phase3_Grid)
            {
                FinishGridPuzzle();
            }
            if (currentState == PblobState.Phase3_Combat)
            {
                MakeVulnerable();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        HandlePlayerExit(collision.gameObject);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        HandlePlayerExit(other.gameObject);
    }

    private void HandlePlayerExit(GameObject obj)
    {
        if (currentState == PblobState.Phase3_Combat && obj.CompareTag("Player"))
            MakeInvulnerable();
    }

    // --- GUI (debug overlay) ---
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

            if (currentState == PblobState.Phase2)
            {
                GUI.Label(new Rect(20, 95, 230, 20), $"<color=yellow>Phase 2 Timer: {phase2MstTimer:F1}s</color>");
            }

            if (GUI.Button(new Rect(20, 115, 100, 25), "KILL"))
            {
                ForceTakeDamage(currentHealth);
            }
            if (GUI.Button(new Rect(130, 115, 100, 25), "NEXT PHASE"))
            {
                ForceNextPhase();
            }
        }
    }
}