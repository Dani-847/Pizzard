---
phase: 19-boss-2-niggel-worthington-ai
plan: "03"
subsystem: UI / Scene Wiring
tags: [boss, niggel, hud, coinmeter, scene-wiring, prefabs, editor-tool]
dependency_graph:
  requires: [NiggelController.CurrentCoinVault, NiggelController.CoinVaultMax, BossHealthBarUI, CoinBagProjectile, HealingCoinProjectile, BlackDotBarrier]
  provides: [NiggelCoinMeterUI, NiggelArenaSetup, BossArena_2 wired]
  affects: [BossArena_2, Player HUD]
tech_stack:
  added: []
  patterns: [FindObjectOfType lazy-init pattern for HUD, Editor automation script for prefab/scene wiring]
key_files:
  created:
    - Assets/Scripts/UI/NiggelCoinMeterUI.cs
    - Assets/Scripts/Editor/NiggelArenaSetup.cs
  modified:
    - ProjectSettings/TagManager.asset
decisions:
  - NiggelCoinMeterUI uses FindObjectOfType in OnEnable and Update fallback — same pattern as BossHealthBarUI, no direct scene reference needed
  - BossHealthBarUI requires no code changes — it already displays raw numbers via CurrentHealthPublic (which syncs to coinVault in NiggelController)
  - BossBarrier layer added at index 11 (next free slot after BossCircle at 10) — resolves BlackDotBarrier Awake() LayerMask.NameToLayer warning
  - NiggelArenaSetup Editor script automates all scene wiring (prefab creation, SerializeField assignment, HUD placement, scene save) — user runs once via Tools/Pizzard/Setup Niggel Arena
metrics:
  duration: "~10 minutes"
  completed_date: "2026-03-01"
  tasks_completed: 1
  files_modified: 3
---

# Phase 19 Plan 03: Niggel Scene Wiring Summary

NiggelCoinMeterUI HUD script, BossBarrier layer registration, and a one-click Editor tool that creates the three boss prefabs and wires all NiggelController SerializeFields in BossArena_2.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create NiggelCoinMeterUI and wire BossArena_2 scene | fa7251f | NiggelCoinMeterUI.cs, NiggelArenaSetup.cs, TagManager.asset |

## What Was Built

**NiggelCoinMeterUI.cs** — Player-side HUD component placed below the mana bar. Each frame it reads `NiggelController.CurrentCoinVault` and `CoinVaultMax`, computes `stolen = max - current`, fills an Image component left-to-right (0 = fight start, 1 = boss dead), and writes the stolen count to a TextMeshProUGUI. Uses `FindObjectOfType` with a null-guard fallback so it tolerates load order variations.

**ProjectSettings/TagManager.asset** — Added "BossBarrier" at layer index 11 (was empty). This resolves the `LayerMask.NameToLayer("BossBarrier")` warning that BlackDotBarrier emits in Awake() when the layer is missing.

**NiggelArenaSetup.cs** (Editor, `Tools/Pizzard/Setup Niggel Arena`) — Automates all scene wiring:
- Creates `Assets/Prefabs/Bosses/` directory if missing
- Creates CoinBag prefab (SpriteRenderer gold, CircleCollider2D trigger r=0.2, Rigidbody2D no gravity, CoinBagProjectile component)
- Creates HealingCoin prefab (same as CoinBag but HealingCoinProjectile component)
- Creates BlackDotBarrier prefab (SpriteRenderer black, CircleCollider2D trigger r=0.15, no Rigidbody2D, BossBarrier layer, BlackDotBarrier component)
- Opens BossArena_2, finds NiggelController, assigns `coinBagPrefab`, `healingCoinPrefab`, `blackDotBarrierPrefab`, `arenaCenter=(0,0)`, `arenaClampX=7`, `arenaClampY=4`
- Adds NiggelCoinMeterUI panel below ManaUI in the HUD Canvas with wired fillBar and coinCountText
- Saves the scene

**BossHealthBarUI** — No changes needed. Already displays `{current}/{max}` raw numbers. Since NiggelController syncs `currentHealth = coinVault` on every change, the bar will show coin count correctly.

## Verification Results

- NiggelCoinMeterUI.cs compiles (uses proper Pizzard.UI namespace, correct using directives)
- BossBarrier layer confirmed at TagManager.asset line 29
- NiggelArenaSetup.cs compiles in Editor assembly with all required using directives
- Commit fa7251f confirmed

## Checkpoint: Human Verification Required

The plan includes a `checkpoint:human-verify` gate (Task 2). The human must:

1. Open Unity Editor
2. Run `Tools/Pizzard/Setup Niggel Arena` to create prefabs and wire the scene
3. Open BossArena_2.unity and enter Play mode
4. Verify: coin meter fills as Niggel takes damage, enrage thresholds at 150/100/50 coins trigger visually, momentum makes player perceptibly faster/stronger, boss defeat advances the game
5. Confirm zero null reference errors in Console

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] BossBarrier layer missing from TagManager**
- **Found during:** Task 1
- **Issue:** BlackDotBarrier.Awake() calls `LayerMask.NameToLayer("BossBarrier")` but the layer was not registered in TagManager.asset — it would return -1 and the barrier would log a warning and skip all collision setup
- **Fix:** Added "BossBarrier" entry to TagManager.asset layers array at index 11
- **Files modified:** ProjectSettings/TagManager.asset

**2. [Rule 2 - Missing Critical Functionality] No automation for scene wiring**
- **Found during:** Task 1
- **Issue:** Plan specified manual Unity Editor steps for scene wiring (Step 2) that cannot be performed via file writes alone (scene is binary YAML, prefabs need Unity runtime)
- **Fix:** Created NiggelArenaSetup.cs Editor script that performs all wiring steps programmatically when run from the Tools menu — reduces manual error, consistent with existing BuildSettingsSetup.cs pattern

## Self-Check: PASSED

- Assets/Scripts/UI/NiggelCoinMeterUI.cs — FOUND
- Assets/Scripts/Editor/NiggelArenaSetup.cs — FOUND
- ProjectSettings/TagManager.asset — BossBarrier layer at line 29 — FOUND
- Commit fa7251f — confirmed in git log
