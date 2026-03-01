using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pizzard.Bosses
{
    using Core;

    /// <summary>
    /// Boss 4: Niggel Worthington (The Rich Guy).
    /// Uses a CoinVault HP system with one-way enrage thresholds and a player momentum system.
    /// Plans 02 and 03 add attack implementations on top of this foundation.
    /// </summary>
    public class NiggelController : BossBase
    {
        // ── CoinVault (HP) ──────────────────────────────
        private int coinVault;
        private int enrageLevel = 0;

        // ── Movement ────────────────────────────────────
        [SerializeField] private float currentMoveSpeed;
        private bool isDashing = false;
        private Coroutine dashCoroutine;
        private float dashCooldownTimer = 0f;

        // ── Arena bounds (mirrors PblobController pattern) ──
        [SerializeField] private Vector2 arenaCenter = Vector2.zero;
        [SerializeField] private float arenaClampX = 7f;
        [SerializeField] private float arenaClampY = 4f;

        // ── Momentum (player side — static so other scripts can read) ──
        public static float PlayerDamageMultiplier = 1f;
        public static float PlayerSpeedMultiplier = 1f;
        private int playerMomentum = 0;
        private Coroutine momentumResetCoroutine;

        // ── Prefabs ──────────────────────────────────────
        [SerializeField] private GameObject coinBagPrefab;
        [SerializeField] private GameObject healingCoinPrefab;
        [SerializeField] private GameObject blackDotBarrierPrefab;

        // ── Barrier tracking ──────────────────────────────
        private List<GameObject> activeBarriers = new List<GameObject>();

        // ── Coin shield state ────────────────────────────
        private bool coinShieldActive = false;
        private Coroutine shieldRoutine;

        // ── Attack routines ──────────────────────────────
        private Coroutine attackRoutine;
        private bool isActive = false;
        private bool hasStarted = false;  // idle until player gives any input

        // ── References ───────────────────────────────────
        [SerializeField] private Transform playerTransform;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        // ── Public accessors ─────────────────────────────
        public int CurrentCoinVault => coinVault;
        public int CoinVaultMax => GameBalance.Bosses.Niggel.CoinVaultMax;
        public int EnrageLevel => enrageLevel;

        // ────────────────────────────────────────────────
        //  Unity lifecycle
        // ────────────────────────────────────────────────

        protected override void Awake()
        {
            // Do NOT call base.Awake() — we manage currentHealth ourselves via coinVault sync.
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (playerTransform == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }

            // CoinVault init
            coinVault = GameBalance.Bosses.Niggel.CoinVaultMax;
            currentHealth = coinVault;
            maxHealth = GameBalance.Bosses.Niggel.CoinVaultMax;

            currentMoveSpeed = GameBalance.Bosses.Niggel.BaseMoveSpeed;
            dashCooldownTimer = GameBalance.Bosses.Niggel.BaseDashCooldown;

            isActive = false;  // starts idle — activates on first player input
            hasStarted = false;
        }

        private void Start()
        {
            AutoDetectArenaBounds();
        }

        /// <summary>Auto-calibrate arena clamp from the scene Tilemap, same approach as PblobController.</summary>
        private void AutoDetectArenaBounds()
        {
            Tilemap tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null) return;

            tilemap.CompressBounds();
            Bounds b = tilemap.localBounds;
            Vector3 scale = tilemap.transform.lossyScale;
            float halfWidth  = b.extents.x * Mathf.Abs(scale.x);
            float halfHeight = b.extents.y * Mathf.Abs(scale.y);

            arenaClampX = Mathf.Max(1f, halfWidth - 1f);
            arenaClampY = Mathf.Max(1f, halfHeight - 1f);

            Vector3 worldCenter = tilemap.transform.TransformPoint(b.center);
            if (Vector3.Distance(worldCenter, arenaCenter) < 20f)
                arenaCenter = worldCenter;

            Debug.Log($"[Niggel] Arena bounds auto-detected: ±{arenaClampX:F1}x ±{arenaClampY:F1}y  center={arenaCenter}");
        }

        private void Update()
        {
            // Idle: wait for any player input to start the fight
            if (!hasStarted)
            {
                if (Input.anyKeyDown)
                {
                    hasStarted = true;
                    isActive = true;
                    StartAttackRoutine();
                }
                return;
            }

            if (!isActive || isDead) return;

            // Move toward player
            if (playerTransform != null)
            {
                Vector2 target = Vector2.MoveTowards(transform.position, playerTransform.position,
                    currentMoveSpeed * Time.deltaTime);
                transform.position = new Vector3(
                    Mathf.Clamp(target.x, arenaCenter.x - arenaClampX, arenaCenter.x + arenaClampX),
                    Mathf.Clamp(target.y, arenaCenter.y - arenaClampY, arenaCenter.y + arenaClampY),
                    transform.position.z);
            }

            // Dash cooldown
            if (!isDashing)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer <= 0f)
                {
                    dashCooldownTimer = GetDashCooldown();
                    if (dashCoroutine != null) StopCoroutine(dashCoroutine);
                    dashCoroutine = StartCoroutine(Attack2_RichDash());
                }
            }
        }

        // ────────────────────────────────────────────────
        //  HP / Damage
        // ────────────────────────────────────────────────

        public override void TakeDamage(int amount)
        {
            if (isDead) return;

            coinVault = Mathf.Max(0, coinVault - amount);
            currentHealth = coinVault;
            Debug.Log($"[Niggel] TakeDamage({amount}) → CoinVault={coinVault}/{GameBalance.Bosses.Niggel.CoinVaultMax}");

            OnPlayerHitNiggel();
            CheckEnrageThresholds();

            if (coinVault <= 0) Die();
        }

        /// <summary>
        /// Heals Niggel's CoinVault without reverting enrage level.
        /// Called by healing coin attacks (Enrage 2+).
        /// </summary>
        public void HealCoinVault(int amount)
        {
            coinVault = Mathf.Min(coinVault + amount, GameBalance.Bosses.Niggel.CoinVaultMax);
            currentHealth = coinVault; // BossBase sync
            // Do NOT call CheckEnrageThresholds — thresholds are strictly one-way.
        }

        // ────────────────────────────────────────────────
        //  Enrage state machine (one-way)
        // ────────────────────────────────────────────────

        private void CheckEnrageThresholds()
        {
            if (enrageLevel < 1 && coinVault <= GameBalance.Bosses.Niggel.Enrage1Threshold)
                EnterEnrage(1);
            else if (enrageLevel < 2 && coinVault <= GameBalance.Bosses.Niggel.Enrage2Threshold)
                EnterEnrage(2);
            else if (enrageLevel < 3 && coinVault <= GameBalance.Bosses.Niggel.Enrage3Threshold)
                EnterEnrage(3);
        }

        private void EnterEnrage(int level)
        {
            enrageLevel = level;
            Debug.Log($"[Niggel] Entered enrage level {level}. CoinVault={coinVault}");

            switch (level)
            {
                case 1:
                    currentMoveSpeed = GameBalance.Bosses.Niggel.BaseMoveSpeed
                        * (1f + GameBalance.Bosses.Niggel.Enrage1SpeedBonus);
                    Debug.Log("[Niggel] Enrage 1: speed increased, spawning barriers.");
                    SpawnBarriers();
                    break;

                case 2:
                    dashCooldownTimer = GameBalance.Bosses.Niggel.Enrage2DashCooldown;
                    Debug.Log("[Niggel] Enrage 2: dash cooldown reduced, healing coins enabled.");
                    break;

                case 3:
                    currentMoveSpeed = GameBalance.Bosses.Niggel.BaseMoveSpeed
                        * (1f + GameBalance.Bosses.Niggel.Enrage3SpeedBonus);
                    dashCooldownTimer = GameBalance.Bosses.Niggel.Enrage3DashCooldown;
                    Debug.Log("[Niggel] Enrage 3: max speed, shield burst attacks enabled.");
                    break;
            }
        }

        /// <summary>Spawns black dot barrier rows at Enrage 1. Clears any previous barriers first.</summary>
        private void SpawnBarriers()
        {
            if (blackDotBarrierPrefab == null)
            {
                Debug.LogWarning("[Niggel] blackDotBarrierPrefab not assigned — skipping barrier spawn.");
                return;
            }

            // Clear existing barriers
            foreach (var b in activeBarriers)
                if (b != null) Destroy(b);
            activeBarriers.Clear();

            int rows = GameBalance.Bosses.Niggel.BarrierRowCount;
            int dots = GameBalance.Bosses.Niggel.BarrierDotsPerRow;
            float spacing = GameBalance.Bosses.Niggel.BarrierDotSpacing;

            for (int r = 0; r < rows; r++)
            {
                float rowY = arenaCenter.y + (r - rows / 2f) * 2f;
                for (int d = 0; d < dots; d++)
                {
                    float dotX = arenaCenter.x + (d - dots / 2f) * spacing;
                    GameObject dot = Instantiate(blackDotBarrierPrefab,
                        new Vector3(dotX, rowY, 0f), Quaternion.identity);
                    activeBarriers.Add(dot);
                }
            }

            Debug.Log($"[Niggel] Spawned {activeBarriers.Count} barrier dots.");
        }

        private float GetDashCooldown()
        {
            if (enrageLevel >= 3) return GameBalance.Bosses.Niggel.Enrage3DashCooldown;
            if (enrageLevel >= 2) return GameBalance.Bosses.Niggel.Enrage2DashCooldown;
            return GameBalance.Bosses.Niggel.BaseDashCooldown;
        }

        // ────────────────────────────────────────────────
        //  Player momentum
        // ────────────────────────────────────────────────

        private void OnPlayerHitNiggel()
        {
            playerMomentum = Mathf.Min(playerMomentum + 1, GameBalance.Bosses.Niggel.MaxMomentum);
            ApplyMomentumToPlayer();
            if (momentumResetCoroutine != null) StopCoroutine(momentumResetCoroutine);
            momentumResetCoroutine = StartCoroutine(MomentumResetTimer());
        }

        private IEnumerator MomentumResetTimer()
        {
            yield return new WaitForSeconds(GameBalance.Bosses.Niggel.MomentumResetDelay);
            playerMomentum = 0;
            ApplyMomentumToPlayer();
        }

        private void ApplyMomentumToPlayer()
        {
            PlayerDamageMultiplier = 1f + (playerMomentum * GameBalance.Bosses.Niggel.MomentumDamagePerStack);
            PlayerSpeedMultiplier = 1f + (playerMomentum * GameBalance.Bosses.Niggel.MomentumSpeedPerStack);
        }

        // ────────────────────────────────────────────────
        //  Attack routines
        // ────────────────────────────────────────────────

        private void StartAttackRoutine()
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            while (!isDead && isActive)
            {
                float interval = enrageLevel >= 2
                    ? GameBalance.Bosses.Niggel.Enrage2AttackInterval
                    : enrageLevel >= 1
                        ? GameBalance.Bosses.Niggel.Enrage1AttackInterval
                        : GameBalance.Bosses.Niggel.BaseAttackInterval;

                yield return new WaitForSeconds(interval);
                if (isDead || !isActive) break;

                // Enrage 3: sometimes do coin shield instead of regular attack
                if (enrageLevel >= 3 && !coinShieldActive && Random.value < 0.35f)
                {
                    if (shieldRoutine != null) StopCoroutine(shieldRoutine);
                    shieldRoutine = StartCoroutine(CoinShieldRoutine());
                    continue;
                }

                // Enrage 2+: sometimes fire a healing coin toward the player
                if (enrageLevel >= 2 && Random.value < 0.3f)
                {
                    FireHealingCoin();
                    continue;
                }

                // Default: throw a coin bag
                yield return StartCoroutine(Attack1_ThrowMoney());
            }
        }

        /// <summary>Throws a coin bag projectile at the player's current position.</summary>
        private IEnumerator Attack1_ThrowMoney()
        {
            if (coinBagPrefab == null || playerTransform == null) yield break;

            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject bag = Instantiate(coinBagPrefab, transform.position, Quaternion.Euler(0f, 0f, angle));

            // Tint yellow
            var bagSr = bag.GetComponent<SpriteRenderer>();
            if (bagSr != null) bagSr.color = Color.yellow;

            // Override velocity — speed scales with enrage
            var bagRb = bag.GetComponent<Rigidbody2D>();
            float spd = enrageLevel >= 1
                ? GameBalance.Bosses.Niggel.Enrage1CoinBagSpeed
                : GameBalance.Bosses.Niggel.CoinBagSpeed;
            if (bagRb != null) bagRb.velocity = dir * spd;
        }

        /// <summary>Dashes toward the player's current position, clamped to arena bounds.</summary>
        private IEnumerator Attack2_RichDash()
        {
            isDashing = true;

            Vector2 dashDir = playerTransform != null
                ? ((Vector2)playerTransform.position - (Vector2)transform.position).normalized
                : Vector2.right;

            Vector3 rawTarget = transform.position + (Vector3)(dashDir * GameBalance.Bosses.Niggel.DashDistance);
            rawTarget.x = Mathf.Clamp(rawTarget.x, arenaCenter.x - arenaClampX, arenaCenter.x + arenaClampX);
            rawTarget.y = Mathf.Clamp(rawTarget.y, arenaCenter.y - arenaClampY, arenaCenter.y + arenaClampY);

            float elapsed = 0f;
            Vector3 startPos = transform.position;
            float duration = GameBalance.Bosses.Niggel.DashDuration;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, rawTarget, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = rawTarget;
            isDashing = false;
        }

        /// <summary>Fires a healing coin toward the player. Called from AttackLoop at Enrage 2+.</summary>
        private void FireHealingCoin()
        {
            if (healingCoinPrefab == null || playerTransform == null) return;

            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject hc = Instantiate(healingCoinPrefab, transform.position, Quaternion.Euler(0f, 0f, angle));

            var hcRb = hc.GetComponent<Rigidbody2D>();
            if (hcRb != null) hcRb.velocity = dir * GameBalance.Bosses.Niggel.HealingCoinSpeed;

            var hcp = hc.GetComponent<HealingCoinProjectile>();
            if (hcp != null) hcp.boss = this;
        }

        /// <summary>Charge visual then burst coin bag projectiles in all directions. Enrage 3.</summary>
        private IEnumerator CoinShieldRoutine()
        {
            coinShieldActive = true;

            // Charge visual: pulse yellow <-> cyan
            Color original = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float chargeTime = GameBalance.Bosses.Niggel.ShieldChargeDuration;
            float t = 0f;
            while (t < chargeTime)
            {
                t += Time.deltaTime;
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.Lerp(Color.yellow, Color.cyan, Mathf.PingPong(t * 3f, 1f));
                yield return null;
            }
            if (spriteRenderer != null) spriteRenderer.color = original;

            // Burst N projectiles evenly in a full circle
            if (coinBagPrefab != null)
            {
                int count = GameBalance.Bosses.Niggel.ShieldBurstCount;
                float burstSpeed = GameBalance.Bosses.Niggel.Enrage1CoinBagSpeed;

                for (int i = 0; i < count; i++)
                {
                    float angleDeg = (360f / count) * i;
                    float angleRad = angleDeg * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                    GameObject proj = Instantiate(coinBagPrefab, transform.position,
                        Quaternion.Euler(0f, 0f, angleDeg));

                    var projRb = proj.GetComponent<Rigidbody2D>();
                    if (projRb != null) projRb.velocity = dir * burstSpeed;

                    // Orange tint to distinguish burst projectiles
                    var projSr = proj.GetComponent<SpriteRenderer>();
                    if (projSr != null) projSr.color = new Color(1f, 0.5f, 0f);
                }
            }

            coinShieldActive = false;
        }

        // Steal mechanic replaced by CoinVault design — see TakeDamage()
        // Attack3_StealStats() removed per plan 01 spec (RESEARCH.md Pitfall 1).

        // ────────────────────────────────────────────────
        //  Death
        // ────────────────────────────────────────────────

        // ── Debug overlay ───────────────────────────────────────────────

        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            GUILayout.BeginArea(new Rect(10, 10, 280, 180));
            GUI.Box(new Rect(0, 0, 280, 180), "");
            GUILayout.Label($"=== NIGGEL DEBUG ===");
            GUILayout.Label($"CoinVault: {coinVault} / {GameBalance.Bosses.Niggel.CoinVaultMax}");
            GUILayout.Label($"Enrage: {enrageLevel}  Dead: {isDead}  Active: {isActive}");
            GUILayout.Label($"Momentum: {playerMomentum}  DmgMult: {PlayerDamageMultiplier:F2}x");
            GUILayout.Label($"CoinShield: {coinShieldActive}  HasStarted: {hasStarted}");
            if (GUILayout.Button("Deal 50 dmg")) TakeDamage(50);
            if (GUILayout.Button("Kill")) TakeDamage(coinVault + 1);
            GUILayout.EndArea();
        }

        
protected override void Die()
        {
            isDead = true;
            isActive = false;

            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (momentumResetCoroutine != null) StopCoroutine(momentumResetCoroutine);
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);
            if (shieldRoutine != null) StopCoroutine(shieldRoutine);

            // Destroy all active barrier dots
            foreach (var b in activeBarriers)
                if (b != null) Destroy(b);
            activeBarriers.Clear();

            // Reset momentum multipliers so player returns to baseline after fight
            PlayerDamageMultiplier = 1f;
            PlayerSpeedMultiplier = 1f;
            playerMomentum = 0;

            if (rb != null) rb.velocity = Vector2.zero;

            base.Die(); // Awards currency and fires OnBossDefeated via BossBase
        }
    }
}
