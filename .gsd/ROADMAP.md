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
**Status**: ⬜ Not Started
**Objective**: Architecture setup. Move unused assets to `Archive`. Establish the `GameFlowManager` singleton and scene transition pipeline.

### Phase 2: Core UI & Main Menu
**Status**: ⬜ Not Started
**Objective**: Build out the Main Menu, Settings panel, Dialog UI, and wire them into the `GameFlowManager`.

### Phase 3: Player Mechanics & Wand Combinations
**Status**: ⬜ Not Started
**Objective**: Implement player movement, taking damage, and the core combat system including Wand Tiers 1-3 and the Queso, Pepperoni, Piña combinations logic.

### Phase 4: Progression & Shop System
**Status**: ⬜ Not Started
**Objective**: Implement persistent currency drops, Shop interface, and item logic. Link these to the dialog and game loop phases.

### Phase 5: Bosses Part 1 (P'blob & Hec'kiel)
**Status**: ⬜ Not Started
**Objective**: Implement the first two bosses: P'blob (mustache + simon-says) and Hec'kiel (elemental clone dragon), integrating them into the GameFlow.

### Phase 6: Bosses Part 2 & Polish (Oven & Niggel)
**Status**: ⬜ Not Started
**Objective**: Implement the remaining bosses: Pomodoro Paganini (pong deflection) and Niggel Worthington (buff/debuff thievery). Final balancing and bug fixes.
