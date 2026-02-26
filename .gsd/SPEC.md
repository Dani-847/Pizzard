# SPEC.md — Project Specification

> **Status**: `FINALIZED`

## Vision
To restructure a prototype 2D Unity action-roguelite game ("Pizzard"), creating a solid foundation based on the original concept documents. The game will feature a core loop of narrative, progression (shop), and combat encounters, starting with a polished vertical slice of the game flow and the first boss.

## Goals
1. Restructure the existing Unity project mess into a clean, maintainable architecture.
2. Implement the core gameplay loop: Intro Dialog → Shop → Pre-Boss Dialog → Boss Fight → Loop.
3. Establish a functional Main Menu (Play, Settings, Exit).
4. Implement the currency system using boss drops for the Shop.
5. Create a clean implementation of the first Boss fight using existing assets as a model.

## Non-Goals (Out of Scope)
- Developing new bosses beyond the first one (for this initial milestone).
- Mobile or console ports (assuming PC standalone initially).
- Multiplayer or network features.

## Users
- PC Gamers looking for a 2D action game with a boss-rush / roguelite progression loop.

## Constraints
- **Technical**: Must be built in Unity 2D (C#). Must use the existing assets and scripts as models but with a cleaner architecture.
- **Time/Scope**: Needs to fix the "mess" created by lack of time/planning, meaning refactoring is a priority over adding entirely new untested features.

## Success Criteria
- [ ] Game boots to a functional Main Menu.
- [ ] Player can navigate from Menu to Intro Dialog to Shop.
- [ ] Player can purchase items in the Shop using boss currency.
- [ ] Player can transition to Pre-Boss Dialog and then enter the Boss Fight.
- [ ] Defeating the Boss awards currency and correctly loops back to the Shop/Dialog phase.
- [ ] The Unity project structure is logically organized with clear separation of concerns.
