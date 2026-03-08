---
phase: 16-language-system-completion
plan: 03
subsystem: ui
tags: [localization, unity-editor, i18n, scene-wiring]

requires:
  - phase: 16-language-system-completion-01
    provides: en.json and es.json with all localization keys
  - phase: 16-language-system-completion-02
    provides: dynamic code wiring for Shop, ElementSelection, BossHealthBar, DialogUI

provides:
  - WireLocalization Unity Editor script for attaching LocalizedText to static UI text objects
  - All main menu, options, combinations, and death screen buttons wired via Tools/Wire Localization

affects: [localization, ui, scene-wiring]

tech-stack:
  added: []
  patterns: [Unity Editor script for batch LocalizedText component wiring]

key-files:
  created:
    - Assets/Editor/WireLocalization.cs
  modified: []

key-decisions:
  - "Unity MCP tools unavailable — used an Editor script (Tools/Wire Localization) as the mechanism for attaching LocalizedText components; user runs it once from the Unity Editor"
  - "WireLocalization script uses GameObject.Find() with known hierarchy paths and adds LocalizedText with correct key via SerializedObject"

patterns-established:
  - "Batch scene wiring pattern: Editor script iterates known GameObject paths, adds LocalizedText component, sets key field via SerializedProperty"

requirements-completed: []

duration: ~10min
completed: 2026-02-28
---

# Phase 16, Plan 03: Scene Wiring and End-to-End Language Switch Verification

**Unity Editor script `WireLocalization.cs` provides one-click attachment of LocalizedText components to all static UI text objects in the main menu, options, combinations, and death screen**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-02-28
- **Completed:** 2026-02-28 (Task 1 complete; Task 2 awaiting human verification)
- **Tasks:** 1/2 complete (Task 2 is human-verify checkpoint)
- **Files modified:** 1

## Accomplishments

- Created `Assets/Editor/WireLocalization.cs` — Unity Editor window with menu item `Tools/Wire Localization`
- Script wires LocalizedText components to: 4 main menu buttons, 4 options labels/buttons, 1 combinations back button, 3 death screen elements
- Script also sets `bossLocalizationKey` on BossHealthBarUI for all 4 bosses (Hec'kiel, Pomodoro, Niggel, P'blob)
- Corrected scene path from `Assets/Scenes/MainScene.unity` to `Assets/FlowScenes/MainMenu.unity` (Rule 1 auto-fix)

## Task Commits

1. **Task 1: Add editor script for localization wiring** - `4e10c1b` (feat)
2. **Task 1: Fix scene path** - `709de09` (fix)

## Files Created/Modified

- `Assets/Editor/WireLocalization.cs` — Editor window script; opens MainMenu.unity, finds text GameObjects by hierarchy path, adds LocalizedText components with correct keys, saves scene

## Decisions Made

- Unity MCP tools were not available in this execution environment. Used an Editor script approach instead — the user runs `Tools/Wire Localization` from the Unity Editor menu once to apply all wiring atomically.
- Scene path corrected from `Assets/Scenes/MainScene.unity` to `Assets/FlowScenes/MainMenu.unity` to match actual project structure.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Corrected scene path in WireLocalization script**
- **Found during:** Task 1 (reviewing script after initial commit)
- **Issue:** Initial script used wrong path `Assets/Scenes/MainScene.unity` — actual scene is at `Assets/FlowScenes/MainMenu.unity`
- **Fix:** Updated path in `EditorSceneManager.OpenScene()` call
- **Files modified:** Assets/Editor/WireLocalization.cs
- **Verification:** Path matches actual project structure
- **Committed in:** `709de09` (fix commit)

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Necessary correctness fix — wrong path would cause the editor script to fail silently.

## Issues Encountered

Unity MCP tools (find_gameobjects, manage_components) were not available in this execution environment. The Editor script approach achieves the same result — the user runs it once from the Unity Editor to apply all wiring.

## User Setup Required

**Run the wiring script from Unity Editor:**

1. Open Unity Editor with the project
2. From the menu bar: **Tools → Wire Localization**
3. The script will open `Assets/FlowScenes/MainMenu.unity`, attach `LocalizedText` components to all static UI text objects, set boss localization keys, and save the scene
4. Check the Unity Console for `Wired: [path] -> [key]` confirmation messages and any warnings about not-found GameObjects
5. After running, proceed to Play mode verification (Task 2)

## Next Phase Readiness

- Task 1 complete: WireLocalization.cs committed and ready to run
- Task 2 (human-verify checkpoint): Human must run `Tools/Wire Localization` in Unity Editor and verify end-to-end language switching in Play mode
- Once verified, Phase 16 language system completion is done

---
*Phase: 16-language-system-completion*
*Completed: 2026-02-28*
