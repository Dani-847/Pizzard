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

## Phase 18 Decisions

**Date:** 2026-02-28

### Scope
- **Arena:** All 3 phases occur within the single `BossArena_1.unity` scene. There are no actual physical "rooms" to transition between.
- **Phase 1 (100% to 66% HP):** Boss spawns in center, idle until hit. Once hit, attack pattern begins. Boss alternates every 2 seconds between moving and standing still. Boss is ONLY vulnerable while moving.
- **Phase 2 (66% to 33% HP):** Boss goes invulnerable, moves to center. A 30s timer spawns above boss/below healthbar. 3 placeholder circles (2 red, 1 green) spawn, 2x player diameter. They randomly move for 5s, then stop until the timer ends or boss HP hits 33%. Circles are white by default, revealing true color only when stepped on. Stepping off turns them white again. Boss is ONLY vulnerable if the player is standing inside the green circle.
- **Phase 3 (33% to 0% HP):** Boss goes invulnerable, moves to top center. Player is explicitly teleported to bottom center. A grid spawns between them. A random, walkable path is generated. Grid animation: starts gray, flashes red/green randomly, then reveals the green path. Player must follow the green route. Stepping on red slows player and deals 1 DMG/sec. Once the path is completed (player reaches the boss), the boss becomes permanently vulnerable until defeated.

### Approach
- **State-Driven Architecture:** The `PblobController` will be refactored to use explicit states (e.g., `Idle`, `Phase1_Combat`, `Phase2_Minigame`, `Phase3_Puzzle`) rather than time-based automatic pattern cycling.
- **GameBalance Integration:** All timings (2s alternate, 5s movement, 30s timer), speeds, sizes (circle size), and damages must be strictly driven from `GameBalance.cs` to allow easy balance adjustments.

### Constraints & Edge Cases
- **Death Loop:** If the player dies during the boss fight, they get 2 options: Return to the last Shop (preserving original incoming tokens) or Return to Main Menu. (Ties into Phase 15 implementation).

## Phase 22 Decisions

**Date:** 2026-03-02

### Scope (dramatically reduced from ROADMAP)
- **NO corruption validation** — not needed for v1.0.
- **NO version field / migration** — not needed for v1.0.
- **NO manual save in pause menu** — not needed.
- **NO "Continue" from main menu** — leaving the game = fresh start from 0.
- **Pause → Main Menu = full restart** — same as closing the game.

### What save DOES need to persist
- **On death/retry ONLY**: element selection and unspent tokens carry over between retries.
- **Everything else resets** on quit or return to main menu.

### Approach
- Chose: **Option A — Minimal hardening**
- Reason: Just need a functional v1.0. No save complexity needed.

### Dependencies
- Phase 19 (Niggel) is fully complete, just needs ROADMAP marked.

## Phase 23 Decisions

**Date:** 2026-03-02

### Scope (dramatically reduced from ROADMAP)
- **YES — End Screen**: Simple "You Win" screen after defeating Niggel. Space for a sprite, congratulations text, 2 buttons (Main Menu + Exit Game).
- **YES — Sprite Pass**: Final plan (23-02) to replace placeholder sprites. User will provide sprites.
- **NO credits** — skip entirely.
- **NO balance pass** — user handles via GameBalance.cs directly.
- **NO build** — user knows how, handles it themselves.
- **NO code/scene audits** — not needed for v1.0.
- **NO bug sweep** — not needed right now.

### Plan Structure
- 23-01: End screen implementation (trigger after Niggel defeat, UI, buttons)
- 23-02: Sprite replacement pass (final, user provides assets)


