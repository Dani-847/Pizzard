using System.Collections;
using UnityEngine;

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

        // ── Attack routines ──────────────────────────────
        private Coroutine attackRoutine;
        private bool isActive = false;

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

            isActive = true;
            StartAttackRoutine();
        }

        private void Update()
        {
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
            currentHealth = coinVault; // BossBase sync for BossHealthBarUI

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
                    // Stub: barrier spawning implemented in plan 02
                    Debug.Log("[Niggel] Spawn barriers");
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
        //  Attack stubs (implementations come in plan 02)
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
                if (!isDead && isActive) Attack1_ThrowMoney();
            }
        }

        private void Attack1_ThrowMoney()
        {
            // Stub — full coin bag projectile implementation in plan 02
            Debug.Log("[Niggel Attack 1] Stub: ThrowMoney — implementation in plan 02.");
        }

        private IEnumerator Attack2_RichDash()
        {
            // Stub — full dash implementation in plan 02
            isDashing = true;
            Debug.Log("[Niggel Attack 2] Stub: RichDash — implementation in plan 02.");
            yield return new WaitForSeconds(GameBalance.Bosses.Niggel.DashDuration);
            isDashing = false;
        }

        // Steal mechanic replaced by CoinVault design — see TakeDamage()
        // Attack3_StealStats() is removed per plan 01 spec (RESEARCH.md Pitfall 1).

        // ────────────────────────────────────────────────
        //  Death
        // ────────────────────────────────────────────────

        protected override void Die()
        {
            isDead = true;
            isActive = false;

            if (attackRoutine != null) StopCoroutine(attackRoutine);
            if (momentumResetCoroutine != null) StopCoroutine(momentumResetCoroutine);
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);

            // Reset momentum multipliers so player returns to baseline after fight
            PlayerDamageMultiplier = 1f;
            PlayerSpeedMultiplier = 1f;
            playerMomentum = 0;

            if (rb != null) rb.velocity = Vector2.zero;

            base.Die(); // Awards currency and fires OnBossDefeated via BossBase
        }
    }
}
