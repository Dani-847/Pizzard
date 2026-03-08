---
phase: 15
plan: 1
subsystem: ui
tags: [unity, c-sharp, game-flow, save-system, boss-loop, continue-button]

# Dependency graph
requires:
  - phase: 14
    provides: BossArenaManager lifecycle, GameFlowManager AvanzarFase(), SaveManager with bossIndex
provides:
  - BossArenaManager wired to AvanzarFase() completing post-boss flow
  - HasSavedGame() method on GameFlowManager for save detection
  - ContinuarJuego() method on GameFlowManager for save resume
  - botonContinuar in MenuUI with conditional visibility and ContinuarJuego wiring
affects: [phase-15-2, MainMenu-scene-setup]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - BossArenaManager delegates state transitions to GameFlowManager via AvanzarFase()
    - MenuUI refreshes Continue button visibility dynamically on Show() and SetMainMenuButtonsVisible()

key-files:
  created: []
  modified:
    - Assets/Scripts/GameFlowManager/BossArenaManager.cs
    - Assets/Scripts/GameFlowManager/GameFlowManager.cs
    - Assets/Scripts/UI/MenuUI.cs

key-decisions:
  - "BossArenaManager calls AvanzarFase() not ChangeState(Shop) — delegates post-boss reward/save/dialogue logic to GameFlowManager"
  - "HasSavedGame() requires bossIndex > 1, ensuring Continue only appears after progressing past first boss"
  - "ContinuarJuego() loads save then transitions to Shop — resume at shop before next boss, not in combat"
  - "Continue button uses botonContinuar serialized field with RefreshContinueButton() for conditional visibility"

patterns-established:
  - "Pattern: Post-boss state: BossArenaManager fires AvanzarFase() which handles tokens, auto-save, dialogue overlay, and boss index increment"
  - "Pattern: Save-gated UI: MenuUI.RefreshContinueButton() checks HasSavedGame() and sets button active state"

requirements-completed: []

# Metrics
duration: 5min
completed: 2026-02-28
---

# Phase 15 Plan 1: Boss Loop Wiring & Continue Button Summary

**BossArenaManager AvanzarFase() wiring, HasSavedGame()/ContinuarJuego() in GameFlowManager, and conditional Continue button in MenuUI — completing the 4-boss progression loop**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-28T13:19:10Z
- **Completed:** 2026-02-28T13:24:00Z
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments
- BossArenaManager.HandleBossDefeated() calls GameFlowManager.Instance.AvanzarFase() — completing the full post-boss flow (token reward, auto-save, dialogue overlay, boss index increment, next shop/credits transition)
- GameFlowManager.HasSavedGame() returns true when save exists with bossIndex > 1
- GameFlowManager.ContinuarJuego() loads save data, sets currentBossIndex, and transitions to Shop state
- MenuUI has botonContinuar serialized field wired to OnClickContinuar() calling ContinuarJuego(), with RefreshContinueButton() driving conditional visibility

## Task Commits

All three tasks were already implemented in prior WIP commit 1ea0463 ("WIP: Work from 13-2 to 15"). No new code changes were needed — the plan was executed as pre-implemented work.

1. **Task 1: Fix BossArenaManager — call AvanzarFase()** - `1ea0463` (feat)
2. **Task 2: Add ContinuarJuego() and HasSavedGame() to GameFlowManager** - `1ea0463` (feat)
3. **Task 3: Add Continue button to MenuUI** - `1ea0463` (feat)

**Plan metadata:** see final docs commit

## Files Created/Modified
- `Assets/Scripts/GameFlowManager/BossArenaManager.cs` - HandleBossDefeated() calls AvanzarFase() instead of ChangeState(Shop)
- `Assets/Scripts/GameFlowManager/GameFlowManager.cs` - Added HasSavedGame() and ContinuarJuego() methods near IniciarJuego()
- `Assets/Scripts/UI/MenuUI.cs` - Added botonContinuar field, OnClickContinuar(), RefreshContinueButton() for conditional visibility

## Decisions Made
- BossArenaManager delegates all post-boss logic to AvanzarFase() — single point of control for the reward/save/dialogue chain
- HasSavedGame() gated at bossIndex > 1 so the Continue button doesn't appear for fresh saves at Shop 1
- ContinuarJuego() resumes at Shop (not Combat) — correct resume point before the next boss fight

## Deviations from Plan

None - all three tasks were already implemented in the codebase before plan execution. Code verified via grep and file inspection. No additional changes needed.

## Issues Encountered

All three tasks were found pre-implemented in commit 1ea0463 (WIP: Work from 13-2 to 15). Verification commands confirmed correct implementation:
- `grep "AvanzarFase" BossArenaManager.cs` — confirmed
- `grep "ContinuarJuego|HasSavedGame" GameFlowManager.cs` — 2 results confirmed
- `grep "botonContinuar|ContinuarJuego" MenuUI.cs` — 8 results confirmed

The Unity scene-side step (creating the ContinueButton GameObject in MainMenu scene) still requires manual Unity Editor wiring of botonContinuar to the MenuUI component — this is a scene-side connection that cannot be done via script edits alone.

## User Setup Required

**Manual Unity scene wiring needed:** The `botonContinuar` Button field in the MenuUI component must be wired in the Unity Editor:
1. Open the MainMenu scene in Unity Editor
2. Find or create a "ContinueButton" GameObject as child of the menu panel
3. Add Button + TMP_Text ("Continuar") components to it
4. Select the MenuUI GameObject and drag the ContinueButton into the `botonContinuar` field in the inspector

## Next Phase Readiness
- Boss loop wiring is complete — AvanzarFase() drives the full 4-boss progression
- Continue button code is ready; scene-side wiring needed in Unity Editor
- Plan 15.2 (Death screen restyle + Credits removal) can proceed independently

---
*Phase: 15*
*Completed: 2026-02-28*
