# ROADMAP.md

> **Current Phase**: 16 — Language System Completion
> **Milestone**: v1.0 (Full Loop & 4 Bosses)
> **Phases Complete**: 1–15 (including 13.1 Mana + 13.2 GameBalance + 14 Dialogue + 15 Boss Loop)
> **Phases Remaining**: 16–30

---

## Completed Phases (1–15)

| Phase | Status | Summary |
|-------|--------|---------|
| 1 | ✅ | Foundation & Refactoring — Archive folder, GameFlowManager singleton, scene pipeline |
| 2 | ✅ | Core UI & Main Menu — Menu, Settings, Dialog UI wired to GameFlowManager |
| 3 | ✅ | Player Mechanics & Wand Combinations — Movement, damage, Tiers 1-3, element combos |
| 4 | ✅ | Progression & Shop System — Currency drops, Shop interface, item logic |
| 5 | ✅ | Bosses Part 1 — P'blob (mustache + simon-says) and Hec'kiel (elemental dragon) |
| 6 | ✅ | Bosses Part 2 — Pomodoro Paganini (pong) and Niggel Worthington (buff/debuff) |
| 7 | ✅ | Boss State Standardization — Boss loading context, GameFlowManager edge cases |
| 8 | ✅ | Main Menu & UI Cleanup — Settings, intro dialogue sequences |
| 9 | ✅ | Boss Debugging — Input freeze, rigidbodies, UI bars, timescale |
| 10 | ✅ | Codebase Refactor & Polish — Cinemachine, Pblob AI, Shop element selection |
| 11 | ✅ | Critical Loop & UI Fixes — Health bar, camera, Shop buttons, progression loop |
| 12 | ✅ | Shop Progression & Layout Rework — Shop UI, Wand unlocking, Token/Mana tracking |
| 13.1 | ✅ | Mana System — Rename Fatigue→Mana, per-spell cost dictionary, cast gating |
| 13.2 | ✅ | GameBalance Centralization — 123+ constants into `GameBalance.cs`, 36 files updated |
| 14 | ✅ | Dialogue System & Narrative Flow — DialogUI overlay, intro/pre/post-boss/death-shop dialogue sequences |
| 15 | ✅ | Complete    | 2026-02-28 | After Boss | Earned | Available at Next Shop |
|------------|--------|------------------------|
| Boss 1     | 1      | 1 (spent 1 in Shop 1)  |
| Boss 2     | 2      | 2 + unspent            |
| Boss 3     | 2      | 2 + unspent            |
| Boss 4     | N/A    | Game ends              |

Starting token = 1 (given before Shop 1).

**Game Over**: Return to last save point. "Game Over" screen with Continue.

**Verify**: Full loop: Menu → Dialogue → Shop1 → Dialogue → Boss1 → … → Boss4 → Credits.

---

### Phase 16: Language System Completion
**Status**: ⬚ Not Started
**Dependencies**: Phase 14

**Objective**: Complete the EN ↔ ES translation system.

**Requirements**:
- Options menu: language toggle (functional).
- On toggle: ALL UI text updates in real-time without scene reload.
- Sources: `Resources/Languages/en.json` and `Resources/Languages/es.json`.
- Every UI text uses a localization key, never hardcoded strings.

**Localization Keys** (minimum):
- Menu: `play`, `options`, `exit`, `volume`, `language`, `combinations`, `accept`
- Shop: `upgrade_wand`, `upgrade_potion`, `upgrade_mana`, `exit_shop`, `token_count`, `warning_exit`
- Dialogue: all narrative keys
- Combat UI: `health`, `mana`, `potion_count`
- Boss names, Game over, Credits

**Implementation**:
- `LocalizationManager` singleton loads JSON on startup.
- `LocalizedText` component on all Text/TMP with a key.
- On language change: event fires, all components refresh.
- Language preference saved in SaveData.

**Verify**: Switch in Options, confirm ALL text changes. Screenshot both EN and ES.

---

### Phase 17: UI Polish & Resolution Independence
**Status**: ⬚ Not Started
**Dependencies**: Phase 16

**Objective**: Final UI polish pass across all scenes.

**Requirements**:
- Every Canvas: Scale With Screen Size (1920×1080, Match 0.5).
- Test at: 1920×1080, 1366×768, 2560×1440, 1280×720.
- No clipping or off-screen elements at any resolution.
- Consistent visual style across all screens.

**Fixes**:
- Shop buttons: uniform size, spacing, hover/press feedback.
- Element selection: tier-based layout (1/2/3 slots by tier).
- Mana bar: vertical fill bottom→top, color gradient.
- Boss health bars: consistent across all boss scenes.
- Dialogue box: no overlap with combat UI.

**Verify**: Unity MCP for anchor verification. Screenshot at EACH resolution.

---

### Phase 18: Boss 1 — P'blob AI & Patterns
**Status**: ⬚ Not Started
**Dependencies**: Phase 15, `GameBalance.Bosses.Pblob`

**Objective**: Full P'blob boss fight implementation.

**Current State**: `PblobController` exists with basic phase system, vulnerability windows, knockback, and wander AI. Attack patterns are stubbed.

**Design**:
- 3-room progression (Room 1 → Room 2 → Room 3).
- Room 1: Hair attacks — projectile patterns from mustache, simon-says color matching.
- Room 2: Tile puzzle — floor tiles activate in patterns, player must dodge/match.
- Room 3: Final phase — combined attacks, faster patterns, rage mode at low HP.

**Mustache Mechanic**:
- Boss has visible mustache with multiple "hair strands" as hitboxes.
- Destroying hair strands reveals vulnerable core temporarily.
- Hair regrows after vulnerability window closes.

**Implementation**:
- Use `GameBalance.Bosses.Pblob` for all timing/HP values.
- `PblobAttackPattern1` and `PblobAttackPattern2` need real implementations.
- Room transitions via `phase2Door` and `StartPhase2()` already wired.
- Add projectile prefabs, visual effects, audio triggers.

**Verify**: Each room independently with Play mode + screenshots.

---

### Phase 19: Boss 2 — Hec'kiel AI & Patterns
**Status**: ⬚ Not Started
**Dependencies**: Phase 18 (pattern template), `GameBalance.Bosses.Heckiel`

**Objective**: Full Hec'kiel boss fight.

**Current State**: `HeckielController` exists with attack interval timer, phase transition at 50% HP, and stubbed attack methods.

**Design**:
- Elemental dragon that reflects the last element it was hit by.
- Phase 1: Random attacks with element mixing.
- Phase 2 (at 50% HP): Dragon splits into two attacking heads — each fires independently, must destroy both.

**Attacks**:
1. Random element projectiles (fire/ice/wind themed visuals).
2. Reflect last element — copies player's spell back.
3. Combine elements — mixes reflected + random into new attack.

**Implementation**:
- `RegisterElementHit()` already tracks last element.
- Need: projectile prefabs, split animation, dual-head controller.
- Phase 2 needs second head GameObject with synced attack timing.

**Verify**: Phase transition, element tracking, and split mechanic.

---

### Phase 20: Boss 3 — Pomodoro Paganini AI
**Status**: ⬚ Not Started
**Dependencies**: `GameBalance.Bosses.Pomodoro`

**Objective**: Full Pomodoro Paganini (The Oven) boss fight.

**Current State**: `PomodoroController` exists with attack interval coroutine and two stubbed attack types.

**Design**:
- Stationary boss — giant oven that cannot be directly damaged.
- Only takes damage from reflected projectiles (pong mechanic).
- Fires slow, heavy projectiles player must dash into to reflect.
- Also fires undodgeable AoE ground hazards to force movement.

**Pizza Pong Mechanic**:
- Boss fires "deflectable" pizza projectiles.
- Player uses Cheese Shield or dash to bounce them back.
- Reflected projectiles gain speed each bounce.
- After 3 bounces, projectile explodes dealing massive damage to boss.

**Element Matching (advanced)**:
- Boss fires element-colored projectiles.
- Matching the correct shield element doubles reflect damage.
- Wrong element still reflects but at base damage.

**Implementation**:
- Need: deflectable projectile prefab with bounce counter, AoE hazard prefab.
- `BlockStandardAttack()` already shows immunity feedback.

**Verify**: Pong mechanic, bounce counting, and element matching.

---

### Phase 21: Boss 4 — Niggel Worthington AI
**Status**: ⬚ Not Started
**Dependencies**: `GameBalance.Bosses.Niggel`

**Objective**: Full Niggel Worthington (The Rich Guy) boss fight.

**Current State**: `NiggelController` exists with attack interval, steal mechanic (range check + currency theft + speed buff), and 3 stubbed attacks.

**Design**:
- Mobile boss that steals currency and buffs himself.
- Gets faster with each successful steal (`speedMultiplier`).
- Has toy soldiers as minions.

**Attacks**:
1. Throw Money — coin bag projectiles.
2. Rich Dash — high-speed dash across room (uses `speedMultiplier`).
3. Steal Stats — if close enough, steals currency or HP.

**Buff/Debuff System**:
- Each steal gives Niggel +0.2x speed (`GameBalance.Bosses.Niggel.SpeedBuffPerSteal`).
- Player gets debuffed: slightly slower, slightly weaker.
- Defeating Niggel restores all stolen stats/currency.

**Minions**:
- Toy soldiers spawn at intervals.
- They don't deal much damage but body-block player spells.
- Must be cleared to reach Niggel effectively.

**Implementation**:
- Need: coin bag projectile prefab, minion prefab, dash trail VFX.
- Steal mechanic already functional — add visual feedback.

**Verify**: Steal mechanic, speed scaling, and minion spawning.

---

### Phase 22: Spell Polish — Tier 1 (Single Element)
**Status**: ⬚ Not Started
**Dependencies**: Boss fights complete for testing context

**Objective**: Polish and complete all 3 Tier 1 single-element spells.

**Spells**:
1. **Queso** — Cheese Shield (orbiting, reflects projectiles, contact damage). Current: `CheeseShield.cs` works. Polish: visual feedback, sound, particle trail.
2. **Pepperoni** — Fire projectile with burn DoT. Current: `PepperoniAttack.cs` works. Polish: fire VFX, burn indicator on target.
3. **Piña** — Fast straight projectile. Current: `PiñaAttack.cs` works (inherits `CharacterProjectile`). Polish: unique VFX.

**For Each Spell**:
- Verify damage values match `GameBalance.Spells.*`
- Add particle effects (spawn + travel + impact).
- Add sound effects (cast + impact).
- Verify mana cost deduction works correctly.

All values come from `GameBalance.cs` — no new hardcoded numbers.

---

### Phase 23: Spell Polish — Tier 2 (Two-Element Combos)
**Status**: ⬚ Not Started
**Dependencies**: Phase 22

**Objective**: Polish all 9 two-element combinations (order matters).

**Combos**:

| Key | Name | Mechanic |
|-----|------|----------|
| queso\|pepperoni | CheesePepperoniWall | Burning wall that reflects + applies burn |
| queso\|piña | PineappleCheese | Absorbing projectile that grows + explodes |
| pepperoni\|queso | PepperoniQueso sticky | Sticks to boss, burns, scales damage |
| pepperoni\|piña | PepperoniPiña bomb | Timed explosion with AoE |
| piña\|queso | Piña+Queso variant | (verify what exists) |
| piña\|pepperoni | PineapplePepperoni | Teleport + explosion at destination |
| pepperoni\|pepperoni | Fire trail | Projectile leaves burning trail |
| queso\|queso | Static shield variant | (verify — may be Tier 3) |
| piña\|piña | Splitter | Splits into 4 diagonal sub-projectiles |

**For Each**: Verify implementation exists, matches GameBalance values, add VFX/SFX, test mana cost.

---

### Phase 24: Spell Polish — Tier 3 (Three-Element Combos)
**Status**: ⬚ Not Started
**Dependencies**: Phase 23

**Objective**: Polish and complete all Tier 3 three-element combinations.

**Key Combos** (most have implementations):
- `pepperoni|pepperoni|pepperoni` — Dire trail spawner
- `pepperoni|pepperoni|piña` — Rotating pepperoni spawner
- `pepperoni|pepperoni|queso` — Area fire trail spawner
- `pepperoni|piña|pepperoni` — Catapult + fire trail explosion
- `pepperoni|piña|piña` — Enhanced explosion
- `pepperoni|piña|queso` — Enhanced fire trail projectile
- `pepperoni|queso|pepperoni` — Long sticky burn
- `pepperoni|queso|piña` — Sticky spawning children
- `pepperoni|queso|queso` — Long duration sticky
- `queso|queso|piña` — Static cheese shield
- `queso|queso|pepperoni` — Cheese pepperoni wall (already Tier 2 combo?)
- `queso|queso|queso` — Triple shield (define mechanic)
- `piña|piña|piña` — Triple split (define mechanic)

**For Each**: Verify implementation exists, create missing ones, all values from `GameBalance.Spells.*`, add VFX + SFX, balance high mana cost / high impact. 27 total permutations (3³). Focus on distinct gameplay feel per combo.

---

### Phase 25: Tutorial System
**Status**: ⬚ Not Started
**Dependencies**: Phase 15 (bossIndex check)

**Objective**: Interactive tutorial teaching basic mechanics.

**Requirements**:
- Triggers on first playthrough (`bossIndex == 0`, no save).
- Teaches: movement (WASD), dash (Space), aiming (mouse), casting (click).
- Step-by-step: each mechanic highlighted with prompt overlay.
- Freezes game until player completes the action.
- Shows element selection tutorial before Shop 1.
- Teaches potion usage (Q key) and mana bar explanation.
- Skippable for returning players (check save file).

**Implementation**:
- `TutorialManager` singleton with step queue.
- `TutorialOverlay` UI: highlight area + instruction text.
- Input detection per step to auto-advance.
- Save `tutorialComplete` flag.

**Verify**: Each tutorial step flows correctly. Screenshot overlays.

---

### Phase 26: Save System Polish
**Status**: ⬚ Not Started
**Dependencies**: Phase 25 (tutorialComplete flag)

**Objective**: Harden the save/load system.

**Requirements**:
- Validate JSON on load — handle corruption gracefully.
- Prevent external JSON editing from breaking game state.
- Add version field to SaveData for future migration.
- Auto-save at key checkpoints: after boss defeat, after shop close.
- Manual save option in pause menu.
- "New Game" properly resets all state.
- "Continue" loads exact loop position.

**Edge Cases**:
- Corrupted file → warning + offer to start new game.
- Missing fields in old saves → fill with defaults.
- `bossIndex` out of range → clamp to valid range.

**Verify**: Corrupt save file manually, confirm graceful recovery.

---

### Phase 27: Audio & SFX
**Status**: ⬚ Not Started
**Dependencies**: Phases 18-24 (all combat content must exist)

**Objective**: Complete audio system.

**Requirements**:
- `SoundManager` singleton (already exists) — verify DontDestroyOnLoad.
- Music tracks: menu theme, shop theme, per-boss battle music (4 tracks).
- Music transitions: crossfade between scenes.
- Volume control: master, music, SFX sliders in Options (save to prefs).

**SFX** (minimum):
- Spell cast (per element type), spell impact (per element type).
- Shield reflect, boss hit / boss defeat.
- Player damage / player death.
- UI: button click, purchase confirm, error buzz.
- Potion use, dialogue advance (typewriter click).

**Implementation**:
- AudioClip references on SoundManager or per-prefab AudioSource.
- Placeholder sounds acceptable — structure must support easy replacement.
- Volume settings persisted via PlayerPrefs or SaveData.

**Verify**: Every interaction has audio feedback.

---

### Phase 28: Credits & Endgame
**Status**: ⬚ Not Started
**Dependencies**: Phase 15 (boss loop), Phase 16 (localization)

**Objective**: Endgame sequence and credits.

**Requirements**:
- After Boss 4 defeated: final dialogue with Raberto.
- Magical pizza reward animation/scene.
- Credits roll: scrolling text with team names.
- "Return to Menu" button after credits.
- Save file marks game as complete (`bossIndex = 4`).
- On "Continue" from menu with complete save: offer New Game+ or replay.

**Implementation**:
- `CreditsScene` with auto-scrolling TMP text.
- `EndgameManager` handles pizza reward sequence.
- Credits text from localization JSON (supports EN/ES).

**Verify**: Full endgame flow: Boss4 → dialogue → reward → credits → menu.

---

### Phase 29: Controller Support
**Status**: ⬚ Not Started
**Dependencies**: Phase 17 (UI polished first)

**Objective**: Full Xbox/gamepad controller support.

**Requirements**:
- All menus navigable with D-pad/left stick.
- Button prompts change based on detected input device.
- Combat: right stick for aiming, triggers for casting.
- Element selection: bumpers to cycle, A to confirm.
- Shop: D-pad navigation, A to buy, B to back.
- Dialogue: A to advance. Pause menu: Start button.

**Implementation**:
- Unity Input System already in use (`InputActions` asset).
- Add gamepad bindings to all existing InputActions.
- UI navigation: EventSystem + explicit navigation on Selectable components.
- Input icon swapper: detects keyboard vs gamepad, shows correct icons.

**Edge Cases**:
- Hot-swap between keyboard and controller mid-gameplay.
- No cursor visible in controller mode.
- Mouse movement re-enables cursor.

**Verify**: Play entire game start-to-finish using ONLY controller.

---

### Phase 30: Final Polish & Balancing
**Status**: ⬚ Not Started
**Dependencies**: ALL previous phases complete

**Objective**: Final pass — bugs, balance, performance. Last phase before release.

**Bug Sweep**:
- Play full loop 3 times, document every issue.
- Fix all blocking bugs (crashes, softlocks, progression breaks).
- Fix all visual bugs (Z-order, clipping, missing sprites).

**Balance Pass** (all via `GameBalance.cs`):
- Boss HP: ensure each boss takes 2-5 minutes.
- Spell costs: mana shouldn't run out in <10 seconds of sustained casting.
- Potion economy: 3 potions should be enough for first boss, tight for later.
- Token economy: player should feel progression pressure but not stuck.
- Damage values: player shouldn't one-shot bosses or vice versa.

**Performance**:
- Profile with Unity Profiler.
- Fix any frame drops below 60fps.
- Object pooling for frequently spawned projectiles.
- Reduce GC allocations in hot paths.

**Build**:
- Create Windows standalone build.
- Test build outside editor.
- Verify save file paths work in build.

---

## Phase Dependency Graph

```
13 ✅ ─→ 14 ─→ 15 ─→ 16 ─→ 17
                │              │
                ├→ 18 ─→ 19 ─→ 20 ─→ 21
                │                         │
                ├→ 25 ─→ 26              │
                │                         ├→ 22 ─→ 23 ─→ 24
                │                         │
                ├→ 28                    ├→ 27
                │
                └→ 29 (after 17)

All ─→ 30 (final)
```

---

## Appendix: Session Protocol

**Every session start**: `/resume` or `/progress`. If new, `/new-project`. Then `/map` to regenerate `ARCHITECTURE.md`.

**Phase workflow**: `/discuss-phase N` → `/plan` → `/execute` → `/verify`

**Emergency**: `/debug [problem]` if stuck. `/pause → new chat → /resume` if context degrades.

---

## Appendix: Golden Rules

1. **Always `/resume` at session start** — never assume context carries over.
2. **Always `/verify` before moving on** — demand screenshots and code evidence.
3. **Never skip the plan** — no code without `/plan` first.
4. **One phase at a time** — never combine phases.
5. **Screenshot every UI change.**
6. **If something breaks, `/debug` immediately** — don't let errors compound.
7. **Save state before closing** — always `/pause` at end of session.
8. **All balance values go through `GameBalance.cs`** — no new hardcoded numbers after Phase 13.2.
9. **Suggest next GSD commands to the user** — always show what comes next.
