# Phase 24: Playground Mode - Context

**Gathered:** 2026-03-08
**Status:** Ready for planning

<domain>
## Phase Boundary

Add a Playground scene accessible from the main menu where new players can freely experiment with spells and mechanics. Playground has a completely separate token economy (10 tokens, reset on entry), a training dummy with a live DPS counter, and a falling projectile hazard. A first-launch pulse on the Playground button guides new players to try it.

</domain>

<decisions>
## Implementation Decisions

### Token Isolation
- Playground always starts with 10 tokens on entry — no persistence, reset every session
- Token state is in-memory only — a PlaygroundManager holds the value locally, never touches SaveManager or PlayerPrefs
- Playground shop offers the same items as the main game shop (reuse ShopUI)

### Dummy & DPS Counter
- Dummy is invincible (infinite HP) — never dies, players can attack indefinitely
- DPS counter shows rolling 3-second DPS: (damage in last 3s) / 3, updated in real time
- DPS counter is a world-space canvas tag floating above the dummy — visually connected to the target
- No name label — DPS number only above the dummy

### Falling Projectile
- Falls at a fixed periodic interval (e.g., every 3 seconds)
- Spawns at a fixed position on the left side of the arena — always same spot, predictable
- Player has HP and can die; on death, player respawns at the Playground spawn point (no scene exit)

### Scene Structure & Navigation
- Playground is a new Unity scene (e.g., "PlaygroundScene"), following the BossArena_X pattern
- Loaded via SceneManager from the main menu
- Player exits via a visible "Back to Menu" button in the Playground HUD
- Playground button position on main menu: below Play/Continue, above Settings

### Onboarding Pulse (ONBD-01)
- Playground button pulses on every game launch until clicked during that session (in-memory only — NOT PlayerPrefs)
- Pulse style: scale animation (1.0 → 1.05 → 1.0 loop) using Unity Animator
- Pulse stops when the player clicks the Playground button

### Shop Access in Playground
- A "Shop" button is always visible in the Playground HUD
- Opening the shop pauses time (Time.timeScale = 0) so projectiles stop while browsing
- Same shop UI/items as main game, running against the in-memory 10-token balance

### Arena Visual Design
- Reuse existing BossArena visual assets (floor/background tiles) — no new art needed
- No "Playground" scene label — the dummy and DPS counter make the context obvious

### Claude's Discretion
- Exact projectile fall speed and damage amount
- Respawn delay after player death
- DPS counter font/size relative to existing UI scale
- Shop button placement within the Playground HUD
- Any visual distinction between main-game shop and playground shop (e.g., token counter showing "10 / 10" instead of main-game currency)

</decisions>

<specifics>
## Specific Ideas

- The pulse is in-memory only — it reappears every game launch. Not a "one-time ever" flag.
- Player should feel free to experiment: shop opens mid-arena, time pauses, no death penalty (just respawn).

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- `MenuUI.cs`: Has `botonJugar`, `botonContinuar`, `botonAjustes`, `botonSalir` buttons. Playground button follows this exact pattern — add `botonPlayground` field and wire `OnClickPlayground()` to `SceneManager.LoadScene("PlaygroundScene")`.
- `ShopController.cs` + `ShopUI.cs`: Existing shop logic uses `ProgressionManager.SpendCurrency()`. Playground needs a `PlaygroundManager` singleton with its own `int playgroundTokens` — ShopUI/ShopController will need to be swapped or overridden to call PlaygroundManager instead of ProgressionManager.
- `SaveSystem.cs`: Uses PlayerPrefs with named keys. The pulse "seen" flag stays in-memory (not PlayerPrefs) per user decision.
- `GameFlowManager.cs`: Manages `GameState` enum and scene transitions. Playground may add a `Playground` state or bypass GameFlowManager entirely with a direct `SceneManager.LoadScene()` from MenuUI.

### Established Patterns
- All scenes are loaded via `GameFlowManager.ChangeState()` or `SceneManager.LoadScene()` — Playground follows `LoadScene` directly since it doesn't fit the Shop→Boss loop.
- Time pausing pattern: check existing pause menu or boss phase for `Time.timeScale = 0` usage.
- World-space canvas tags: check if any existing health bars or name tags use world-space Canvas for DPS counter reference.

### Integration Points
- `MenuUI.cs` — add Playground button and pulse animation trigger
- New `PlaygroundManager.cs` — holds in-memory token state, initializes on scene load
- New `PlaygroundScene` Unity scene — needs player prefab, dummy prefab, projectile spawner, shop button, back button, DPS counter canvas
- `ShopUI.cs` / `ShopController.cs` — needs to support a non-ProgressionManager token source for Playground context

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 24-playground-mode*
*Context gathered: 2026-03-08*
