# ROADMAP.md

> **Current Phase**: Not started
> **Milestone**: v1.0 (Full Loop & 4 Bosses)

## Must-Haves (from SPEC)
- [ ] Clean, organized Unity project structure with an `Archive` folder.
- [ ] Functional Main Menu (Play, Settings, Exit).
- [ ] Core Gameplay Loop accurately transitioned by `GameFlowManager` (Intro -> Shop -> PreBoss -> BossFight -> Loop).
- [ ] Working Shop using boss currency to purchase wand tiers and stats.
- [ ] Wand Tier mechanics enabling 1, 2, or 3 element combinations (Queso, Pepperoni, Piña).
- [ ] Fully functional 4 Boss Fights (P'blob, Hec'kiel, Pomodoro Paganini, Niggel Worthington) with distinct mechanics.

## Phases

### Phase 1: Foundation & Refactoring
**Status**: ✅ Complete
**Objective**: Architecture setup. Move unused assets to `Archive`. Establish the `GameFlowManager` singleton and scene transition pipeline.

### Phase 2: Core UI & Main Menu
**Status**: ✅ Complete
**Objective**: Build out the Main Menu, Settings panel, Dialog UI, and wire them into the `GameFlowManager`.

### Phase 3: Player Mechanics & Wand Combinations
**Status**: ✅ Complete
**Objective**: Implement player movement, taking damage, and the core combat system including Wand Tiers 1-3 and the Queso, Pepperoni, Piña combinations logic.

### Phase 4: Progression & Shop System
**Status**: ✅ Complete
**Objective**: Implement persistent currency drops, Shop interface, and item logic. Link these to the dialog and game loop phases.

### Phase 5: Bosses Part 1 (P'blob & Hec'kiel)
**Status**: ✅ Complete
**Objective**: Implement the first two bosses: P'blob (mustache + simon-says) and Hec'kiel (elemental clone dragon), integrating them into the GameFlow.

### Phase 6: Bosses Part 2 & Polish (Oven & Niggel)
**Status**: ✅ Complete
**Objective**: Implement the remaining bosses: Pomodoro Paganini (pong deflection) and Niggel Worthington (buff/debuff thievery). Final balancing and bug fixes.

### Phase 7: Boss State Standardization
**Status**: ✅ Complete
**Objective**: Fix boss loading context and GameFlowManager edge cases.

### Phase 8: Main Menu & UI Cleanup
**Status**: ✅ Complete
**Objective**: Clean up settings and intro dialogue sequences.

### Phase 9: Boss Debugging
**Status**: ✅ Complete
**Objective**: Fix player input freezing, missing physics rigidbodies, invisible UI bars, and timescale pauses upon boss encounter.

### Phase 10: Codebase Refactor, Architecture Standardization & Polish
**Status**: ✅ Complete
**Objective**: Clean scene architectures via MCP, implement Cinemachine Camera for wide-room views, add Pblob Boss drift AI with phase knockbacks, and build the Shop Element Selection menu mechanic.

### Phase 11: Critical Loop & UI Fixes
**Status**: ✅ Complete
**Objective**: Restore full playability by addressing the broken health bar UI, stuck camera boundaries, missing Shop element selection buttons, and the broken progression loop post-boss defeat.

### Phase 12: Shop Progression & Layout Rework
**Status**: ✅ Complete
**Objective**: Reconstruct the Shop UI, progressive Wand unlocking logic, Element selection flow, and integrate Token/Fatigue UI tracking as per original design.

### Phase 13: Mana System — Combat Integration
**Status**: ✅ Complete (code) | Awaiting visual verify checkpoint
**Objective**: Wire the Mana system (renamed from Fatigue) into combat with per-spell costs, cast gating, and visual feedback. Hardcoded cost dictionary for easy balancing.
**Progress**: Plan 13.1 — 1/2 tasks complete. Wave 1 (code) done. Wave 2 (visual verify) is a human-verify checkpoint.
