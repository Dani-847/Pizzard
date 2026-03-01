---
phase: 19-boss-2-niggel-worthington-ai
plan: "01"
subsystem: Boss AI
tags: [boss, niggel, coinvault, enrage, momentum, gamebalance]
dependency_graph:
  requires: []
  provides: [NiggelController.TakeDamage, NiggelController.HealCoinVault, NiggelController.CurrentCoinVault, NiggelController.PlayerDamageMultiplier, NiggelController.PlayerSpeedMultiplier, GameBalance.Bosses.Niggel]
  affects: [BossHealthBarUI, PlayerController]
tech_stack:
  added: []
  patterns: [CoinVault HP system, one-way enrage state machine, static multiplier fields for cross-script momentum]
key_files:
  created: []
  modified:
    - Assets/Scripts/Core/GameBalance.cs
    - Assets/Scripts/Bosses/NiggelController.cs
decisions:
  - CoinVault syncs to BossBase.currentHealth on every change for BossHealthBarUI compatibility without modifying UI code
  - enrageLevel is an integer (0-3) checked via one-way threshold guards (enrageLevel < N) in TakeDamage only, never in HealCoinVault
  - PlayerDamageMultiplier and PlayerSpeedMultiplier are public static fields so PlayerController can read them without a direct reference
  - Attack stubs left in NiggelController with Debug.Log so plan 02 can drop in implementations without structural changes
metrics:
  duration: "~15 minutes"
  completed_date: "2026-03-01"
  tasks_completed: 2
  files_modified: 2
---

# Phase 19 Plan 01: NiggelController Foundation Summary

CoinVault HP system, one-way enrage state machine (levels 0-3), and player momentum multipliers — all constants driven from GameBalance.Bosses.Niggel.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Expand GameBalance.Bosses.Niggel with all constants | c212ed4 | Assets/Scripts/Core/GameBalance.cs |
| 2 | Rewrite NiggelController with CoinVault, enrage, momentum | eff4aaf | Assets/Scripts/Bosses/NiggelController.cs |

## What Was Built

**GameBalance.Bosses.Niggel** expanded from 4 legacy constants to 30 constants covering:
- CoinVault: `CoinVaultMax=200`, `Enrage1Threshold=150`, `Enrage2Threshold=100`, `Enrage3Threshold=50`
- Movement: `BaseMoveSpeed=2.5f`, speed bonuses per enrage level
- Dash: `BaseDashCooldown=20f`, `Enrage2DashCooldown=12f`, `Enrage3DashCooldown=8f`, `DashDistance`, `DashDuration`
- Coin bag, healing coin, shield burst, black dot barrier constants
- Momentum: `MaxMomentum=5`, `MomentumResetDelay=4f`, `MomentumDamagePerStack=0.05f`, `MomentumSpeedPerStack=0.03f`
- Attack timing per enrage level

**NiggelController** fully rewritten:
- Inherits BossBase, overrides TakeDamage() (never calls base.TakeDamage())
- `coinVault` field (int) is the authoritative HP; `currentHealth` is synced on every change
- `CheckEnrageThresholds()` called from TakeDamage only — guards: `enrageLevel < N && coinVault <= threshold`
- `HealCoinVault(int amount)` updates coinVault + currentHealth, never calls CheckEnrageThresholds
- `OnPlayerHitNiggel()` increments playerMomentum (capped at MaxMomentum), restarts 4s reset coroutine
- `ApplyMomentumToPlayer()` sets static `PlayerDamageMultiplier` and `PlayerSpeedMultiplier`
- `Die()` resets both static multipliers to 1f, stops all coroutines, calls `base.Die()`
- Attack1_ThrowMoney and Attack2_RichDash are stubs (plan 02 fills implementations)
- Attack3_StealStats removed; steal mechanic replaced by CoinVault design

## Verification Results

- `CoinVaultMax`, `Enrage1Threshold`, `MaxMomentum`, `MomentumResetDelay` confirmed in GameBalance via grep
- `base.TakeDamage` does NOT appear in NiggelController
- `HealCoinVault` does NOT contain a call to `CheckEnrageThresholds`
- `PlayerDamageMultiplier` is static, initialized to 1f, and reset to 1f in Die()
- Legacy constants (AttackInterval, StealRange, CurrencyStealAmount, SpeedBuffPerSteal) preserved at bottom of Niggel class

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

- Assets/Scripts/Core/GameBalance.cs — FOUND
- Assets/Scripts/Bosses/NiggelController.cs — FOUND
- Commit c212ed4 — confirmed in git log
- Commit eff4aaf — confirmed in git log
