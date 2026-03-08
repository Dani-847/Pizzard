# Pizzard v1.1

## What This Is

Pizzard is a Unity 2D game where the player casts spells with a wand. v1.1 adds a Playground/Parque mode for new players to try the game mechanics safely, fixes several visual and interaction bugs carried from v1, and introduces wand sprites with proper orbit behavior.

## Core Value

The game feels polished and approachable — new players can experiment freely in the Playground before diving into the main game, and all sprites/controls feel tight and consistent.

## Requirements

### Validated

- ✓ Core gameplay loop (player, spells, enemies, boss fights) — v1

### Active

- [ ] Playground/Parque mode accessible from main menu
- [ ] Playground has separate tokens and progress (no main game impact)
- [ ] Playground shop starts with 10 tokens
- [ ] Playground scene: dummy on right with DPS counter, falling projectile on left
- [ ] Debug button moved to pause menu
- [ ] Playground button blinks on first launch to guide new players
- [ ] Animated sprites fixed for Pblob, main character, and Niggel (v1.1 assets)
- [ ] Wand orbits around the player center (not rotating by its own pivot)
- [ ] Sprite sizes made consistent across all characters
- [ ] Wand sprites added from v1.1 folder

### Out of Scope

- Main game progression changes — v1.1 is polish/UX only
- New enemies or spells — deferred to v1.2+
- New boss fights — deferred

## Context

- Unity 2D project, existing v1 shipped
- New sprites available in `Assets/Sprites/v1.1/`
- Player currently rotates whole body toward mouse; goal is wand-only aiming that orbits the player
- Sprite inconsistency stems from pixel art assets at different scales/PPU settings

## Constraints

- **Tech stack**: Unity 2D — no engine changes
- **Assets**: Must use sprites already in `Assets/Sprites/v1.1/`
- **Scope**: No gameplay changes, only the items listed above

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Wand orbits player center | Clarified by user — wand rotates around player, not its own pivot | — Pending |
| Playground progress fully isolated | Avoids affecting main save/token state | — Pending |

---
*Last updated: 2026-03-08 after initialization*
