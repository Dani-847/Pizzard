---
phase: 18
plan: 4
wave: 1
---

# Plan 18.4.2 — Esc Pause in Boss Fight & Phase 3 Grid Spawn

## Objective
Allow the player to pause with Esc during the boss fight, and fix Phase 3 so the grid puzzle spawns and the player is relocated correctly.

## Context
- `Assets/Scripts/Pblob/PblobController.cs`
- `Assets/Scripts/Pblob/PblobGridPuzzle.cs`
- `Assets/Prefabs/Base/Pblob.prefab`
- `Assets/Scripts/GameFlowManager/` — existing pause/menu system

## Tasks

<task type="auto">
  <name>Create PauseBossArena component — Esc key pauses game in boss scene</name>
  <files>Assets/Scripts/GameFlowManager/PauseBossArena.cs [NEW]</files>
  <action>
    Create a new MonoBehaviour `PauseBossArena`:
    ```
    - Field: bool isPaused = false
    - Update(): if Input.GetKeyDown(KeyCode.Escape) → TogglePause()
    - TogglePause():
        isPaused = !isPaused
        Time.timeScale = isPaused ? 0f : 1f
        Find MenuUI via FindObjectOfType<MenuUI>() and call ShowPauseMenu(isPaused)
        (If MenuUI has no ShowPauseMenu method, use gameObject.SetActive(isPaused) on the MenuUI's parent panel)
    - OnDestroy(): Time.timeScale = 1f  // safety — always restore on scene unload
    ```
    Look up what existing pause method MenuUI / UIManager expose before implementing.
    If MenuUI has a TogglePause() or ShowMenu() already, call that.
    Do NOT duplicate existing pause logic — extend it.
    
    Add this component to the Pblob prefab (Assets/Prefabs/Base/Pblob.prefab) as a new component via MCP manage_prefabs or manage_components.
  </action>
  <verify>Play BossArena_1 → press Esc → game freezes (Time.timeScale=0) and menu UI appears. Press Esc again → game resumes.</verify>
  <done>Esc toggles pause during boss fight. Time.timeScale correctly set. No input accepted while paused.</done>
</task>

<task type="auto">
  <name>Fix Phase 3 grid spawn — auto-find PblobGridPuzzle + correct player relocation</name>
  <files>Assets/Scripts/Pblob/PblobController.cs</files>
  <action>
    In PblobController.Start():
    - Add: `if (gridPuzzle == null) gridPuzzle = FindObjectOfType<PblobGridPuzzle>();`
    (Same pattern used for rhythmManager)

    In ChangeState() case Phase3_Grid:
    - If gridPuzzle is still null after auto-find: `Debug.LogError("[Pblob] Phase3_Grid: gridPuzzle not found in scene!")` then break
    - If found: set spawn position using GameBalance.Bosses.Pblob.GridSpawnOffsetY:
      ```
      Vector3 spawnPos = arenaCenter + new Vector3(0, GameBalance.Bosses.Pblob.GridSpawnOffsetY, 0);
      gridPuzzle.GenerateGrid(spawnPos);
      ```
    
    In Phase3TransitionRoutine():
    - Player target: arenaCenter + new Vector3(0, GameBalance.Bosses.Pblob.GridSpawnOffsetY + 1f, 0)
      (player spawns just above the grid bottom)
    - Boss target: arenaCenter + new Vector3(0, Mathf.Abs(GameBalance.Bosses.Pblob.GridSpawnOffsetY) - 2f, 0)
      (boss moves to top of arena, above the grid)
    - After positioning: freeze player movement during grid reveal (set playerTransform rigidbody to kinematic or find PlayerMovement and disable it temporarily via a 2s coroutine)

    Add a coroutine `FreezePlayerBriefly(float seconds)` that:
    - Finds PlayerMovement component
    - Disables it for `seconds`
    - Re-enables it
  </action>
  <verify>In Play mode: reach Phase 3 (take ~667 damage to boss) → grid tiles appear below boss → player relocates to bottom area → grid reveals green/red path</verify>
  <done>Grid spawns. Player visibly relocated. Grid path visible. No null reference errors in console.</done>
</task>

## Success Criteria
- [ ] Esc pauses/unpauses during boss fight
- [ ] Phase 3 grid appears when boss HP < 33%
- [ ] Player relocated to bottom of arena at Phase 3 start
- [ ] Boss moves to top of arena at Phase 3 start
- [ ] No MissingReferenceException for gridPuzzle
