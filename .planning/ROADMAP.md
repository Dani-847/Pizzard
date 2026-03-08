# Roadmap: Pizzard v1.1

## Overview

v1.1 delivers three focused improvements to the shipped v1 game: a Playground mode so new players can experiment without risk, sprite and debug UX polish to eliminate visual inconsistencies from v1, and a reworked wand control system that orbits the player center. Phases execute in dependency order — Playground is self-contained, sprite fixes come next, and control rework depends on correct sprites being in place.

## Milestones

- [x] **v1.0 MVP** - Core game shipped (phases 1-23)
- [ ] **v1.1 Polish + Playground** - Phases 24-26 (in progress)

## Phases

### v1.1 Polish + Playground

- [ ] **Phase 24: Playground Mode** - Build the Playground scene and wire it from the main menu with onboarding cue
- [ ] **Phase 25: Sprite and Debug Polish** - Replace v1 sprites with v1.1 assets, normalize sizes, move debug button
- [ ] **Phase 26: Wand Control Rework** - Decouple player body rotation from mouse; implement wand orbit around player center

## Phase Details

### Phase 24: Playground Mode
**Goal**: New players can access a safe Playground scene from the main menu and try mechanics without touching their main game progress
**Depends on**: Nothing (self-contained scene)
**Requirements**: PLAY-01, PLAY-02, PLAY-03, PLAY-04, PLAY-05, ONBD-01
**Success Criteria** (what must be TRUE):
  1. A Playground button appears on the main menu and loads a separate Playground scene
  2. Entering the Playground gives the player 10 tokens in its own shop, not touching main game tokens
  3. The Playground scene contains a dummy on the right side displaying a live DPS counter
  4. The Playground scene contains a falling projectile on the left that can damage the player
  5. On first game launch the Playground button blinks/pulses until the player clicks it
**Plans**: 5 plans

Plans:
- [ ] 24-01-PLAN.md — ITokenSource interface + PlaygroundManager singleton (token economy foundation)
- [ ] 24-02-PLAN.md — MenuUI Playground button + pulse Animator (main menu entry point)
- [ ] 24-03-PLAN.md — ShopUI/ShopController ITokenSource decoupling (token isolation wiring)
- [ ] 24-04-PLAN.md — PlaygroundScene construction: dummy, DPS counter, HUD
- [ ] 24-05-PLAN.md — Falling projectile spawner + respawn handler + human verify

### Phase 25: Sprite and Debug Polish
**Goal**: All character sprites use v1.1 assets at consistent scale, and the debug button is where developers expect it
**Depends on**: Phase 24
**Requirements**: DEBG-01, SPRT-01, SPRT-02, SPRT-03
**Success Criteria** (what must be TRUE):
  1. Pblob, main character, and Niggel animate correctly using v1.1 sprite sheets
  2. All character sprites appear at a coherent pixel art scale (no character looks oversized or undersized relative to others)
  3. Wand sprites from Assets/Sprites/v1.1/ are visible in the game
  4. The debug button is accessible from the pause menu and not present in its old location
**Plans**: TBD

### Phase 26: Wand Control Rework
**Goal**: Players aim only with the wand — the character body stays forward-facing while the wand orbits the player center toward the mouse
**Depends on**: Phase 25
**Requirements**: CTRL-01, CTRL-02
**Success Criteria** (what must be TRUE):
  1. Moving the mouse left/right does not rotate the player's body sprite
  2. The wand visibly orbits around the player's center point, always pointing toward the mouse cursor
  3. Spell projectiles still fire in the correct direction after the rework
**Plans**: TBD

## Progress

**Execution Order:** 24 → 25 → 26

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 24. Playground Mode | 4/5 | In Progress|  |
| 25. Sprite and Debug Polish | 0/TBD | Not started | - |
| 26. Wand Control Rework | 0/TBD | Not started | - |
