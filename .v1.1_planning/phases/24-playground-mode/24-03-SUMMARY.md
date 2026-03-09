---
phase: 24-playground-mode
plan: "03"
subsystem: ui
tags: [shop, tokens, interface, decoupling, ITokenSource]

requires:
  - phase: 24-01
    provides: ITokenSource interface and PlaygroundManager implementing it

provides:
  - ProgressionManager implements ITokenSource (GetTokens, SpendTokens)
  - ShopUI.SetTokenSource(ITokenSource) with ProgressionManager fallback
  - ShopUI.Hide(suppressSave) guard preventing SaveManager call in playground
  - ShopController.SetTokenSource(ITokenSource) with ProgressionManager fallback

affects:
  - 24-04 (PlaygroundScene wires PlaygroundManager as token source via SetTokenSource)

tech-stack:
  added: []
  patterns: [ITokenSource dependency injection via setter, fallback-to-singleton pattern]

key-files:
  created: []
  modified:
    - Assets/Scripts/Progression/ProgressionManager.cs
    - Assets/Scripts/UI/ShopUI.cs
    - Assets/Scripts/UI/ShopController.cs

key-decisions:
  - "Fallback pattern (_tokenSource ?? ProgressionManager.Instance) ensures main-game scenes need zero changes"
  - "suppressSave defaults to false so existing Hide() callers are unaffected"
  - "ShopController gets its own SetTokenSource rather than routing through ShopUI"

patterns-established:
  - "ITokenSource setter injection: classes call SetTokenSource(source) to override default ProgressionManager singleton"
  - "Fallback-to-singleton: null-coalescing to ProgressionManager.Instance keeps backward compatibility"

requirements-completed: [PLAY-02, PLAY-03]

duration: 15min
completed: 2026-03-08
---

# Phase 24 Plan 03: Shop Decoupling via ITokenSource Summary

**ShopUI and ShopController decoupled from ProgressionManager using ITokenSource setter injection with ProgressionManager fallback, plus suppressSave guard on Hide()**

## Performance

- **Duration:** ~15 min
- **Started:** 2026-03-08T00:00:00Z
- **Completed:** 2026-03-08T00:15:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- ProgressionManager now implements ITokenSource with GetTokens() and SpendTokens() delegating to existing BossCurrency/SpendCurrency
- ShopUI accepts an optional ITokenSource via SetTokenSource(), falling back to ProgressionManager.Instance when not set
- All ShopUI purchase handlers (MaxPotion, Mana, Wand) route through the resolved ITokenSource
- ShopUI.Hide() gains a suppressSave parameter (default false) so Playground callers skip SaveManager
- ShopController mirrors the same pattern — SetTokenSource() with ProgressionManager fallback

## Task Commits

1. **Task 1: ProgressionManager implements ITokenSource** - `7b6262a` (feat)
2. **Task 2: Decouple ShopUI and ShopController via ITokenSource** - `99fafe5` (feat)

## Files Created/Modified

- `Assets/Scripts/Progression/ProgressionManager.cs` - Added ITokenSource to class declaration; GetTokens() and SpendTokens() methods
- `Assets/Scripts/UI/ShopUI.cs` - _tokenSource field, SetTokenSource(), all purchase methods updated, RefreshTokens() updated, Hide(suppressSave=false) guard
- `Assets/Scripts/UI/ShopController.cs` - _tokenSource field, SetTokenSource(), OnBuyWandUpgrade and OnBuyHealth updated

## Decisions Made

- Chose setter injection over constructor injection (Unity MonoBehaviours don't use constructors)
- Fallback pattern used in every resolution site so no existing scene needs any changes
- suppressSave defaults to false to preserve backward compatibility for all existing Hide() callers

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 04 (PlaygroundScene wiring) can now call `shopUI.SetTokenSource(PlaygroundManager.Instance)` before showing the shop
- Closing the shop in playground context uses `shopUI.Hide(suppressSave: true)` to avoid corrupting main-game saves
- Main-game shop behavior is fully preserved — no scene changes required

---
*Phase: 24-playground-mode*
*Completed: 2026-03-08*
