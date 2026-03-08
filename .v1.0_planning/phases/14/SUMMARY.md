---
phase: 14
subsystem: dialogue-system
tags: [dialogue, overlay, typewriter, portraits, localization, input-freeze]
dependency_graph:
  requires: [phase-13]
  provides: [dialogue-overlay, typewriter-ui, input-freeze-during-dialogue]
  affects: [DialogUI, GameFlowManager, UIManager, PlayerController, PlayerAimAndCast]
key_files:
  modified:
    - Assets/Scripts/UI/DialogUI.cs
    - Assets/Scripts/GameFlowManager/GameFlowManager.cs
    - Assets/Scripts/UI/UIManager.cs
    - Assets/Scripts/Player/PlayerController.cs
    - Assets/Scripts/Player/PlayerAimAndCast.cs
    - Assets/Resources/Languages/en.json
    - Assets/Resources/Languages/es.json
  created:
    - Assets/Sprites/Portraits/Bob_Portrait.png
    - Assets/Sprites/Portraits/Raberto_Portrait.png
  archived:
    - Assets/Scripts/GameFlowManager/IntroManager.cs → Archive/Scripts/
    - Assets/FlowScenes/IntroDialog.unity → Archive/Scenes/
    - Assets/FlowScenes/PreBossDialog.unity → Archive/Scenes/
    - Assets/FlowScenes/PostBossDialog.unity → Archive/Scenes/
decisions:
  - "Overlay approach — dialogue UI on UIManager canvas, no scene loads"
  - "Click-anywhere to advance — removed Next button"
  - "Typewriter effect with instant-complete on click"
  - "Placeholder portrait colors: cyan=Bob, orange=Raberto"
  - "Placeholder dialogue format: Dialog N (LAN)"
metrics:
  completed: "2026-02-28"
  files_modified: 7
  files_created: 2
  files_archived: 4
---

# Phase 14: Dialogue System & Narrative Flow — Summary

**One-liner:** Reworked dialogue from scene-based to overlay-based with typewriter text, click-anywhere advance, portrait placeholders, and input freezing.

## What Was Built

- **DialogUI.cs** — Full rewrite: overlay on UIManager canvas, typewriter text reveal (configurable speed), click-anywhere to advance (mouse/keyboard/space), speaker name label, left/right portrait Image slots.
- **GameFlowManager.cs** — `IsDialogueActive` property covering all 3 states. `GetSceneForState()` returns empty string for dialogue states (no scene load).
- **Player input freeze** — `PlayerController` and `PlayerAimAndCast` use `IsDialogueActive` for all movement, dash, aiming, and casting guards.
- **UIManager.cs** — `ForceApplyUIForState` handles PreBossDialogue and PostBossDialogue.
- **Localization JSONs** — Placeholder format `"Dialog N (EN/ES)"` for easy content swap later.
- **Portrait textures** — 200×200 placeholder rectangles at `Assets/Sprites/Portraits/`.

## Verification Results

- 0 compile errors ✅
- 0 console error entries ✅
- `ShowShopWarningDialog` — caught and fixed during verification ✅
- Player input guards updated from single-state to multi-state check ✅

## Deviations from Plan

- `ShowShopWarningDialog` was accidentally dropped during DialogUI rewrite. Fixed during verification pass.
- `BuildSettingsSetup.cs` still references old dialogue scene paths — these are in the Editor folder and won't cause runtime issues. Can be cleaned up in a future pass.
