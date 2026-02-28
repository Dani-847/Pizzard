# SPEC.md — Project Specification

> **Status**: `FINALIZED`

## Vision
To restructure a prototype 2D Unity action-roguelite game ("Pizzard"), creating a solid foundation based on the original concept documents. The game will feature a core loop of narrative, progression (shop with currency), and combat encounters featuring 4 unique bosses, wand tiers, and elemental spell combinations.

## Goals
1. Restructure the existing Unity project mess into a clean, maintainable architecture, using an overarching `GameFlowManager` state machine.
2. Implement the core gameplay loop: Intro Dialog → Shop → Pre-Boss Dialog → Boss Fight → Loop.
3. Establish a functional Main Menu (Play, Settings, Exit).
4. Implement the currency system using boss drops for the Shop to buy wand combinations/stats.
5. Implement wand mechanics: Tiers 1-3 that allow combining 1 to 3 elements (Queso, Pepperoni, Piña) into unique spells or utilities.
6. Create clean implementations of all 4 Boss fights from the design document:
   - **P'blob**: Slime boss with mustache mechanics and 3 SIMON-says type rooms.
   - **Hec'kiel**: Dragon that splits in two phases and combines elements.
   - **Pomodoro Paganini (Oven)**: Giant oven requiring PONG-like element deflection.
   - **Niggel Worthington**: Element thief boss that applies buffs with debuffs.

## Non-Goals (Out of Scope)
- Mobile or console ports (Targeting PC standalone).
- Multiplayer or network features.
- Any features not explicitly stated in the design document.

## Users
- PC Gamers looking for a 2D action game with a boss-rush / roguelite progression loop and complex spell combining mechanics.

## Constraints
- **Technical**: Built in Unity 2D (C#). Must use the existing assets and scripts as models but with a cleaner architecture. Everything is open to restructure.
- **Organization**: Any unused code or assets must be moved to an `Archive` folder, not deleted.
- **Time/Scope**: The game may be temporarily broken while laying the new architectural foundation before building the vertical slice.

## Success Criteria
- [ ] Game boots to a functional Main Menu.
- [ ] Player can navigate from Menu to Intro Dialog to Shop.
- [ ] Player can purchase items/upgrades in the Shop using boss currency.
- [ ] Player wields wand tiers to combine up to 3 elements for unique attacks.
- [ ] Player can transition to Pre-Boss Dialog and then enter all 4 Boss Fights sequentially.
- [ ] Defeating a Boss awards currency and correctly loops back to the Shop/Dialog phase.
- [ ] The Unity project structure is logically organized with clear separation of concerns, and an `Archive` folder holds legacy unused data.
