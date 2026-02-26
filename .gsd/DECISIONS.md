# Architecture Decision Records

## Phase 1 Decisions

**Date:** 2026-02-26

### Scope
- **Restructuring:** Everything is open to restructuring.
- **Unused Assets:** Any unused assets and scripts will be moved to an `Archive` folder rather than being deleted entirely.
- **Full Scope Target:** The primary goal is shifted to meet everything specified in the "Propuesta de proyecto integrado" PDF/TXT document.
  - Features 4 distinct bosses (P'blob, Hec'kiel, Pomodoro Paganini, Niggel Worthington).
  - Wand tier system (Tier 1-3) allowing combinations of elements (Queso, Pepperoni, Piña).
  - Currency system utilizing boss drops to purchase upgrades in the Shop.

### Approach
- **Game State Architecture:** Chose Option A.
  - We will build a global `GameFlowManager` (likely an overarching Singleton or persistent GameObject) that dictates what scene or state the game is currently in at all times.
- **Reason:** This provides a strong, centralized backbone for the complex loop of `Intro Dialog → Shop → PreBoss Dialog → Boss Fight → Loop`.

### Constraints
- The game will be temporarily broken during this initial phase while laying the new architectural foundation.
