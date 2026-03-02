# Phase 23: End Screen & Final Polish - Context

**Gathered:** 2026-03-02
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 23 delivers the complete v1.0 release package: end screen sequence, bug sweep, balance verification, project structure cleanup, and Windows standalone build. Plans 23-01 (Victory Screen) and 23-02 (Sprite Pass) are already written. This CONTEXT covers plan 23-03: Bug Sweep, Balance Pass, Project Structure Audit, and Build.

</domain>

<decisions>
## Implementation Decisions

### Balance Pass
- Source of truth: `GameBalance.cs` — all changes go there, never hardcoded in scripts
- Boss fight duration target: 2–5 minutes per boss
- Mana: player must NOT run out of mana in fewer than 10 seconds of sustained casting (MaxMana=100, BaseRecoveryRate=50/s — verify spell costs sustain this)
- Potion economy: 3 potions must be sufficient for Boss 1; Boss 2 should feel tight on 3 potions
- No one-shotting in either direction (player or boss)
- Verify all Tier 1 spell damage, mana costs, and status effect values (Queso, Pepperoni, Piña) match `GameBalance.Spells.*`
- Verify all Tier 2 combo spell values match `GameBalance`

### Bug Sweep
- Play full loop (Menu → Shop → Boss 1 → Shop → Boss 2 → Credits) minimum 3 times
- **Blocking bugs** (must fix before build): crashes, softlocks, progression breaks (can't advance)
- **Visual bugs** (must fix): Z-order issues, clipping, missing sprites (pink squares anywhere)
- Non-blocking cosmetic issues are acceptable for v1.0 if not jarring

### Project Structure Audit
- No stray `.cs` files in root `Assets/` (currently: `FixButtonTexts.cs`, `FixUIVisuals2.cs`, `SetupVictoryUI.cs` — these must be moved or deleted)
- All scripts must be under `Assets/Scripts/` in appropriate subfolders
- All prefabs under `Assets/Prefabs/`
- Scene tree hygiene: no duplicate GameObjects, no leftover debug objects
- **Critical rule**: No scene may have its own UIManager canvas — all HUD lives in the persistent MainMenu canvas
- **DontDestroyOnLoad audit**: 9 scripts use DontDestroyOnLoad — verify no conflicts (duplicates on scene reload)
- Editor-only scripts (`Assets/Scripts/Editor/`) are fine where they are

### Build
- Target: Windows standalone (x86_64)
- Test build by running outside Unity Editor
- Verify save file paths resolve correctly in build (not editor-relative paths)
- Verify full loop works in build: Menu → Boss 1 → Shop → Boss 2 → Credits → Menu

### Claude's Discretion
- Exact balance values to tweak (within the time/economy targets above)
- Order of scene audits
- Which stray root-level scripts to move vs delete (if they're one-time setup scripts with no runtime use, delete them)

</decisions>

<specifics>
## Specific Ideas

- Stray scripts in `Assets/Scripts/` root: `FixButtonTexts.cs`, `FixUIVisuals2.cs`, `SetupVictoryUI.cs` — likely one-time editor setup scripts; can be deleted if no longer needed
- DontDestroyOnLoad users to audit: GameFlowManager, LocalizationManager, ManaSystem, PlayerHPController, ProgressionManager, SaveManager, SoundManager, PersistentEventSystem, UIManager

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- `GameBalance.cs`: Single source of truth for all balance constants — Tier 1/2/3 spell costs, boss HP, player stats, mana recovery. Planner should only touch this file for balance changes.
- `GameFlowManager.cs`: Controls full loop progression including DontDestroyOnLoad — key file for softlock investigation

### Established Patterns
- All balance values use `GameBalance.*` static constants — no magic numbers in scripts
- DontDestroyOnLoad pattern used by 9 persistent managers — duplicates possible on scene reload if not guarded

### Integration Points
- Plan 23-03 runs after 23-01 (Victory Screen) and 23-02 (Sprite Pass) are complete
- Bug sweep validates the full integrated loop that 23-01 and 23-02 affect
- Build is final step — all other work must be complete first

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 23-end-screen-polish*
*Context gathered: 2026-03-02*
