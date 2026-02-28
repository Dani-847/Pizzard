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

## Phase 13 Decisions

**Date:** 2026-02-27

### Scope
- **Rename Fatigue → Mana** across all code and UI for player intuitiveness.
- **No anti-abuse combo system** — fatigue/mana costs per spell handle balance naturally.
- **End screen (credits)** deferred to Phase 28 as planned. Annotated for future.

### Approach
- Chose: **Hardcoded dictionary** for spell costs (not ScriptableObjects)
- Reason: Easier to balance by modifying numbers directly. One central location.

### Technical Debt Resolved
- Removed `IsVulnerable()` pre-checks from **all 12 projectile/attack scripts**. `TakeDamage()` handles vulnerability internally with proper console logging.

## Phase 14 Decisions

**Date:** 2026-02-28

### Scope
- **Overlay approach** — dialogue UI lives on UIManager canvas as a persistent overlay, no separate scenes loaded for dialogue states.
- **1 dialogue per phase trigger** — easy placeholder swap later. Format: `"Dialog N (LAN)"`.
- **Click-anywhere to advance** — remove "Next" button, any screen click/tap advances dialogue.

### Approach
- Chose: **Overlay on UIManager canvas** (Option B)
- Reason: No scene load overhead, simpler flow. DialogUI stays on the DontDestroyOnLoad UIManager object.
- **Typewriter effect** on text reveal with instant-complete on click.
- **Character portraits** as colored placeholder rectangles (cyan = Bob, orange = Raberto) on both sides of centered text panel.

### Cleanup
- `IntroManager.cs` → moved to Archive (dead code, references archived DialogController).
- Dialogue scenes (`IntroDialog`, `PreBossDialog`, `PostBossDialog`) → unused after overlay switch.

### Constraints
- All dialogue text via `LocalizationManager` keys — no hardcoded strings in code.

## Phase 15 Decisions

**Date:** 2026-02-28

### Critical Fix
- **BossArenaManager** calls `AvanzarFase()` instead of `ChangeState(Shop)` — wires token reward, auto-save, post-boss dialogue, and boss index increment in one fix.

### Main Menu
- **Continue button** loads last saved game (stage + data). Only visible when `bossIndex > 1`. Not a full save-slot system — just resume from last checkpoint.

### Death Screen
- **Restyle** with dark overlay (black, alpha 0.7), centered image placeholder (upper half), and 2 buttons at bottom. Keep existing backend logic (Reintentar → Shop, Salir → Menu).

### Credits Removal
- **Credits scene removed** — development overhead not justified. Death screen is sufficient for watermarks/branding in the future. Boss 4 ending → Main Menu for now.
