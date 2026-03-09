---
phase: 24-playground-mode
plan: "04"
subsystem: playground
tags: [playground, training-dummy, dps-tracker, hud, shop]
dependency_graph:
  requires: [24-01, 24-02, 24-03]
  provides: [DummyDPSTracker, PlaygroundHUDController, PlaygroundScene]
  affects: [CharacterProjectile collision routing, ShopUI token source]
tech_stack:
  added: []
  patterns: [BossBase override for invincible dummy, Editor scene-generation tool]
key_files:
  created:
    - Assets/Scripts/Playground/DummyDPSTracker.cs
    - Assets/Scripts/Playground/PlaygroundHUDController.cs
    - Assets/Scripts/Editor/PlaygroundSceneSetup.cs
  modified:
    - Assets/Scripts/Editor/BuildSettingsSetup.cs
decisions:
  - DummyDPSTracker extends BossBase (not standalone MonoBehaviour) so CharacterProjectile tag+TakeDamage routing works without modifying CharacterProjectile
  - Scene created via Editor MenuItem (Tools/Pizzard/Setup Playground Scene) rather than a binary .unity file — maintains YAML correctness and respects existing FlowScenes/ convention
  - Scene path is Assets/FlowScenes/PlaygroundScene.unity (project uses FlowScenes/, not Assets/Scenes/)
metrics:
  duration: ~15 minutes
  tasks_completed: 2
  files_created: 3
  files_modified: 1
  completed_date: "2026-03-08"
---

# Phase 24 Plan 04: Playground Scene + Dummy DPS Tracker Summary

**One-liner:** Training dummy with rolling 3s DPS counter (via BossBase override) + HUD with shop pause and back-to-menu, wired by an Editor scene-generation tool.

## What Was Built

### DummyDPSTracker.cs
Extends `Pizzard.Bosses.BossBase` so that `CharacterProjectile.OnTriggerEnter2D` routes hits through the existing tag="Boss" + `TakeDamage(int)` path without any modifications to the projectile system. The override discards HP reduction (dummy is invincible) and calls `RegisterHit(float)` instead. `Update()` maintains a `List<(time, damage)>` and prunes entries older than 3 seconds every frame, displaying `sum/3` in a world-space `TextMeshPro` field.

### PlaygroundHUDController.cs
Screen-space HUD controller with:
- `OpenShop()` — calls `shopUI.SetTokenSource(PlaygroundManager.Instance)`, sets `Time.timeScale = 0f`, calls `shopUI.Show()`
- `CloseShop()` — calls `shopUI.Hide(suppressSave: true)`, restores `Time.timeScale = 1f`
- `BackToMenu()` — restores timeScale and calls `Pizzard.Core.SceneLoader.LoadScene("MainMenu")`
- `RefreshTokenDisplay()` — syncs token counter text with `PlaygroundManager.PlaygroundTokens`

### PlaygroundSceneSetup.cs (Editor)
`[MenuItem("Tools/Pizzard/Setup Playground Scene")]` creates `Assets/FlowScenes/PlaygroundScene.unity` programmatically: PlaygroundManager, TrainingDummy (tagged "Boss", DummyDPSTracker, world-space DPS canvas), PlayerSpawnPoint, HUD Canvas with all Inspector refs wired via `SerializedObject`. Appends the scene to Build Settings automatically.

### BuildSettingsSetup.cs (updated)
Added `Assets/FlowScenes/PlaygroundScene.unity` to the required scenes array.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Integration] DummyDPSTracker extends BossBase instead of standalone MonoBehaviour**
- **Found during:** Task 1
- **Issue:** CharacterProjectile only calls `TakeDamage` on `BossBase` or `PblobController` components found on tag="Boss" objects. A plain MonoBehaviour with `TakeDamage(int)` would never be called.
- **Fix:** Made `DummyDPSTracker` extend `BossBase` and override `TakeDamage` to suppress HP/death while routing to `RegisterHit`.
- **Files modified:** Assets/Scripts/Playground/DummyDPSTracker.cs

**2. [Rule 3 - Convention] Scene path corrected to FlowScenes/**
- **Found during:** Task 2
- **Issue:** Plan specified `Assets/Scenes/PlaygroundScene.unity` but project uses `Assets/FlowScenes/` for all runtime scenes.
- **Fix:** Scene created at `Assets/FlowScenes/PlaygroundScene.unity` to match project conventions.
- **Files modified:** Assets/Scripts/Editor/PlaygroundSceneSetup.cs, Assets/Scripts/Editor/BuildSettingsSetup.cs

**3. [Rule 3 - Tooling] Scene created via Editor script, not binary file**
- **Found during:** Task 2
- **Issue:** Unity scene files reference GUIDs and cannot be hand-crafted correctly. Creating a binary .unity file outside of Unity would produce a corrupt/unloadable scene.
- **Fix:** Implemented `PlaygroundSceneSetup` Editor MenuItem that generates the scene inside Unity at first run. Developer runs `Tools/Pizzard/Setup Playground Scene` once.
- **Files modified:** Assets/Scripts/Editor/PlaygroundSceneSetup.cs (new)

## Next Steps (Plan 05)
- Run `Tools/Pizzard/Setup Playground Scene` in the Unity Editor to generate PlaygroundScene.unity
- Assign a visible sprite to TrainingDummy in Inspector
- Wire the real ShopUI prefab to the HUD Canvas ShopUI slot
- Place the Player prefab at PlayerSpawnPoint

## Self-Check: PASSED

- [x] DummyDPSTracker.cs created — `Assets/Scripts/Playground/DummyDPSTracker.cs`
- [x] PlaygroundHUDController.cs created — `Assets/Scripts/Playground/PlaygroundHUDController.cs`
- [x] PlaygroundSceneSetup.cs created — `Assets/Scripts/Editor/PlaygroundSceneSetup.cs`
- [x] BuildSettingsSetup.cs updated
- [x] Task 1 commit: `0257559`
- [x] Task 2 commit: `a2e67d3`
