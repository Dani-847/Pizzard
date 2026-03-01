---
phase: 19-boss-2-niggel-worthington-ai
plan: "02"
subsystem: Boss AI
tags: [boss, niggel, attacks, projectiles, barriers, enrage, coinshield]
dependency_graph:
  requires: [NiggelController.TakeDamage, NiggelController.HealCoinVault, GameBalance.Bosses.Niggel, EnemyProjectile]
  provides: [CoinBagProjectile, HealingCoinProjectile, CoinShieldBurst, BlackDotBarrier, NiggelController.Attack1_ThrowMoney, NiggelController.Attack2_RichDash, NiggelController.SpawnBarriers, NiggelController.CoinShieldRoutine, NiggelController.FireHealingCoin]
  affects: [PlayerHPController, NiggelController.coinVault, BossArena_2]
tech_stack:
  added: []
  patterns: [EnemyProjectile subclass pattern, coroutine-based attack loop, enrage-gated randomized attacks, layer collision ignore for barrier pass-through]
key_files:
  created:
    - Assets/Scripts/Bosses/Projectiles/CoinBagProjectile.cs
    - Assets/Scripts/Bosses/Projectiles/HealingCoinProjectile.cs
    - Assets/Scripts/Bosses/Projectiles/CoinShieldBurst.cs
    - Assets/Scripts/Bosses/Barriers/BlackDotBarrier.cs
  modified:
    - Assets/Scripts/Bosses/NiggelController.cs
decisions:
  - CoinShieldBurst is an unused standalone component — CoinShieldRoutine inlined the burst logic into NiggelController directly using coinBagPrefab, so CoinShieldBurst.Burst() exists but is not called by NiggelController (simplifies wiring, avoids extra GetComponent overhead)
  - HealingCoinProjectile fully overrides OnTriggerEnter2D rather than calling base — base would never damage the player (damage=0) but also would destroy on any untagged collision; full override gives precise control over what destroys the coin
  - Attack1_ThrowMoney changed from void to IEnumerator — allows yield return in AttackLoop so the loop properly waits for throw completion before choosing next attack; consistent with Attack2_RichDash pattern
  - CoinShieldRoutine inlines burst logic rather than using CoinShieldBurst component — reduces coupling, avoids runtime GetComponent, easier to maintain
metrics:
  duration: "~20 minutes"
  completed_date: "2026-03-01"
  tasks_completed: 2
  files_modified: 5
---

# Phase 19 Plan 02: Niggel Attack Implementations Summary

All Niggel attack types implemented — coin bag throw, arena-clamped dash, healing coin (Enrage 2), coin shield burst (Enrage 3), and black dot barriers (Enrage 1) — wired into NiggelController's attack loop gated by enrage level.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create projectile scripts and BlackDotBarrier | 51bcec0 | Assets/Scripts/Bosses/Projectiles/CoinBagProjectile.cs, HealingCoinProjectile.cs, CoinShieldBurst.cs, Assets/Scripts/Bosses/Barriers/BlackDotBarrier.cs |
| 2 | Wire attack coroutines into NiggelController | a556c99 | Assets/Scripts/Bosses/NiggelController.cs |

## What Was Built

**CoinBagProjectile** — extends EnemyProjectile. Destroys on `Wall` tag before base logic. Damage from `GameBalance.Bosses.Niggel.CoinBagDamage`. NiggelController sets velocity after spawn (direction-aimed, enrage-scaled speed).

**HealingCoinProjectile** — extends EnemyProjectile with `damage = 0f`. On player contact: calls `boss.HealCoinVault(HealingCoinAmount)` then destroys. Pulses yellow-to-orange via `Color.Lerp + PingPong` in Update. Boss reference set at spawn time by `FireHealingCoin()`.

**CoinShieldBurst** — standalone MonoBehaviour helper with a `Burst(prefab, speed, count)` method that fires N projectiles in an even 360-degree spread. Note: NiggelController inlined the burst logic into `CoinShieldRoutine()` for simplicity — CoinShieldBurst exists for future use but is not currently called.

**BlackDotBarrier** — trigger collider placed on "BossBarrier" layer. In Awake(): `Physics2D.IgnoreLayerCollision` disables collision with Default layer (player) and EnemyProjectile layer (boss shots). `OnTriggerEnter2D` destroys any object on the PlayerProjectiles layer. Warns if BossBarrier layer is missing.

**NiggelController (updated):**
- Added `[SerializeField]` for `coinBagPrefab`, `healingCoinPrefab`, `blackDotBarrierPrefab`
- `Attack1_ThrowMoney()` — coroutine, instantiates coin bag aimed at player, yellow tint, speed from GameBalance (scales at enrage 1+)
- `Attack2_RichDash()` — coroutine, Lerp-moves boss toward player, clamped to arenaClampX/Y bounds
- `SpawnBarriers()` — clears + spawns BarrierRowCount x BarrierDotsPerRow black dot barrier instances
- `FireHealingCoin()` — instantiates healing coin aimed at player, sets boss reference
- `CoinShieldRoutine()` — 1.5s yellow/cyan pulse charge, then fires ShieldBurstCount (8) orange coin bags in 360-degree spread
- `AttackLoop()` — Enrage 3: 35% coin shield, Enrage 2+: 30% healing coin, default: coin bag throw
- `EnterEnrage(1)` — now calls `SpawnBarriers()` instead of `Debug.Log` stub
- `Die()` — stops `shieldRoutine`, destroys all `activeBarriers` and clears list

## Verification Results

- CoinBagProjectile destroys on Wall tag before base.OnTriggerEnter2D
- HealingCoinProjectile: damage=0f, calls boss.HealCoinVault on player hit, does NOT call base
- BlackDotBarrier: IgnoreLayerCollision for Default and EnemyProjectile layers; OnTriggerEnter2D destroys PlayerProjectiles layer objects
- EnterEnrage(1) calls SpawnBarriers() — no more debug stub
- Die() stops shieldRoutine and clears activeBarriers list
- All attack coroutines null-guarded on coinBagPrefab and playerTransform

## Deviations from Plan

### Minor Structural Decisions

**1. [Rule 1 - Design] Attack1_ThrowMoney changed to IEnumerator**
- **Found during:** Task 2
- **Issue:** Plan spec showed it as `void`; but `AttackLoop` uses `yield return StartCoroutine(...)` to wait for throw completion
- **Fix:** Changed to `private IEnumerator Attack1_ThrowMoney()` — single frame execution, no actual yield needed, but consistent with coroutine pattern
- **Files modified:** NiggelController.cs

**2. [Design] CoinShieldBurst inlined into CoinShieldRoutine**
- CoinShieldBurst.cs exists and compiles but CoinShieldRoutine() inlines the burst loop directly using `coinBagPrefab` — avoids `GetComponent<CoinShieldBurst>()` overhead and keeps all Niggel attack code in one place

## Self-Check: PASSED

- Assets/Scripts/Bosses/Projectiles/CoinBagProjectile.cs — FOUND
- Assets/Scripts/Bosses/Projectiles/HealingCoinProjectile.cs — FOUND
- Assets/Scripts/Bosses/Projectiles/CoinShieldBurst.cs — FOUND
- Assets/Scripts/Bosses/Barriers/BlackDotBarrier.cs — FOUND
- Assets/Scripts/Bosses/NiggelController.cs — FOUND (modified)
- Commit 51bcec0 — confirmed in git log
- Commit a556c99 — confirmed in git log
