# Requirements: Pizzard v1.1

**Defined:** 2026-03-08
**Core Value:** The game feels polished and approachable — Playground lets new players experiment freely, all sprites/controls feel tight and consistent.

## v1 Requirements

### Playground Mode

- [ ] **PLAY-01**: Playground scene is accessible from the main menu via a dedicated button
- [x] **PLAY-02**: Playground has completely separate token count and progress from the main game
- [x] **PLAY-03**: Playground shop starts with 10 tokens each session
- [ ] **PLAY-04**: Playground scene includes a dummy on the right side with a live DPS counter
- [ ] **PLAY-05**: Playground scene includes a falling projectile on the left that damages the player

### Onboarding

- [ ] **ONBD-01**: On first game launch, the Playground button blinks/pulses continuously until clicked, suggesting the player try it first

### Debug UX

- [ ] **DEBG-01**: Debug button is moved from its current location into the pause menu

### Sprite Fixes

- [ ] **SPRT-01**: Animated sprites for Pblob, main character, and Niggel are replaced with v1.1 assets and animate correctly
- [ ] **SPRT-02**: All character sprites are sized consistently (coherent pixel art scale)
- [ ] **SPRT-03**: Wand sprites from v1.1 folder are added to the game

### Player Controls

- [ ] **CTRL-01**: The player's body does not rotate toward the mouse — only the wand rotates
- [ ] **CTRL-02**: The wand orbits around the player's center point (not rotating by its own pivot), following the mouse direction

## v2 Requirements

*(None defined for v1.1 — all items above are committed scope)*

## Out of Scope

| Feature | Reason |
|---------|--------|
| New spells or enemies | Gameplay changes deferred to v1.2+ |
| New boss fights | Deferred to v1.2+ |
| Main game progression changes | v1.1 is polish/UX only |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| PLAY-01 | Phase 24 | Pending |
| PLAY-02 | Phase 24 | Complete |
| PLAY-03 | Phase 24 | Complete |
| PLAY-04 | Phase 24 | Pending |
| PLAY-05 | Phase 24 | Pending |
| ONBD-01 | Phase 24 | Pending |
| DEBG-01 | Phase 25 | Pending |
| SPRT-01 | Phase 25 | Pending |
| SPRT-02 | Phase 25 | Pending |
| SPRT-03 | Phase 25 | Pending |
| CTRL-01 | Phase 26 | Pending |
| CTRL-02 | Phase 26 | Pending |

**Coverage:**
- v1 requirements: 12 total
- Mapped to phases: 12
- Unmapped: 0 ✓

---
*Requirements defined: 2026-03-08*
*Last updated: 2026-03-08 after roadmap creation (phases renumbered 24-26 to continue from v1)*
