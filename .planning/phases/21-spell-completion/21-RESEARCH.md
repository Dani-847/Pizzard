# Phase 21: Spell Completion — All Combos (T2 + T3) - Research

**Researched:** 2026-03-01
**Domain:** Unity 2D C# — Spell combination system (scripts, prefabs, ScriptableObject database, mana costs)
**Confidence:** HIGH — all findings sourced directly from the live codebase

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **Scope:** Mechanics only — no particle effects, no sound
- All values from `GameBalance.Spells.*` — no hardcoded numbers
- Each combo registered in `CombinationDatabase.asset` with a key + prefab
- VFX/SFX deferred to v2

#### Missing T2 mechanics
- `queso|queso` — Wall placed on ground: damages enemies standing on it (ticking), reflects projectiles, has finite HP
- `queso|pepperoni` — Ground area placed at cast position: applies burn on contact, damage scales the longer enemy stays in it
- `queso|piña` — Pillar placed at cast position: ticking area damage around it
- `pepperoni|queso` — Projectile that sticks to enemy on hit, applies burn, damage increases the longer it stays stuck (2s duration)

#### Missing T3 mechanics (16 combos — see phase description)
(Full list in CONTEXT.md `## Missing T3 — Mechanics`)

#### Verify Existing
- All 5 existing T2 combos: confirm hit detection, correct damage, mana cost deducted
- All 11 existing T3 combos: same checks

### Claude's Discretion
- Script naming convention for new combos (follow existing pattern e.g. `PepperoniQuesoAttack.cs`)
- Whether to reuse base classes vs inline (follow existing pattern)
- Prefab structure for area/wall/pillar combos

### Deferred Ideas (OUT OF SCOPE)
- VFX for all combos — v2.0
- SFX for all combos — v2.0
- Balance tuning of new combo values — Phase 23 (Final Polish)
</user_constraints>

---

## Summary

Phase 21 is entirely a Unity C# codebase task — no third-party libraries, no web APIs. The project has a mature, well-factored spell system: `CharacterProjectile` as the projectile base class, a family of placed-object scripts for walls/areas/shields, `PlayerAimAndCast.cs` as the central dispatcher, and `CombinationDatabase.asset` as the ScriptableObject registry. Every existing T2 and T3 spell follows one of three patterns: (1) a `CharacterProjectile` subclass fired via `HandleNormalAttack`, (2) a placed MonoBehaviour instantiated via a dedicated `Handle*` method in `PlayerAimAndCast`, or (3) a `CharacterProjectile` subclass with a custom `Handle*` method for non-trivial setup.

Twenty new combos need scripts + prefabs + database entries; 16 existing combos need mechanical verification. The biggest implementation complexity is in the `queso|queso` (wall with HP) and `queso|queso|queso` (black hole with absorption + return fire) combos, which require new MonoBehaviour patterns not yet present. The `piña|pepperoni` T3 variants (3 combos) reuse the `PineapplePepperoniAttack` teleport base with modifier scripts. The absorbing-projectile T3 variants inherit from `PineappleCheeseProjectile`.

`nyquist_validation` is **false** — no test infrastructure section needed.

**Primary recommendation:** Implement new combos in strict pattern order — first the script, then the prefab (duplicate nearest existing), then register in `CombinationDatabase.asset`, then add `Handle*` dispatch only when placement logic differs from `HandleNormalAttack`. Add `GameBalance` constants before writing any script.

---

## Standard Stack

### Core (all already present in the project)

| Component | Location | Purpose |
|-----------|----------|---------|
| `CharacterProjectile.cs` | `Assets/Scripts/ElementsAttack/Projectile/` | Base class for all fired projectiles — handles collision, boss damage, tag filtering |
| `CombinationDatabase.cs` | `Assets/Scripts/DataBase/` | ScriptableObject list of `CombinationEntry` (key + prefab) |
| `CombinationDatabase.asset` | `Assets/` | Singleton asset — all new entries added here |
| `PlayerAimAndCast.cs` | `Assets/Scripts/Player/` | Spell dispatcher — routes key to `Handle*` methods or `HandleNormalAttack` |
| `GameBalance.cs` | `Assets/Scripts/Core/` | Single source of truth for all numeric constants |
| `StatusEffectSystem` (on bosses) | Boss GameObjects | `ApplyEffect(StatusType.picante, duration, source, stacks)` |
| `FireTrail.cs` | `Assets/Scripts/ElementsAttack/Otros/` | Reusable ticking-area-damage component |
| `PineappleCheeseProjectile.cs` | `Assets/Scripts/ElementsAttack/Projectile/` | Absorbing projectile base — `AbsorbEnemyProjectiles()`, `Impact()` |
| `PepperoniQuesoPepperoniAttack.cs` | `Assets/Scripts/ElementsAttack/Projectile/` | Sticky projectile base pattern |
| `PepperoniPineapplePepperoniAttack.cs` | `Assets/Scripts/ElementsAttack/Projectile/` | Timed-explosion catapult base with virtual `Explode()`, `ApplyExplosionDamage()`, `CreateSingleFireTrail()` |
| `SmallStickyProjectile.cs` | `Assets/Scripts/ElementsAttack/Projectile/` | Sub-projectile sticky pattern used by pepperoni|queso family |

### Supporting

| Component | Purpose | When to Use |
|-----------|---------|-------------|
| `EnemyProjectile.cs` | Enemy projectile tag + component — must be disabled on reflection | All reflection-capable walls |
| `PblobController.TakeDamage(float)` | Legacy boss damage path | Scripts that haven't migrated to `BossBase.TakeDamage(int)` |
| `BossBase.TakeDamage(int)` | Modern boss damage path | Preferred for new scripts |
| `SmallPiñaProjectile.cs` | Sub-projectile used by piña|piña splitter | piña T3 sub-projectile spawn |

---

## Architecture Patterns

### Pattern 1: Normal Projectile (fires via HandleNormalAttack)

Used by: `pepperoni|queso`, all piña T3 absorbing variants, piña|piña T3 variants, pepperoni T3 variants.

```
PlayerAimAndCast.HandleNormalAttack(entry)
  → Instantiate(entry.attackPrefab, castPoint.position, rot)
  → charProjectile.Initialize(direction)     ← called if component found
```

**Script template** — subclass `CharacterProjectile`, override `OnTriggerEnter2D` and optionally `Start`/`Initialize`. Pull values from `GameBalance.Spells.YourClass.*`.

```csharp
public class QuesoQuesoAttack : CharacterProjectile   // example name
{
    public float someValue = Pizzard.Core.GameBalance.Spells.QuesoQueso.SomeValue;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // custom logic first, then:
        base.OnTriggerEnter2D(other);
    }
}
```

### Pattern 2: Placed Object (requires Handle* method in PlayerAimAndCast)

Used by: `queso|queso` (wall), `queso|pepperoni` (ground area), `queso|piña` (pillar), and their T3 variants.

```
PlayerAimAndCast.HandleQuesoQueso(entry)
  → Instantiate at castPoint.position (or aimDir offset)
  → myComponent.InitializeAtPosition(spawnPosition)
```

**Key decisions per placed object:**
- Does it need `aimDir` offset? — Yes for walls/pillars (use `GetCurrentAimDirection()` + `GameBalance.Casting.*Distance`)
- Does it need single-instance enforcement? — Yes for wall/pillar (static `CurrentActiveX` pattern, see `CheesePepperoniWall.CurrentActiveWall`)
- Does it react to `EnemyProjectile` tag? — Yes for anything that reflects/absorbs projectiles

**Existing Casting distances in GameBalance:**
```csharp
GameBalance.Casting.StaticShieldDistance = 2f   // queso|queso|piña
GameBalance.Casting.WallDistance = 2.5f          // queso|queso|pepperoni
GameBalance.Casting.CatapultMinDistance = 2f     // pepperoni|piña|pepperoni
```
New placed combos (`queso|queso`, `queso|pepperoni`, `queso|piña` and T3 variants) need their own `PlacementDistance` constants added to `GameBalance.Casting`.

### Pattern 3: Special-Dispatch Projectile (needs Handle* but fires a projectile)

Used by: `piña|pepperoni` (teleport — no projectile at all, purely `PineapplePepperoniAttack.Initialize(transform, this)`) and `pepperoni|piña|pepperoni` (catapult — needs direction + target).

The T3 `piña|pepperoni` variants (`piña|pepperoni|piña`, `piña|pepperoni|pepperoni`, `piña|pepperoni|queso`) share the same teleport + explosion mechanic as `piña|pepperoni`. They should be added as `else if (key == "piña|pepperoni|piña")` blocks in `PlayerAimAndCast.OnCastSpell`, each calling a `HandleTeleportAttack`-style method but passing a different prefab variant. The prefab variant script subclasses `PineapplePepperoniAttack` and overrides `CreateExplosion`.

### Pattern 4: Inheritance Chain (T3 extends T2 extends base)

The pepperoni catapult family demonstrates this perfectly:
- `PepperoniPineapplePepperoniAttack` — base catapult with virtual `Explode()`, `ApplyExplosionDamage()`, `CreateSingleFireTrail()`
- `PepperoniPineapplePineappleAttack` — overrides radius/damage multipliers, disables fire trail
- `PepperoniPineappleQuesoAttack` — overrides fire trail params, adds burn on explosion

New combos in families that already have a base class SHOULD subclass rather than copy-paste.

### Anti-Patterns to Avoid

- **Hardcoded numbers:** Every float/int value must reference `GameBalance.Spells.YourClass.*`. Add the constant first.
- **Calling `base.OnTriggerEnter2D` before custom logic for special effects:** Status effects must be applied before `Destroy(gameObject)` in base. Always apply effect first, then call base (see `PepperoniAttack` pattern).
- **Forgetting `combiner.ClearSelectedElements()` in Handle* methods:** Every dispatch path must clear elements or the combiner gets stuck.
- **Using `PblobController.TakeDamage` directly in new scripts:** Prefer `BossBase.TakeDamage(int)` from `Pizzard.Bosses` namespace — `CharacterProjectile` already does this with fallback to `PblobController`.
- **Calling `base.Start()` in stuck/placed objects:** `CharacterProjectile.Start()` schedules a `Destroy(gameObject, lifetime)` — sticky/placed scripts must call `rb = GetComponent<Rigidbody2D>()` manually and set their own lifetime (see `PepperoniQuesoPepperoniAttack.Start()`).

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Ticking area damage | Custom loop | `FireTrail.cs` with `Initialize(charges, interval, duration, radius, effectDuration)` | Already handles coroutine, collider, duration countdown |
| Projectile absorption | Custom overlap logic | Subclass `PineappleCheeseProjectile`, call `base.AbsorbEnemyProjectiles()` in Update | All absorption/growth/impact logic exists |
| Sticky projectile with scaling damage | Custom coroutine | Subclass `PepperoniQuesoPepperoniAttack`, override `StickCoroutine` if needed | Pattern: `isStuck`, `StickCoroutine`, `CleanupAndDestroy` |
| Catapult with virtual explosion | Custom timed behavior | Subclass `PepperoniPineapplePepperoniAttack`, override `Explode()`/`ApplyExplosionDamage()` | All timing/scale animation already in base |
| Single-instance wall/shield | Own static field | Copy `CurrentActiveWall` pattern from `CheesePepperoniWall` | Handles replacement + cleanup on `OnDestroy` |
| Status effect application | Direct field manipulation | `statusSystem.ApplyEffect(StatusType.picante, duration, gameObject, stacks)` | Stacks stack correctly through `StatusEffectSystem` |

---

## Common Pitfalls

### Pitfall 1: Missing `GameBalance` constants for new combos
**What goes wrong:** Script won't compile — `GameBalance.Spells.QuesoQueso.WallHP` doesn't exist.
**Why it happens:** New combos need new nested static classes in `GameBalance.cs`.
**How to avoid:** Add the `GameBalance.Spells.YourCombo.*` static class FIRST before writing the spell script.
**Warning signs:** Compile errors referencing `GameBalance`.

### Pitfall 2: Missing SpellCosts entries cause mana drain to use DefaultSpellCost (30f)
**What goes wrong:** The combo costs 30 mana instead of its intended value, and no warning is printed.
**Why it happens:** `ManaSystem.GetSpellCost` silently falls back to `DefaultSpellCost` for unknown keys.
**Current state:** Many new combo keys are missing from `GameBalance.Mana.SpellCosts` dictionary. Only T1, some T2, and a subset of T3 have explicit costs. Missing entries:
  - T2: `queso|queso`, `queso|pepperoni`, `queso|piña` (not in dict)
  - T3: all new combos — `piña|piña|piña` and `queso|queso|queso` exist but most others don't
**How to avoid:** Add all 20 new combo keys to `SpellCosts` in `GameBalance.Mana.SpellCosts`.
**Warning signs:** No compile error — only visible via runtime mana costs being 30.

### Pitfall 3: Placed objects not registered as special cases in PlayerAimAndCast
**What goes wrong:** Placed object (area, pillar, wall) is routed through `HandleNormalAttack` → treated as a projectile at `castPoint.position` with `Initialize(direction)` called, which tries to give it velocity.
**Why it happens:** `HandleNormalAttack` is the catch-all path. Placed objects need an `else if (key == ...)` block before `HandleNormalAttack`.
**How to avoid:** All `queso`-starting placed combos and the `queso|queso|queso` black hole need their own dispatch blocks.
**Warning signs:** Object spawns then immediately flies off in cast direction instead of staying placed.

### Pitfall 4: T3 `piña|pepperoni` variants not dispatched specially
**What goes wrong:** `piña|pepperoni|piña` etc. go to `HandleNormalAttack` → `Initialize(direction)` → no teleport occurs, just a projectile.
**Why it happens:** These share the same non-projectile teleport mechanic as `piña|pepperoni`.
**How to avoid:** Add `else if` blocks for all three `piña|pepperoni|*` variants in `PlayerAimAndCast.OnCastSpell`.

### Pitfall 5: Absorbing sub-projectile T3 variants (piña|piña|queso) need a new sub-projectile type
**What goes wrong:** `piña|piña` spawns `SmallPiñaProjectile` sub-projectiles. For `piña|piña|queso`, the subs must absorb incoming projectiles — but `SmallPiñaProjectile` is a plain `CharacterProjectile`.
**Why it happens:** The splitter spawn loop instantiates a fixed prefab.
**How to avoid:** Create a new sub-projectile prefab + script (e.g. `SmallAbsorbingProjectile`) that extends `PineappleCheeseProjectile` with reduced stats. The parent T3 script sets a different `smallProjectilePrefab` reference.

### Pitfall 6: Wall with HP (`queso|queso`) — the HP system doesn't exist yet
**What goes wrong:** Nothing in the codebase currently tracks HP for a wall. `CheesePepperoniWall` uses `WallDuration` only.
**Why it happens:** This mechanic is new for this phase.
**How to avoid:** The `QuesoQuesoWall` script needs its own `currentHP` + `TakeDamage(float)` method, called from `OnTriggerEnter2D` when hit by an `EnemyProjectile` (after reflecting it). When HP <= 0, call `ForceDestroy()`.

### Pitfall 7: Black hole (`queso|queso|queso`) — most complex new mechanic
**What goes wrong:** No absorption-then-return-fire pattern exists for placed objects.
**Why it happens:** This is unique — stationary, absorbs projectiles for a duration, then fires them back at 1.5x damage.
**How to avoid:** Use a coroutine: Phase 1 = absorb (like `PineappleCheeseProjectile.AbsorbEnemyProjectiles` but in a placed object, store `absorbedCount`), Phase 2 = fire back (spawn `EnemyProjectile`-tagged projectiles toward nearest boss at accumulated damage × 1.5). No existing pattern to inherit from — implement from scratch as a standalone MonoBehaviour.

---

## Code Examples

### Adding GameBalance constants for a new combo
```csharp
// In GameBalance.cs, inside the Spells class:
public static class QuesoQueso
{
    public const float WallDuration = 5f;
    public const float WallHP = 50f;
    public const float TickDamage = 5f;
    public const float TickInterval = 0.5f;
    public const float ReflectionSpeedMultiplier = 1.2f;
    public const float ReflectionCooldown = 0.3f;
}
```

### Adding mana cost for a new combo
```csharp
// In GameBalance.Mana.SpellCosts dictionary:
{ "queso|queso", 22f },
{ "queso|pepperoni", 20f },
{ "queso|piña", 18f },
// ... (add all 20 new combos)
```

### Registering a new Handle* dispatch in PlayerAimAndCast (placed object)
```csharp
// In OnCastSpell, before the HandleNormalAttack fallthrough:
else if (key == "queso|queso")
{
    HandleQuesoQuesoWall(entry);
    return;
}
// ...
private void HandleQuesoQuesoWall(CombinationEntry entry)
{
    if (entry.attackPrefab == null) { combiner.ClearSelectedElements(); return; }
    // optional: destroy existing instance
    Vector3 aimDir = GetCurrentAimDirection();
    float dist = Pizzard.Core.GameBalance.Casting.QuesoQuesoWallDistance;
    Vector3 spawnPos = transform.position + aimDir.normalized * dist;
    GameObject go = Instantiate(entry.attackPrefab, spawnPos, Quaternion.identity);
    go.GetComponent<QuesoQuesoWall>()?.InitializeAtPosition(spawnPos);
    combiner.ClearSelectedElements();
}
```

### Sticky projectile T2 (`pepperoni|queso`) — follows PepperoniQuesoPepperoniAttack pattern exactly
```csharp
// PepperoniQuesoAttack.cs — subclass PepperoniQuesoPepperoniAttack OR duplicate pattern
// Key difference: this is T2 (simpler values), reads from GameBalance.Spells.PepperoniQueso.*
// The class already exists as PepperoniQuesoPepperoniAttack — pepperoni|queso T2 may just
// use the same or a minimal subclass with different GameBalance section.
```

### piña|piña T3 variant with modified sub-projectiles
```csharp
// PinaPinaPinaAttack.cs : piña_piñaAttack
// Override SpawnDiagonalProjectiles to use a new sub-prefab whose script also explodes.
// OR: have SpawnDiagonalProjectiles call base but set a different smallProjectilePrefab.
```

### Adding CombinationDatabase.asset entry (via Unity Inspector)
```
In Unity Editor → Project window → Assets/CombinationDatabase.asset
→ Combinations list → + → fill:
  combinationKey: "queso|queso"
  combinationName: "Muro de queso"
  description: "Un muro de queso que daña y refleja proyectiles"
  isUnlocked: true
  attackPrefab: [drag new prefab]
```

---

## Key Discoveries

### What already exists in CombinationDatabase.asset
Cross-referencing the YAML asset against the task list:

**Confirmed in asset (verified T2+T3 — need mechanic verification only):**
- `piña|piña` — entry + prefab exists
- `piña|pepperoni` — entry + prefab exists
- `piña|queso` — entry + prefab exists
- `pepperoni|piña` — entry + prefab exists
- `pepperoni|pepperoni` — entry + prefab exists
- `pepperoni|pepperoni|pepperoni` — entry + prefab exists
- `pepperoni|pepperoni|piña` — entry + prefab exists
- `pepperoni|pepperoni|queso` — entry + prefab exists
- `pepperoni|piña|pepperoni` — entry + prefab exists
- `pepperoni|piña|piña` — entry + prefab exists
- `pepperoni|piña|queso` — entry + prefab exists
- `pepperoni|queso|pepperoni` — entry + prefab exists
- `pepperoni|queso|piña` — entry + prefab exists
- `pepperoni|queso|queso` — entry + prefab exists
- `queso|queso|piña` — entry + prefab exists
- `queso|queso|pepperoni` — entry + prefab exists

**NOT in asset (need script + prefab + entry):**
- `queso|queso` — missing
- `queso|pepperoni` — missing
- `queso|piña` — missing
- `pepperoni|queso` — missing
- All 16 missing T3 combos — missing

### Naming convention (from codebase analysis)
Existing scripts follow: `{Element1}{Element2}Attack.cs` / `{Element1}{Element2}{Element3}Attack.cs`
- camelCase elements: `Queso`, `Pepperoni`, `Pina` (no accent in class names, file names use `Piña`)
- Examples: `PepperoniQuesoPepperoniAttack.cs`, `PepperoniPineapplePineappleAttack.cs`
- Placed objects in `Otros/`: `CheesePepperoniWall.cs`, `FireTrail.cs`
- New placed combos should go in `Otros/`, new projectiles in `Projectile/`

### BossBase.TakeDamage signature
```csharp
// BossBase (namespace Pizzard.Bosses): TakeDamage(int damage)
// PblobController (legacy): TakeDamage(float damage)
// CharacterProjectile uses both: BossBase first, PblobController as fallback
// New scripts should follow the same dual-path pattern as CharacterProjectile.
```

### PlayerAimAndCast dispatch order (current)
```
1. "queso"                → HandleCheeseShield
2. "queso|queso|piña"     → HandleStaticCheeseShield
3. "queso|queso|pepperoni" → HandleCheesePepperoniWall
4. "piña|pepperoni"       → HandleTeleportAttack
5. "pepperoni|piña|pepperoni" → HandlePepperoniPineapplePepperoniAttack
6. everything else        → HandleNormalAttack
```
New placed combos and `piña|pepperoni` T3 variants must be inserted BEFORE `HandleNormalAttack`.

### GameBalance.Casting — what's missing
Currently only `StaticShieldDistance`, `WallDistance`, `CatapultMinDistance` exist.
New constants needed for placed T2/T3 queso combos:
- `QuesoQuesoWallDistance` (queso|queso — placement offset)
- `QuesoPepperoniAreaDistance` (queso|pepperoni)
- `QuesoPinaaPillarDistance` (queso|piña)
- T3 variants inherit the same constants or define scaled variants

---

## Implementation Groupings (for planner wave organization)

**Wave 0 — Prerequisites (no tasks blocked, enables all others):**
1. Add all 20 missing keys to `GameBalance.Mana.SpellCosts`
2. Add all new `GameBalance.Spells.*` constant classes for new combos
3. Add new `GameBalance.Casting.*` placement distance constants

**Wave 1 — T2 Implementations (4 combos, relatively independent):**
- `pepperoni|queso` — sticky projectile (normal dispatch, subclass pattern)
- `queso|piña` — pillar placed object (new Handle* + new MonoBehaviour)
- `queso|pepperoni` — ground area placed object (new Handle* + new MonoBehaviour)
- `queso|queso` — wall with HP placed object (new Handle* + new MonoBehaviour with HP)

**Wave 2 — T3 Piña/Piña Variants (4 combos, share piña|piña base):**
- `piña|piña|piña` — sub-projectiles also explode (new sub-prefab/script)
- `piña|piña|queso` — sub-projectiles absorb (new sub-prefab using PineappleCheeseProjectile)
- `piña|piña|pepperoni` — sub-projectiles burn (new sub-prefab with StatusEffect)

**Wave 3 — T3 Piña/Queso Absorbing Variants (3 combos, share PineappleCheeseProjectile base):**
- `piña|queso|piña` — damage = absorbed count
- `piña|queso|queso` — becomes cone on hit
- `piña|queso|pepperoni` — burn stacks = absorbed count

**Wave 4 — T3 Piña/Pepperoni Teleport Variants (3 combos, share piña|pepperoni base):**
- `piña|pepperoni|piña` — explosion bonus
- `piña|pepperoni|pepperoni` — bigger explosion
- `piña|pepperoni|queso` — explosion reflects projectiles

**Wave 5 — T3 Queso Variants (7 combos, all placed objects extending T2 bases):**
- `queso|queso|queso` — black hole (most complex, standalone)
- `queso|piña|piña`, `queso|piña|queso`, `queso|piña|pepperoni` — pillar variants
- `queso|pepperoni|pepperoni`, `queso|pepperoni|piña`, `queso|pepperoni|queso` — area variants

**Wave 6 — Verification (16 existing combos):**
- Verify 5 T2 + 11 T3 existing combos: hit detection, correct damage, mana cost deducted

---

## Open Questions

1. **`queso|queso` (wall with HP) — single instance or stackable?**
   - What we know: `CheesePepperoniWall` uses `CurrentActiveWall` static for single-instance
   - What's unclear: Design doc says "has HP" but doesn't specify if casting again replaces or adds
   - Recommendation: Follow single-instance pattern (replace on recast), consistent with all other placed objects

2. **`piña|queso|queso` — "becomes a cone on hit" — what does a cone mean mechanically?**
   - What we know: No cone-damage helper exists; `Physics2D.OverlapCircleAll` is the only area check used
   - What's unclear: Cone angle, range, whether it creates a cone collider or uses a fan of raycasts
   - Recommendation: Use `Physics2D.OverlapCircleAll` at impact with a reduced radius but check angle to boss: `Vector2.Angle(toTarget, impactDir) < coneHalfAngle`. Planner should pick cone half-angle value for GameBalance.

3. **`queso|queso|queso` — "fires absorbed projectiles back" — which direction?**
   - What we know: No targeting system exists beyond `Physics2D.OverlapCircleAll`
   - What's unclear: Random direction? Toward nearest boss? Toward last known player aim?
   - Recommendation: At end of absorption phase, spawn `CharacterProjectile` instances toward nearest `Boss`-tagged object, one per absorbed projectile, at `absorbedDamage * 1.5f`.

4. **T3 Pillar variants (`queso|piña|queso` — slows projectiles) — how?**
   - What we know: No projectile-slowing mechanic exists
   - What's unclear: Reduce `rb.velocity.magnitude` on `EnemyProjectile` on trigger stay? Or modify speed field?
   - Recommendation: In `OnTriggerStay2D`, reduce `rb.velocity` by a multiplier per frame for `EnemyProjectile`-tagged objects.

---

## Sources

### Primary (HIGH confidence)
All findings verified directly from source files in `C:/Users/danir/Pizzard/Pizzard (2)/Assets/`:

- `Assets/Scripts/Player/PlayerAimAndCast.cs` — dispatch logic, Handle* method signatures
- `Assets/Scripts/ElementsAttack/Projectile/CharacterProjectile.cs` — base class contract
- `Assets/Scripts/ElementsAttack/Otros/CheesePepperoniWall.cs` — placed object pattern with reflection
- `Assets/Scripts/ElementsAttack/Otros/StaticCheeseShield.cs` — placed object with contact damage
- `Assets/Scripts/ElementsAttack/Otros/FireTrail.cs` — reusable ticking area
- `Assets/Scripts/ElementsAttack/Projectile/PineappleCheeseProjectile.cs` — absorbing projectile
- `Assets/Scripts/ElementsAttack/Projectile/PepperoniQuesoPepperoniAttack.cs` — sticky projectile
- `Assets/Scripts/ElementsAttack/Projectile/PepperoniPineapplePepperoniAttack.cs` — catapult base
- `Assets/Scripts/ElementsAttack/Projectile/PepperoniPineapplePineappleAttack.cs` — catapult inheritance
- `Assets/Scripts/ElementsAttack/Projectile/PepperoniPineappleQuesoAttack.cs` — catapult inheritance
- `Assets/Scripts/ElementsAttack/Projectile/piña_piñaAttack.cs` — splitter pattern
- `Assets/Scripts/ElementsAttack/Projectile/SmallStickyProjectile.cs` — sub-projectile sticky
- `Assets/Scripts/ElementsAttack/Otros/PineapplePepperoniAttack.cs` — teleport dispatch
- `Assets/Scripts/Core/GameBalance.cs` — all balance constants + SpellCosts dictionary
- `Assets/Scripts/DataBase/CombinationDatabase.cs` — ScriptableObject schema
- `Assets/CombinationDatabase.asset` — current registered combos (YAML)
- `Assets/Scripts/Bosses/BossBase.cs` — boss damage API

---

## Metadata

**Confidence breakdown:**
- Architecture patterns: HIGH — read directly from all major script files
- Missing combos list: HIGH — cross-referenced CONTEXT.md against CombinationDatabase.asset YAML
- GameBalance gaps (missing SpellCosts): HIGH — read the dictionary directly
- Open questions (cone mechanic, black hole direction): LOW — design doc not read; CONTEXT.md describes behavior but not implementation detail

**Research date:** 2026-03-01
**Valid until:** Stable — codebase is not rapidly changing; valid until any refactor of PlayerAimAndCast or CharacterProjectile
