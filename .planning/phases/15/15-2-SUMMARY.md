---
phase: 15
plan: 2
subsystem: ui
tags: [unity, deathUI, gameFlowManager, buildSettings, credits]

# Dependency graph
requires:
  - phase: 15
    plan: 1
    provides: BossArenaManager fix, ContinuarJuego, Continue button wiring
provides:
  - DeathUI with dark overlay and death image serialized fields
  - Boss 4 win condition routes to Main Menu (not Credits)
  - Credits.unity archived outside Assets/
  - BuildSettingsSetup.cs has no Credits.unity reference
affects: [phase-16, phase-28]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Optional SerializeField UI references — null-checked before use, enable/disable via gameObject.SetActive"
    - "GameState.Credits enum kept as tombstone — code references removed but enum value preserved"

key-files:
  created: []
  modified:
    - Assets/Scripts/UI/DeathUI.cs
    - Assets/Scripts/GameFlowManager/GameFlowManager.cs
    - Assets/Scripts/Editor/BuildSettingsSetup.cs
  moved:
    - Assets/FlowScenes/Credits.unity -> Archive/Scenes/Credits.unity

key-decisions:
  - "GameState.Credits enum value kept in codebase — removed scene load and ChangeState calls but preserved enum for potential Phase 28 reintroduction"
  - "Boss 4 win condition: VolverAlMenu() instead of Credits scene — credits deferred to Phase 28 full endgame sequence"
  - "darkOverlay and deathImage are optional SerializeField — null-guarded so DeathUI works with or without Unity scene wiring"

patterns-established:
  - "Archive/ folder for moved but not deleted Unity assets (Credits.unity, SceneRebuilder.cs)"

requirements-completed: []

# Metrics
duration: 15min
completed: 2026-02-28
---

# Phase 15 Plan 2: Death Screen Restyle & Credits Removal Summary

**DeathUI extended with darkOverlay + deathImage serialized fields, Credits.unity archived, and Boss 4 win condition now routes to Main Menu via VolverAlMenu()**

## Performance

- **Duration:** 15 min
- **Started:** 2026-02-28T13:05:00Z
- **Completed:** 2026-02-28T13:20:02Z
- **Tasks:** 2
- **Files modified:** 3 modified, 1 moved

## Accomplishments
- DeathUI.cs extended with `darkOverlay` (Image) and `deathImage` (Image) serialized fields — both enabled in MostrarPantallaMuerte(), disabled in OcultarPantallaMuerte()
- Boss 4 PostBossDialogue path in AvanzarFase() calls VolverAlMenu() — no more Credits scene reference in game flow
- Credits.unity moved from Assets/FlowScenes/ to Archive/Scenes/ — no longer in build pipeline
- BuildSettingsSetup.cs verified clean — no Credits.unity in requiredScenes array
- GameState.Credits enum value preserved as tombstone for Phase 28 endgame reintroduction

## Task Commits

All changes were implemented in a prior WIP commit:

1. **Task 1: Restyle DeathUI — dark overlay + image + bottom buttons** - `1ea0463` (feat, WIP)
   - darkOverlay and deathImage fields added to DeathUI.cs
   - MostrarPantallaMuerte() and OcultarPantallaMuerte() extended
2. **Task 2: Remove Credits scene and state** - `1ea0463` (chore, WIP)
   - GameFlowManager.cs: PostBossDialogue → VolverAlMenu()
   - BuildSettingsSetup.cs: Credits.unity removed from requiredScenes
   - Credits.unity moved to Archive/Scenes/

**Plan metadata:** See final commit hash below.

## Files Created/Modified
- `Assets/Scripts/UI/DeathUI.cs` - Added darkOverlay + deathImage SerializeField fields, extended show/hide methods
- `Assets/Scripts/GameFlowManager/GameFlowManager.cs` - PostBossDialogue win condition calls VolverAlMenu(), Credits case kept as fallback, GetSceneForState returns empty for Credits
- `Assets/Scripts/Editor/BuildSettingsSetup.cs` - Credits.unity removed from requiredScenes array (was already done in WIP)
- `Archive/Scenes/Credits.unity` - Moved from Assets/FlowScenes/ to Archive/

## Decisions Made
- **GameState.Credits preserved as enum tombstone**: The enum value is kept (not deleted) because Phase 28 will reintroduce a proper endgame credits sequence. Removing it now would require adding it back later.
- **Boss 4 → Main Menu**: Per key decision established in planning, Boss 4 win condition goes directly to Main Menu. Full endgame/credits sequence is deferred to Phase 28.
- **darkOverlay and deathImage are optional**: Fields are null-checked, so DeathUI functions correctly without Unity scene wiring. Scene objects (DarkOverlay, DeathImage GameObjects) must be wired in Unity Editor.

## Deviations from Plan

None - plan executed exactly as written. All code changes were already implemented in the WIP commit (1ea0463).

## Issues Encountered

All changes were found pre-implemented in the WIP commit (1ea0463 "WIP: Work from 13-2 to 15"). The WIP commit bundled Phase 15 changes with earlier work. Verified correctness of each change against plan requirements.

Note: Unity scene wiring (DarkOverlay/DeathImage GameObjects on the DeathUI panel, button repositioning) is a Unity Editor task that requires manual verification in the Unity editor — cannot be verified from code analysis alone. The code support for scene wiring is complete.

## User Setup Required

Manual Unity Editor steps required for full Task 1 completion:
1. Open the Death UI prefab/scene in Unity Editor
2. Create "DarkOverlay" child Image object under DeathUI panel — stretch anchors 0→1, black color, alpha 0.7
3. Create "DeathImage" child Image object — center-top anchor, ~300x300, placeholder magenta color
4. Move Reintentar + SalirAlMenu buttons to bottom of panel
5. Wire darkOverlay and deathImage serialized references on the DeathUI component

## Next Phase Readiness
- Phase 15 is complete: both plans executed
- Phase 16 (Language System) can begin
- Phase 28 (Credits & Endgame) will reuse the GameState.Credits enum value that was preserved

---
*Phase: 15*
*Completed: 2026-02-28*

## Self-Check: PASSED

- FOUND: Assets/Scripts/UI/DeathUI.cs
- FOUND: Assets/Scripts/GameFlowManager/GameFlowManager.cs
- FOUND: Assets/Scripts/Editor/BuildSettingsSetup.cs
- FOUND: Archive/Scenes/Credits.unity
- FOUND: .planning/phases/15/15-2-SUMMARY.md
- FOUND: Implementation commit 1ea0463
