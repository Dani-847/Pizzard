# Phase 21: Spell Completion — All Combos (T2 + T3) - Context

**Gathered:** 2026-03-01
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement all 20 missing spell combinations (4 T2 + 16 T3) and verify all 16 existing combos work correctly. Mechanics only — no VFX, no SFX (deferred to v2). All damage/cost values from GameBalance.Spells.*.

</domain>

<decisions>
## Implementation Decisions

### Scope
- **Mechanics only** — no particle effects, no sound
- All values from `GameBalance.Spells.*` — no hardcoded numbers
- Each combo registered in CombinationDatabase.asset with a prefab
- VFX/SFX deferred to v2

### Missing T2 — Mechanics (from design doc)
- **`queso|queso`** — Wall placed on ground: damages enemies standing on it (ticking), reflects projectiles, has finite HP
- **`queso|pepperoni`** — Ground area placed at cast position: applies burn on contact, damage scales the longer enemy stays in it
- **`queso|piña`** — Pillar placed at cast position: ticking area damage around it
- **`pepperoni|queso`** — Projectile that sticks to enemy on hit, applies burn, damage increases the longer it stays stuck (2s duration)

### Missing T3 — Mechanics (from design doc)
- **`piña|piña|piña`** — Projectile explodes into sub-projectiles, each sub also explodes
- **`piña|piña|queso`** — Projectile explodes into sub-projectiles, subs absorb incoming projectiles
- **`piña|piña|pepperoni`** — Projectile explodes into sub-projectiles that apply burn
- **`piña|queso|piña`** — Absorbing projectile: damage on hit = number of absorbed projectiles
- **`piña|queso|queso`** — Absorbing projectile: on hit becomes a cone
- **`piña|queso|pepperoni`** — Absorbing projectile: applies burn stacks equal to absorbed count
- **`piña|pepperoni|piña`** — Two projectiles deal damage en route → teleport to impact → explosion
- **`piña|pepperoni|pepperoni`** — Same but bigger explosion
- **`piña|pepperoni|queso`** — Same but explosion reflects projectiles
- **`queso|queso|queso`** — Black hole: absorbs projectiles, fires them back at enemy at ×1.5 damage
- **`queso|piña|piña`** — Pillar: bigger area, more damage
- **`queso|piña|queso`** — Pillar: more HP, slows projectiles passing through
- **`queso|piña|pepperoni`** — Pillar: ticking damage + stacks burn, bonus damage per burn stack
- **`queso|pepperoni|pepperoni`** — Ground area: double burn stacks
- **`queso|pepperoni|piña`** — Ground area: burn + separate DoT
- **`queso|pepperoni|queso`** — Ground area: larger radius

### Verify Existing
- All 5 existing T2 combos: confirm hit detection, correct damage, mana cost deducted
- All 11 existing T3 combos: same checks

### Claude's Discretion
- Script naming convention for new combos (follow existing pattern e.g. PepperoniQuesoAttack.cs)
- Whether to reuse base classes vs inline (follow existing pattern)
- Prefab structure for area/wall/pillar combos

</decisions>

<specifics>
## Specific Ideas

- Design doc source: `ProyectDocumentation/Propuesta de proyecto integrado.txt`
- Pillar/wall/area combos (queso-starting) are placed at cast position, not fired as projectiles
- piña|pepperoni variants: two projectiles travel to target, then player teleports, then explosion — same base as existing piña|pepperoni teleport but with added effects
- queso|queso|queso black hole: stationary for a duration, pulls in projectiles, then fires them all back

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- `CharacterProjectile.cs` — base class for all fired projectiles, handles collision + damage
- `PepperoniAttack.cs` — example of status effect application on hit
- `PineappleCheeseProjectile.cs` — example of absorbing projectile pattern
- `CheesePepperoniWall.cs` — example of placed ground effect
- `SmallPiñaProjectile.cs` — sub-projectile used by piña|piña splitter
- `SmallStickyProjectile.cs` — sub-projectile used by pepperoni|queso sticky
- `FireTrail.cs` — example of ground-placed ticking area

### Established Patterns
- All combos registered in `CombinationDatabase.asset` with a key + prefab
- Special-case combos (wall, teleport, shield) handled in `PlayerAimAndCast.cs` with dedicated `Handle*` methods
- Normal projectile combos go through `HandleNormalAttack(entry)` — no special case needed
- Mana cost checked via `ManaSystem.GetSpellCost(key)` before casting

### Integration Points
- New entries: add to `CombinationDatabase.asset`
- New special cases (area, pillar, wall): add `Handle*` method + `else if (key == "...")` block in `PlayerAimAndCast.cs`
- New scripts: `Assets/Scripts/ElementsAttack/` — follow Otros/ for areas/walls, Projectile/ for projectiles

</code_context>

<deferred>
## Deferred Ideas

- VFX for all combos — v2.0
- SFX for all combos — v2.0
- Balance tuning of new combo values — Phase 23 (Final Polish)

</deferred>

---

*Phase: 21-spell-completion*
*Context gathered: 2026-03-01*
