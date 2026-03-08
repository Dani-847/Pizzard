---
phase: 24-playground-mode
plan: "05"
subsystem: playground
tags: [hazard, respawn, player-death, projectile-spawner]
dependency_graph:
  requires: [24-04]
  provides: [falling-projectile-hazard, safe-death-respawn, onDeathOverride-hook]
  affects: [PlayerHPController, PlaygroundScene]
tech_stack:
  added: []
  patterns: [Action-override-hook, WaitForSecondsRealtime, coroutine-respawn]
key_files:
  created:
    - Assets/Scripts/Playground/PlaygroundProjectileSpawner.cs
    - Assets/Scripts/Playground/PlaygroundRespawnHandler.cs
  modified:
    - Assets/Scripts/Player/PlayerHPController.cs
decisions:
  - Used WaitForSecondsRealtime for respawn delay so it works even if timeScale is paused
  - Applied downward velocity override one frame after instantiation so it runs after EnemyProjectile.Start()
  - RestaurarVidaCompleta() used for HP restore (existing Spanish-named method)
  - onDeathOverride checked before GameFlowManager so existing death flow is 100% unmodified when null
metrics:
  duration: ~15min
  completed: 2026-03-08
  tasks_completed: 2
  files_created: 2
  files_modified: 1
---

# Phase 24 Plan 05: Falling Hazard and Respawn Handler Summary

**One-liner:** PlayerHPController onDeathOverride hook plus PlaygroundRespawnHandler and PlaygroundProjectileSpawner enable risk-free infinite experimentation in the Playground sandbox.

## What Was Built

### Task 1 — onDeathOverride hook + PlaygroundRespawnHandler (commit eb4e4ed)

**PlayerHPController.cs change:**
- Added `public System.Action onDeathOverride;` field
- `OnDeath()` now checks this field first; if set it invokes it and returns — GameFlowManager is never reached
- When `onDeathOverride` is null, existing behavior is completely unchanged

**PlaygroundRespawnHandler.cs (new):**
- Sets `playerHP.onDeathOverride` in `Awake()` to a coroutine that waits 1.5 real-time seconds then teleports the player to `spawnPoint` and calls `RestaurarVidaCompleta()`
- Uses `WaitForSecondsRealtime` so the brief pause works even if `timeScale` is 0
- Clears the override in `OnDestroy()` so it cannot bleed into other scenes

### Task 2 — PlaygroundProjectileSpawner (commit f2a4f5c)

**PlaygroundProjectileSpawner.cs (new):**
- Infinite coroutine `SpawnLoop` waits `spawnInterval` (3s) then checks `Time.timeScale > 0`
- If timeScale is positive, instantiates `projectilePrefab` at `transform.position`
- One frame later overrides `Rigidbody2D.velocity` to `Vector2.down * projectileSpeed` (5 u/s), winning over EnemyProjectile's own `Start()` which sets horizontal velocity

### Scene Wiring (manual — required before human-verify checkpoint)

The following must be done in the Unity Editor in PlaygroundScene:

1. Select **ProjectileSpawnPoint** GameObject (left side, created in Plan 04) — add `PlaygroundProjectileSpawner` component, assign an EnemyProjectile prefab (e.g. `pineappleChikitProjectile.prefab`) to `projectilePrefab`
2. Create new empty GameObject named **RespawnHandler** — add `PlaygroundRespawnHandler` component, assign `Player.PlayerHPController` to `playerHP`, assign `PlayerSpawnPoint` Transform to `spawnPoint`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] EnemyProjectile.Start() sets horizontal velocity, overriding any velocity set at spawn time**
- **Found during:** Task 2 implementation
- **Issue:** EnemyProjectile.Start() runs after Instantiate returns and sets `rb.velocity = transform.right * speed`, which would cancel a downward velocity set inline
- **Fix:** Added one-frame delay coroutine `ApplyDownwardVelocity` that runs after EnemyProjectile.Start() and then overrides with `Vector2.down * projectileSpeed`
- **Files modified:** PlaygroundProjectileSpawner.cs
- **Commit:** f2a4f5c

## Self-Check

- [x] PlayerHPController.cs modified with onDeathOverride field and guard
- [x] PlaygroundRespawnHandler.cs created at correct path
- [x] PlaygroundProjectileSpawner.cs created at correct path
- [x] Commits eb4e4ed and f2a4f5c exist
