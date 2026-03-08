# Roadmap: Pizzard v1.1

## Overview

v1.1 added a Playground mode so new players can experiment without risk. Visual fixes (sprites, UI, projectiles) are complete. Sprite/debug polish and wand control rework have been moved to PIZZARD.md as deferred items.

## Milestones

- [x] **v1.0 MVP** - Core game shipped (phases 1-23)
- [x] **v1.1 Polish + Playground** - Phase 24 (completed 2026-03-08)

## Phases

### v1.1 Polish + Playground

- [x] **Phase 24: Playground Mode** - Build the Playground scene and wire it from the main menu with onboarding cue (completed 2026-03-08)

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
- [x] 24-01-PLAN.md — ITokenSource interface + PlaygroundManager singleton (token economy foundation)
- [x] 24-02-PLAN.md — MenuUI Playground button + pulse Animator (main menu entry point)
- [x] 24-03-PLAN.md — ShopUI/ShopController ITokenSource decoupling (token isolation wiring)
- [x] 24-04-PLAN.md — PlaygroundScene construction: dummy, DPS counter, HUD
- [x] 24-05-PLAN.md — Falling projectile spawner + respawn handler + human verify

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 24. Playground Mode | 5/5 | Complete | 2026-03-08 |

## Deferred to PIZZARD.md

The following items were originally planned as phases 25 and 26 but have been moved to `PIZZARD.md` under v1.1 Planned items:
- Sprite and Debug Polish (v1.1 sprite assets, size normalization, debug button relocation)
- Wand Control Rework (orbit around player center, decouple body rotation)
