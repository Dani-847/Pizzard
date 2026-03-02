# Pizzard — Official Project Documentation

> A 2D top-down spell-casting game built in Unity where a pizza wizard defeats bosses using food-element combo spells.
> **Engine:** Unity 2022.3.62f1 LTS · **Platform:** Windows Standalone · **Version:** v1.0 — Complete

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Development Methodology](#2-development-methodology)
3. [v1.0 Completed Phases](#3-v10-completed-phases)
4. [v2.0 — Future Roadmap](#4-v20--future-roadmap)
5. [Technology Index](#5-technology-index)

---

## 1. Project Overview

**Pizzard** is a 2D top-down action game in which the player controls a pizza wizard who casts spells by chaining food elements — **Queso**, **Pepperoni**, and **Piña** — into combinations. The spell system produces 27 unique attacks across three tiers, ranging from a single cheese shield to a black hole that absorbs and returns enemy projectiles.

The v1.0 release delivers a complete loop across two boss fights, a shop/progression system, bilingual UI (EN/ES), a hardened save system, and a victory end screen.

### Core Game Loop

```
Main Menu → Intro Dialogue → Shop 1 → Boss 1 (P'blob) → Post-Boss Dialogue
         → Shop 2 → Boss 2 (Niggel Worthington) → Final Dialogue → Victory Screen → Credits → Main Menu
```

### Game Scenes

| Scene | Purpose |
|-------|---------|
| `MainMenu.unity` | Main menu, options/settings, intro dialogue |
| `Shop.unity` | Between-boss upgrade shop — wands, potions, mana |
| `BossArena_1.unity` | P'blob fight — three-phase encounter |
| `BossArena_2.unity` | Niggel Worthington fight — enrage/steal mechanic |
| `BossArena_3.unity` | Reserved — Boss 3 (v2.0) |
| `BossArena_4.unity` | Reserved — Boss 4 (v2.0) |

### Key Systems at a Glance

| System | Description |
|--------|-------------|
| Spell Dispatch | Element queue → 27 spells across T1/T2/T3 via `PlayerAimAndCast` |
| GameBalance | Single-source constants file; no magic numbers elsewhere in the codebase |
| Game Flow | `GameFlowManager` singleton drives all scene transitions and loop state |
| Save System | JSON-based save with versioning, corruption recovery, and auto-save |
| Localization | Real-time EN ↔ ES toggle — every string is keyed, none are hardcoded |
| Dialogue | Overlay `DialogUI` system for all narrative sequences |
| Persistent HUD | `UIManager` canvas survives scene loads; no scene owns its own HUD |

---

## 2. Development Methodology

Every phase in Pizzard followed a strict four-step process to guarantee quality and traceability from idea to shipped feature.

### The Phase Workflow

```
Discuss → Plan → Execute → Verify
```

**1. Discuss**
Before any code was written, each phase began with a structured discussion session to extract implementation decisions. Gray areas were identified — layout choices, behavior tradeoffs, edge cases, balance targets — and resolved explicitly. The output was a `CONTEXT.md` file capturing locked decisions so that downstream work never had to guess intent.

**2. Plan**
With decisions locked, one or more `PLAN.md` files were produced per phase. Each plan defined exact files to be modified, atomic task lists with must-have truths (verifiable facts about the final state), and dependency links between tasks. Plans were reviewed and verified for completeness before execution began.

**3. Execute**
Plans were executed task by task, with atomic commits after each meaningful unit of work. Deviations from the plan — unexpected state, compilation errors, runtime bugs — were handled through documented gap resolution rather than improvisation. No code was written without a corresponding plan.

**4. Verify**
After execution, a verification pass confirmed every must-have truth from the plan. This included running the game, checking Unity console for errors, validating UI at multiple resolutions, and playing through the affected loop segment. Phases were not considered complete until verification passed.

This methodology was applied uniformly to all 23 phases of v1.0.

---

## 3. v1.0 Completed Phases

All 23 phases of v1.0 are complete. Each phase was discussed, planned, executed, and verified before the next began.

---

### Phase 1 — Foundation & Refactoring
**Delivered:** Project structure, core singleton, scene pipeline.

Established the foundational architecture the entire project builds on. The `GameFlowManager` singleton was introduced as the authoritative controller for scene transitions and game loop state. An archive folder was created for legacy scripts. Scene loading order was defined and locked.

- `GameFlowManager.cs` — persistent singleton, `DontDestroyOnLoad`
- Scene pipeline: MainMenu → Shop → BossArena → (loop)
- Archive structure for deprecated code

---

### Phase 2 — Core UI & Main Menu
**Delivered:** Functional main menu, settings screen, and dialog UI wired to game flow.

Built the first player-facing screens. Menu buttons connected to `GameFlowManager` transitions. The Settings screen was scaffolded. A `DialogUI` overlay was wired up for narrative sequences. All canvases configured with the persistent HUD pattern established here.

- `MainMenu.unity` fully functional
- Settings screen with volume control
- `DialogUI` first version

---

### Phase 3 — Player Mechanics & Wand Combinations
**Delivered:** Full player controller, dash, and the spell dispatch foundation for all 27 combos.

The player movement system was built with dash, invulnerability frames, and Rigidbody2D physics. The wand combination system was designed and implemented: an element queue reads up to three inputs, hashes the combination key, and dispatches the matching spell prefab. Tier 1, 2, and 3 dispatch stubs were established.

- `PlayerController.cs` — movement, dash, invulnerability
- `PlayerAimAndCast.cs` — element queue, combo key hashing, prefab dispatch
- Tier 1/2/3 spell prefab architecture defined

---

### Phase 4 — Progression & Shop System
**Delivered:** Currency economy, shop interface, and item purchase logic.

Introduced the token economy that gates progression between bosses. The Shop scene was built with UI panels for wand upgrades, potion purchases, and mana upgrades. Purchase logic validates token count and updates persistent player state. Item availability and pricing wired to `GameBalance`.

- `Shop.unity` — fully functional shop scene
- Token drop on boss defeat
- Wand tier unlocking (T1 → T2 → T3 via shop)
- Potion inventory and mana upgrade purchasing

---

### Phase 5 — Bosses Part 1 (Initial Stubs)
**Delivered:** P'blob and Hec'kiel boss scripts — state machines, health systems, and phase skeletons.

P'blob and Hec'kiel were implemented as playable boss stubs. Each boss received a state machine, a health bar wired to `UIManager`, and placeholder attack patterns. The shared `BossController` base class was established here, defining the interface all bosses inherit.

- `BossController.cs` base class — health, phases, death trigger
- `PblobController.cs` — three-phase skeleton
- `HeckielController.cs` — elemental dragon skeleton

---

### Phase 6 — Bosses Part 2 (Initial Stubs)
**Delivered:** Pomodoro Paganini and Niggel Worthington boss stubs.

The remaining two bosses were scaffolded. Pomodoro's pong deflection mechanic and Niggel's steal/buff system were stubbed with placeholder logic. Both bosses received health systems, scene assignments, and connections to the progression loop via `GameFlowManager`.

- `PomodoroController.cs` — pong mechanic stub
- `NiggelController.cs` — steal mechanic stub, momentum system placeholder

---

### Phase 7 — Boss State Standardization
**Delivered:** Consistent boss loading context and `GameFlowManager` edge case handling.

Discovered and fixed a class of bugs caused by inconsistent boss initialization — bosses loaded before the player was ready, or `GameFlowManager` state was incorrect on scene entry. All boss scenes were normalized to a common initialization contract. Edge cases in `AvanzarFase` (loop advancement) were resolved.

- Boss initialization contract standardized across all 4 controllers
- `GameFlowManager.AvanzarFase` — edge cases resolved
- `currentBossIndex` tracking hardened

---

### Phase 8 — Main Menu & UI Cleanup
**Delivered:** Polished main menu, settings wiring, and intro dialogue sequences.

The main menu received its final layout and visual pass for v1.0. Settings buttons (volume, language placeholder, keybindings) were fully wired. The intro dialogue sequence — the first narrative moment the player sees — was authored and connected to `GameFlowManager` to trigger on new game start.

- Main menu final layout
- Settings screen fully wired
- Intro dialogue sequence authored and triggered

---

### Phase 9 — Boss Debugging
**Delivered:** Resolution of input freeze, Rigidbody conflicts, UI bar desyncs, and timescale bugs.

A targeted debugging pass across all boss encounters. Player input was freezing after boss phase transitions due to a timescale not being restored. Rigidbody constraints were conflicting with boss movement code. Health bar UI was desyncing from actual HP values. All blocking bugs were identified, root-caused, and fixed.

- Input freeze on phase transition — fixed
- Rigidbody constraint conflicts — resolved
- Health bar UI sync — corrected
- Timescale reset after pause/death — hardened

---

### Phase 10 — Codebase Refactor & Polish
**Delivered:** Cinemachine camera, P'blob AI improvements, and shop element selection polish.

Cinemachine was introduced for camera follow with configurable dead zones. P'blob's wander AI was rewritten for smoother movement in Phase 1. The element selection UI in the shop received layout and interaction polish — tier indicators, hover states, and selection feedback.

- Cinemachine Virtual Camera on player
- P'blob wander AI rewrite
- Element selection UI polish

---

### Phase 11 — Critical Loop & UI Fixes
**Delivered:** Health bar, camera, shop buttons, and full progression loop verified end-to-end.

The first successful full-loop playthrough (Menu → Shop → Boss 1 → Shop → Boss 2) was achieved and verified in this phase. Remaining blocking issues were resolved: health heart rendering, camera follow edge cases, shop button interaction bugs, and death loop returning to the correct checkpoint.

- Full loop verified end-to-end for the first time
- Health heart sprites rendering correctly
- Death → continue returning to correct boss

---

### Phase 12 — Shop Progression & Layout Rework
**Delivered:** Shop UI overhaul, wand unlock flow, and token/mana tracking.

The shop was reworked to handle progression state correctly across multiple visits. Wand unlocking (T2 after Boss 1, T3 after Boss 2) was implemented with proper guards. Token and mana counts displayed correctly across scenes. Layout was rebuilt to be resolution-independent.

- Wand unlock gating by `bossIndex`
- Token display and spend confirmation
- Mana upgrade persistent across scenes

---

### Phase 13.1 — Mana System
**Delivered:** Fatigue system renamed to Mana; per-spell cost dictionary; cast gating.

The original "Fatigue" cooldown system was replaced with a true mana pool. Each spell now has a specific cost defined by key in `GameBalance.Mana.SpellCosts`. Casting is gated — if mana is insufficient, the cast is rejected with UI feedback. Mana recovers at `BaseRecoveryRate` per second with a delay after casting.

- `ManaSystem.cs` — pool, recovery, cost gating
- Per-spell mana cost dictionary in `GameBalance`
- Mana bar UI wired to `UIManager`

---

### Phase 13.2 — GameBalance Centralization
**Delivered:** 123+ numeric constants extracted from 36 scripts into `GameBalance.cs`.

A structural refactor that eliminated all magic numbers from the codebase. Every balance-relevant constant — boss HP, player stats, spell costs, mana values, shop prices, timing — was moved into the static `GameBalance` class. From this phase forward, no numeric constants are written directly in scripts.

- `GameBalance.cs` — single source of truth for all constants
- 36 files updated to reference `GameBalance.*`
- Established the rule: all balance changes go through `GameBalance`, never inline

---

### Phase 14 — Dialogue System & Narrative Flow
**Delivered:** `DialogUI` overlay system for all narrative sequences across the full loop.

A complete dialogue system was built and all narrative sequences authored. The overlay supports sequential text panels with speaker names, triggered by `GameFlowManager` at defined loop points: intro, pre-boss, post-boss, death-shop, and final. Dialogue keys are localization-ready.

- `DialogUI.cs` — overlay controller, panel queue, auto-advance
- Intro, pre-Boss 1, post-Boss 1, pre-Boss 2, post-Boss 2, death-shop dialogues
- Final dialogue with Raberto after defeating Niggel

---

### Phase 15 — Boss Loop Completion
**Delivered:** Token economy finalized, game-over flow complete, full loop verified.

The progression economy was finalized: starting token count, boss kill rewards, shop spend tracking, and carry-over between visits. The game-over screen was built and connected — death shows the screen, "Continue" returns to the last save point. The full loop was verified three times from Menu to Credits.

- Token economy table finalized
- Game over screen — "Continue" returns to checkpoint
- Full loop played and verified 3× end-to-end

---

### Phase 16 — Language System Completion
**Delivered:** Full EN ↔ ES localization — every UI string keyed, real-time toggle in Options.

All hardcoded strings across the game were replaced with localization keys resolved at runtime. `LocalizationManager` loads the active language's JSON on startup and fires an event on change. `LocalizedText` components on all Text/TMP objects refresh instantly on language switch. Language preference is persisted in save data.

- `LocalizationManager.cs` — JSON loader, event broadcast
- `LocalizedText.cs` — component on every text element
- `en.json` and `es.json` — complete string tables
- Language toggle in Options → real-time update, no reload

---

### Phase 17 — UI Polish & Resolution Independence
**Delivered:** All canvases on Scale With Screen Size; consistent visual style across four tested resolutions.

Every Canvas in the project was configured with Scale With Screen Size at 1920×1080 reference, Match 0.5. UI was tested and corrected at 1920×1080, 1366×768, 2560×1440, and 1280×720. Shop buttons received uniform sizing and hover/press feedback. The mana bar was converted to a vertical fill with color gradient. Boss health bars were normalized across all boss scenes.

- Canvas scalers — all scenes
- Tested at 4 resolutions — no clipping or off-screen elements
- Mana bar — vertical fill, color gradient
- Boss health bars — consistent across all arenas

---

### Phase 18 — Boss 1: P'blob AI & Patterns
**Delivered:** Full three-phase P'blob fight — wander AI, circle minigame, and grid puzzle.

P'blob was fully implemented as a proper boss encounter across three distinct phases:

- **Phase 1 (100%–66% HP):** P'blob alternates between moving (vulnerable, 2s) and standing still shooting (invulnerable, 2s). Wander AI pursues the player.
- **Phase 2 (66%–33% HP):** Boss moves to center and becomes invulnerable. Three circles (2 Red, 1 Green) spawn and wander for 5s, then freeze and hide colors. Player must stand in the Green circle to deal damage. 30-second timer — failure deals damage.
- **Phase 3 (<33% HP):** Grid spawns between boss and player. Tiles flash Gray → Red/Green, revealing a safe path. Player must follow the green path (red tiles damage and slow). Reaching the boss makes it permanently vulnerable.

All timing, HP thresholds, and damage values sourced from `GameBalance.Bosses.Pblob`.

---

### Phase 19 — Boss 2: Niggel Worthington AI
**Delivered:** Full Niggel Worthington fight — coin-steal mechanic, enrage scaling, and minion spawning.

Niggel Worthington ("The Rich Guy") is a mobile boss built around a role-reversal mechanic: he steals the player's coins and gets stronger with each successful theft.

- **HP System:** `CoinVault` — 200 coins. Three enrage thresholds at 150, 100, and 50 coins remaining.
- **Attacks:**
  - *Throw Money* — coin bag projectiles in spread patterns
  - *Rich Dash* — high-speed arena dash, speed scales with steal count (`speedMultiplier`)
  - *Steal Stats* — on close contact, steals player currency or HP; each steal grants Niggel +0.2× speed
- **Minions:** Toy soldiers spawn at intervals to body-block player spells
- **Resolution:** Defeating Niggel restores all stolen stats and currency
- **HUD:** `NiggelCoinMeterUI` shows the boss's CoinVault draining in real time

All values sourced from `GameBalance.Bosses.Niggel`.

---

### Phase 20 — Spell Polish: Tier 1
**Delivered:** Visual and mechanical polish for all three Tier 1 spells.

The three single-element spells received a full polish pass — particle effects on spawn, travel, and impact; mana cost verification against `GameBalance`; and visual feedback on target.

| Spell | Element | Mechanic | Polish |
|-------|---------|----------|--------|
| Cheese Shield | Queso | Orbiting shield, reflects projectiles, contact damage | Particle trail, orbit VFX |
| Fire Bolt | Pepperoni | Fast projectile, burn DoT on impact | Fire VFX, burn indicator on target |
| Pineapple Dart | Piña | Fast straight projectile | Unique travel VFX, impact burst |

---

### Phase 21 — Spell Completion: All T2 + T3 Combos
**Delivered:** All 16 missing combo spells implemented; all 9 existing T3 combos verified.

The full spell system was completed. Every combination of the three elements across Tiers 2 and 3 was implemented and verified. All values come from `GameBalance.Spells.*` — no inline constants.

**Tier 2 — Implemented:**

| Combination | Mechanic |
|-------------|---------|
| `queso\|queso` | Reflective wall on ground — has HP, reflects projectiles |
| `queso\|pepperoni` | Ground area — applies burn on contact, damage scales with exposure time |
| `queso\|piña` | Damage pillar — ticking area damage |
| `pepperoni\|queso` | Sticky projectile — burn + bonus damage the longer it adheres (2s) |

**Tier 2 — Verified:**

| Combination | Mechanic |
|-------------|---------|
| `piña\|piña` | Splitter — explodes into sub-projectiles on impact |
| `piña\|queso` | Absorbing projectile — grows on contact, explodes |
| `piña\|pepperoni` | Teleport — player teleports to impact point + explosion |
| `pepperoni\|piña` | Catapult — arcing projectile, burn on impact |
| `pepperoni\|pepperoni` | Fire trail — leaves persistent fire trail, burn on impact |

**Tier 3 — Implemented (16 spells):**

| Combination | Mechanic |
|-------------|---------|
| `piña\|piña\|piña` | Splitter chain — sub-projectiles also explode |
| `piña\|piña\|queso` | Splitter — sub-projectiles absorb incoming shots |
| `piña\|piña\|pepperoni` | Splitter — sub-projectiles apply burn |
| `piña\|queso\|piña` | Absorbing — damage = absorbed count |
| `piña\|queso\|queso` | Absorbing — releases as a cone |
| `piña\|queso\|pepperoni` | Absorbing — burn stacks = absorbed count |
| `piña\|pepperoni\|piña` | Dual projectiles → teleport → explosion |
| `piña\|pepperoni\|pepperoni` | Same, larger explosion |
| `piña\|pepperoni\|queso` | Same, explosion reflects projectiles |
| `queso\|queso\|queso` | Black hole — absorbs projectiles, returns at ×1.5 damage |
| `queso\|piña\|piña` | Pillar — bigger area, increased damage |
| `queso\|piña\|queso` | Pillar — extra HP, slows projectiles passing through |
| `queso\|piña\|pepperoni` | Pillar — burn stacks per tick, bonus damage per stack |
| `queso\|pepperoni\|pepperoni` | Ground area — double burn stacks |
| `queso\|pepperoni\|piña` | Ground area — burn + persistent DoT |
| `queso\|pepperoni\|queso` | Ground area — increased radius |

**Tier 3 — Verified (existing, 11 combos):** All `pepperoni`-lead and `queso\|queso\|*` variants confirmed functional against `GameBalance` values.

---

### Phase 22 — Save System Polish
**Delivered:** Hardened JSON save with versioning, corruption recovery, auto-save, and correct New Game / Continue behavior.

The save system was made production-ready. The core improvements:

- **Versioning:** `SaveData` includes a `version` field for forward-compatible migration
- **Corruption recovery:** `SaveManager` validates JSON on load — a corrupted or missing file presents a recovery prompt ("Start New Game?") rather than crashing
- **Auto-save:** Triggers automatically after each boss defeat and after closing the shop
- **bossIndex clamping:** Out-of-range values on load are clamped to the valid range
- **New Game:** Fully resets all persistent state — HP, mana upgrades, tokens, bossIndex
- **Continue:** Restores exact loop position — scene, boss state, inventory

---

### Phase 23 — End Screen & Final Polish
**Delivered:** Victory sequence, final sprite pass, balance verification, bug sweep, and Windows standalone build.

The final phase brought all v1.0 systems to release quality.

**End Screen Sequence:**
1. Niggel defeated → final dialogue with Raberto plays via `DialogUI`
2. Victory screen: "You did it!" with pizza reward visual (`VictoryUI.cs`)
3. Scrolling credits — team names, TMP auto-scroll
4. "Return to Menu" button — calls `VolverAlMenu` (full state reset)
5. Save file updated: `bossIndex = 2`, game marked complete

**Sprite Pass:**
All placeholder sprites replaced with final artwork. Player, bosses (P'blob, Niggel), all projectile prefabs, UI elements, and backgrounds verified. No missing sprite references (pink squares) in the full loop.

**Balance Pass** (all via `GameBalance.cs`):
- P'blob fight: 2–5 minutes at target skill level ✓
- Niggel fight: 2–5 minutes at target skill level ✓
- Mana: sustained casting does not exhaust pool in under 10 seconds ✓
- Potion economy: 3 potions sufficient for Boss 1, tight for Boss 2 ✓
- No one-shotting in either direction ✓

**Bug Sweep:** Full loop played 3× — all blocking bugs (crashes, softlocks, progression breaks) and visual bugs (Z-order, clipping, missing sprites) resolved.

**Project Structure Audit:** All scripts in correct subfolders under `Assets/Scripts/`. No stray files in `Assets/` root. All scenes verified — no scene owns its own UIManager canvas. No `DontDestroyOnLoad` conflicts on scene reload.

**Build:** Windows standalone (x86_64) created, tested outside the editor. Save file paths confirmed correct in build. Full loop verified in the build binary.

---

### Phase Dependency Graph

```
1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9 → 10 → 11 → 12 → 13.1 → 13.2 → 14 → 15
                                                                              │
                                                              ┌───────────────┘
                                                              │
                                                             16 → 17 → 18 → 19 → 20 → 21
                                                                                │
                                                                               22
                                                                                │
                                                                               23 ← release
```

---

## 4. v2.0 — Future Roadmap

v2.0 extends Pizzard to a four-boss full run with audio, a tutorial, New Game+, and controller support. All items below are explicitly out of scope for v1.0 and planned for a future update.

**Target loop for v2.0:**
```
Menu → Dialogue → Shop → Boss 1 → Shop → Boss 2 → Shop → Boss 3 → Shop → Boss 4 → Credits → New Game+
```

| Phase | Name | Description |
|-------|------|-------------|
| v2-01 | Boss 3 — Pomodoro Paganini | Pong mechanic: the player must deflect Pomodoro's projectiles back using a wand-controlled paddle. AoE stage hazards. Uses `BossArena_3.unity`. |
| v2-02 | Boss 4 — Hec'kiel AI & Patterns | Elemental dragon with a mid-fight phase split into two independent heads, each requiring a different spell element to damage. Uses `BossArena_4.unity`. |
| v2-03 | Tutorial System | Interactive first-playthrough tutorial via a `TutorialManager` singleton — teaches movement, casting, and the combo system in a controlled arena before the first boss. |
| v2-04 | Audio & SFX | Full audio pass — background music tracks per scene, spell cast and impact SFX for all 27 combos, UI interaction sounds, boss phase transition stings. |
| v2-05 | Full Endgame & Localized Credits | Post-Boss 4 endgame cutscene, full localized credits scroll (EN/ES), New Game+ flag in save data. |
| v2-06 | Controller Support | Full Xbox/gamepad mapping for all inputs. Input icon swapper — UI glyphs switch between keyboard and gamepad icons based on last active input device. |

---

## 5. Technology Index

### Engine & Runtime

| Technology | Version | Role |
|-----------|---------|------|
| Unity | 2022.3.62f1 (LTS) | Game engine — rendering, physics, asset pipeline, build system |
| C# / .NET | .NET Standard 2.1 | Scripting language — all game logic |
| Unity 2D Feature Set | 2.0.1 | 2D sprite rendering, Physics 2D, Tilemap, 2D animation |

### Unity Packages

| Package | Version | Role |
|---------|---------|------|
| TextMesh Pro | 3.0.7 | All in-game text, UI labels, dialogue panels, and credits |
| Input System | 1.14.2 | Keyboard and mouse input — movement, aiming, casting, UI navigation |
| Unity UI (uGUI) | 1.0.0 | Canvas-based UI system — buttons, sliders, health hearts, mana bar |
| Tilemap | module | Boss arena floor layouts and wall geometry |
| Particle System | module | Spell VFX — spawn, travel, and impact effects for all 27 spells |
| Physics 2D | module | Rigidbody2D movement, trigger colliders, projectile collision |
| Animation | module | Character and boss sprite animation state machines |
| JSON Serialize | module | Save system serialization via `JsonUtility` |
| Timeline | 1.7.7 | Included — reserved for v2.0 cutscene sequences |
| Test Framework | 1.1.33 | Edit mode and Play mode unit tests |
| Visual Scripting | 1.9.4 | Included — not used in production logic |
| Unity MCP | (main) | AI-assisted editor tooling used during development workflow |

### IDE & Version Control

| Tool | Role |
|------|------|
| Rider (JetBrains) | Primary IDE — C# editing, debugging, refactoring |
| Visual Studio 2022 | Secondary IDE |
| Git | Version control — all source history |
| Plastic SCM | Unity-native VCS integration (`.plastic/` config) |

### Custom Systems — Key Scripts

| System | Primary Script(s) | Description |
|--------|-------------------|-------------|
| Game Flow | `GameFlowManager.cs` | Persistent singleton. Drives all scene transitions, tracks `currentBossIndex`, and controls the full game loop from menu to credits. |
| Balance | `GameBalance.cs` | Static constants class. Single source of truth for all numeric values — HP, damage, mana costs, timing, speed, economy. No magic numbers exist elsewhere. |
| Spell Dispatch | `PlayerAimAndCast.cs` | Reads the player's element queue, constructs a combination key (`"queso\|pepperoni"`), and instantiates the corresponding spell prefab. |
| Mana | `ManaSystem.cs` | Mana pool with per-second recovery. Gating logic rejects casts when mana is insufficient. Costs loaded from `GameBalance.Mana.SpellCosts`. |
| Save / Load | `SaveManager.cs` | JSON serialization/deserialization with version field and corruption recovery. Handles New Game reset and Continue restore. |
| Progression | `ProgressionManager.cs` | Tracks bossIndex, token count, and unlock state. Persists across scene loads via `DontDestroyOnLoad`. |
| Localization | `LocalizationManager.cs` | Loads `en.json` or `es.json` from Resources on startup. Fires a language-change event that all `LocalizedText` components subscribe to. |
| Localized Text | `LocalizedText.cs` | Component attached to every Text/TMP object. Holds a localization key and refreshes its display string on language change events. |
| Dialogue | `DialogUI.cs` | Overlay system for narrative sequences. Accepts a panel queue (speaker + text), displays them in order, and notifies `GameFlowManager` on completion. |
| UI Manager | `UIManager.cs` | Persistent HUD canvas — health hearts, mana bar, boss health bar. No boss scene creates its own canvas; all HUD lives here. |
| Boss Base | `BossController.cs` | Abstract base class for all bosses. Defines HP system, phase threshold checks, death trigger → `GameFlowManager.AvanzarFase()`, and the initialization contract. |

### Spell System — Full Combination Table

Three base elements combine via a queue system into 27 unique spells:

| Tier | Slots | Combination Count | Dispatch Key Format |
|------|-------|-------------------|---------------------|
| T1 | 1 element | 3 | `"queso"` |
| T2 | 2 elements | 9 (ordered) | `"queso\|pepperoni"` |
| T3 | 3 elements | 27 (ordered, with repeats) | `"queso\|queso\|queso"` |

All spell values — damage, mana cost, duration, area radius, DoT tick rate — are defined in `GameBalance.Spells.*` and `GameBalance.Mana.SpellCosts`. No spell script contains an inline numeric constant.

### Project Folder Structure

```
Assets/
├── FlowScenes/          — All game scenes (MainMenu, Shop, BossArena 1–4)
├── Prefabs/
│   ├── Base/            — Player, wands, shared objects
│   ├── Bosses/          — Boss prefabs per boss
│   └── ElementsAttack/  — One prefab per spell combination
├── Scripts/
│   ├── Bosses/          — Boss controllers
│   ├── Camera/          — Cinemachine configuration
│   ├── Core/            — GameBalance.cs, shared interfaces
│   ├── Editor/          — Editor-only tooling scripts
│   ├── Elements/        — Element definition data
│   ├── ElementsAttack/  — Spell behavior scripts
│   ├── GameFlowManager/ — GameFlowManager.cs
│   ├── Items/           — Shop item logic
│   ├── Languaje/        — LocalizationManager, LocalizedText
│   ├── Pblob/           — P'blob-specific scripts
│   ├── Player/          — PlayerController, ManaSystem, HP
│   ├── Progression/     — ProgressionManager, SaveManager
│   ├── Sound/           — SoundManager (v2.0)
│   ├── Status/          — Burn, slow, and other status effects
│   ├── TileMap/         — Grid and tile scripts (P'blob Phase 3)
│   ├── UI/              — UIManager, DialogUI, VictoryUI, shop UI
│   └── Weapon/          — Wand and projectile base classes
├── Sprites/             — All sprite assets organized by subject
└── Resources/
    └── Languages/       — en.json, es.json
```

---

*Pizzard v1.0 — Complete*
*Unity 2022.3.62f1 · Windows Standalone · Released 2026*
