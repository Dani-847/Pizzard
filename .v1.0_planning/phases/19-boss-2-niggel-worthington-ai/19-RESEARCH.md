# Phase 19: Boss 2 — Niggel Worthington AI - Research

**Researched:** 2026-03-01
**Domain:** Unity 2D boss AI, state machine, projectile systems, HUD extension
**Confidence:** HIGH (all findings from direct codebase inspection)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Core Mechanic — CoinVault (Niggel HP)**
- Niggel's HP = CoinVault, starts at 200 coins
- HP bar shows a visible number (coin count) alongside the standard bar
- Player has a coin meter displayed below the mana bar — shows coins stolen so far
- Player hits Niggel → steals coins (damages CoinVault) → player gains momentum
- Goal: drain CoinVault to 0

**Player Momentum System**
- Every successful hit on Niggel: `playerMomentum += 1`
- `maxMomentum = 5`
- Per stack: +5% damage, +3% movement speed
- At max (5 stacks): +25% damage, +15% speed
- If player does NOT hit Niggel for 4 seconds: `playerMomentum = 0` (full reset)
- All values go to `GameBalance.cs`

**Enrage Thresholds**
Three thresholds trigger as CoinVault drops:

- Base (200–150 coins): Slow movement, coin bag throw (slow yellow circles toward player, destroy on wall), rare dash every 20s (arena-bounded)
- Enrage Level 1 (≤150 coins): +10% speed, coin bags move faster, spawns static black dot barriers in rows — block player projectiles only, player walks through
- Enrage Level 2 (≤100 coins): Dash cooldown reduced + distance slightly increased (arena-bounded), can throw healing coin (fast yellow pulsating orange) — heals Niggel 30 coins on player hit
- Enrage Level 3 (≤50 coins): +20% speed, coin shield (translucent yellow pulsating blue) then bursts all-direction orange coin projectiles, dash used more frequently

**Thresholds are one-way** — healing above a threshold does NOT revert enrage phase.

**Projectile Visuals**
- Coin bag: yellow circle, slow arc, destroys on wall contact
- Healing coin: fast, yellow body pulsating orange, flies toward player
- Coin shield burst: orange coins fired in all directions (spread pattern)
- Black dot barriers (Enrage 1): static, row-based placement, player projectile blockers only

**Balance Constraints**
- All tunable values go in `GameBalance.Bosses.Niggel` — no magic numbers in controller
- No default attack damages exceed 2 DMG
- Enrage thresholds are one-way

### Claude's Discretion
- Exact number of black dot barriers per row and row count (tune for feel)
- Coin shield activation frequency at Enrage 3 (tune for pressure vs fairness)
- Momentum reset timer implementation (coroutine vs Update timer)
- Coin bag arc trajectory (straight vs slight curve)
- Whether dash direction avoids arena edges via raycast or fixed-direction logic

### Deferred Ideas (OUT OF SCOPE)
- None — discussion stayed within phase scope
</user_constraints>

---

## Summary

Phase 19 implements the full Niggel Worthington boss fight in `NiggelController.cs`, which already extends `BossBase` and has three stubbed attack methods. The fight is a role-reversal: the player gets stronger as they deal damage (momentum system), while Niggel escalates through four one-way enrage tiers as his CoinVault depletes. All boss state logic in this project uses coroutines and boolean flags, following the pattern established by `PblobController.cs`.

The primary technical work is: (1) replacing `BossBase`'s integer HP with a CoinVault integer, (2) implementing a `playerMomentum` system that reads from `NiggelController.TakeDamage()`, (3) wiring three projectile types via `EnemyProjectile` subclasses, (4) implementing the one-way enrage state machine, and (5) extending the HUD with a coin meter UI element below the mana bar and making the boss health bar display the raw coin count.

The `nyquist_validation` workflow flag is `false` — no test infrastructure section is needed.

**Primary recommendation:** Strip `NiggelController` of `BossBase` inheritance and rewrite it as a self-contained MonoBehaviour modelled after `PblobController`, which holds its own health integer, state enum, and coroutine-driven attack routines. `BossBase.TakeDamage(int)` does not support the CoinVault design (float/int coin semantics, momentum callback, one-way enrage) and will need to be bypassed regardless.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Unity 2D (Rigidbody2D, Collider2D) | In-project | Boss movement, projectile physics | All existing bosses use it |
| TextMesh Pro (TMP) | In-project | Coin count text on HP bar, coin meter | Used by `BossHealthBarUI.healthText` already |
| UnityEngine.UI (Image) | In-project | Fill bars for boss HP and player coin meter | `BossHealthBarUI` and `ManaUI` both use it |
| Coroutines (IEnumerator) | Built-in | Attack routines, enrage transitions, pulse VFX | Every boss routine in project uses coroutines |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `ProgressionManager.SpendCurrency()` | In-project | Deduct coins from player's wallet on steal | Only for old steal mechanic — see Architecture note |
| `PlayerHPController.RecibirDaño()` | In-project | Deal damage to player from projectile hits | Called from EnemyProjectile subclasses |
| `GameFlowManager.AvanzarFase()` | In-project | Advance game flow on boss defeat | Call from `Die()` override |
| `SoundManager` | In-project | Boss music on battle start | Pattern from PblobController |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Self-contained MonoBehaviour (like Pblob) | Keep BossBase inheritance | BossBase.TakeDamage(int) cannot inject momentum or coin semantics — you'd need to override it completely anyway; cleaner to mirror Pblob's standalone approach |
| Update-based momentum decay timer | Coroutine timer | Both work; Update timer is simpler for a single float countdown |

---

## Architecture Patterns

### Recommended Project Structure

```
Assets/Scripts/Bosses/
├── NiggelController.cs          # Full boss AI (rewrite from stub)
├── BossBase.cs                  # Leave untouched — Niggel may not extend it
Assets/Scripts/Bosses/Projectiles/
├── CoinBagProjectile.cs         # EnemyProjectile subclass — yellow circle
├── HealingCoinProjectile.cs     # EnemyProjectile subclass — heals Niggel on player hit
├── CoinShieldBurst.cs           # Spawner — fires N orange coins in spread
Assets/Scripts/Bosses/Barriers/
├── BlackDotBarrier.cs           # Static collider, only blocks PlayerProjectiles layer
Assets/Scripts/Core/
├── GameBalance.cs               # Expand Niggel section (see Code Examples)
Assets/Scripts/UI/
├── NiggelCoinMeterUI.cs         # New — player's coin meter below mana bar
├── BossHealthBarUI.cs           # Extend — display raw coin number (already has healthText TMP)
```

### Pattern 1: Enrage State Machine (one-way thresholds)

**What:** Integer `enrageLevel` (0–3) checked in `TakeDamage()` after every CoinVault change. Once a threshold is crossed the level only increments; healing above a threshold does not revert it.

**When to use:** Anytime thresholds must be permanent.

```csharp
// Source: project pattern (PblobController.CheckPhaseThresholds)
private int enrageLevel = 0;
private int coinVault;

public void TakeDamage(int coins)
{
    if (isDead) return;
    coinVault = Mathf.Max(0, coinVault - coins);

    // Momentum callback
    OnPlayerHitNiggel();

    // One-way threshold checks
    if (enrageLevel < 1 && coinVault <= GameBalance.Bosses.Niggel.Enrage1Threshold)
        EnterEnrage(1);
    else if (enrageLevel < 2 && coinVault <= GameBalance.Bosses.Niggel.Enrage2Threshold)
        EnterEnrage(2);
    else if (enrageLevel < 3 && coinVault <= GameBalance.Bosses.Niggel.Enrage3Threshold)
        EnterEnrage(3);

    // Broadcast to UI
    OnHealthChanged?.Invoke(coinVault);

    if (coinVault <= 0) Die();
}
```

### Pattern 2: Player Momentum System

**What:** `playerMomentum` int field increments on every player hit; resets to 0 if 4 seconds pass without a hit. Applied as multipliers to `PlayerController.moveSpeed` and spell damage.

**When to use:** Applied in `NiggelController.OnPlayerHitNiggel()`. Reset via a coroutine or Update timer in `NiggelController` (Claude's discretion).

```csharp
// Coroutine approach (recommended — cleaner than Update timer)
private int playerMomentum = 0;
private Coroutine momentumResetCoroutine;

private void OnPlayerHitNiggel()
{
    playerMomentum = Mathf.Min(playerMomentum + 1, GameBalance.Bosses.Niggel.MaxMomentum);
    ApplyMomentumToPlayer();

    if (momentumResetCoroutine != null) StopCoroutine(momentumResetCoroutine);
    momentumResetCoroutine = StartCoroutine(MomentumResetTimer());
}

private IEnumerator MomentumResetTimer()
{
    yield return new WaitForSeconds(GameBalance.Bosses.Niggel.MomentumResetDelay);
    playerMomentum = 0;
    ApplyMomentumToPlayer();
}

private void ApplyMomentumToPlayer()
{
    // Read base values from GameBalance, apply multipliers per stack
    float speedBonus = 1f + (playerMomentum * GameBalance.Bosses.Niggel.MomentumSpeedPerStack);
    // Apply to PlayerController.moveSpeed or a combat modifier
}
```

**Important integration point:** `PlayerController.moveSpeed` is a public field. A `NiggelMomentumModifier` approach (set a multiplier that `PlayerController.FixedUpdate` reads) keeps separation clean.

### Pattern 3: EnemyProjectile Subclasses

**What:** Existing `EnemyProjectile` base class auto-tags `"EnemyProjectile"`, sets `rb.velocity = transform.right * speed` in `Start()`, damages player on trigger, destroys on other collisions.

**When to use:** All three Niggel projectile types extend it.

```csharp
// CoinBagProjectile.cs — extends EnemyProjectile
// Set speed low (GameBalance), override OnTriggerEnter2D to also destroy on "Wall" tag
public class CoinBagProjectile : EnemyProjectile
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall")) { Destroy(gameObject); return; }
        base.OnTriggerEnter2D(other);
    }
}

// HealingCoinProjectile.cs — flies toward player, heals Niggel on player hit
public class HealingCoinProjectile : EnemyProjectile
{
    public NiggelController boss;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss?.HealCoinVault(GameBalance.Bosses.Niggel.HealingCoinAmount);
            Destroy(gameObject);
            return;
        }
        base.OnTriggerEnter2D(other);
    }
}
```

### Pattern 4: Black Dot Barriers (Enrage 1)

**What:** Static GameObjects with `Collider2D` set to only block the `PlayerProjectiles` layer. Player can walk through (no physics layer collision with Player layer). Spawned in rows at Enrage 1; destroyed on enrage 2 transition or boss defeat.

**When to use:** Enrage Level 1 entry (`EnterEnrage(1)`).

```csharp
// BlackDotBarrier.cs
// In Awake: Physics2D.IgnoreLayerCollision(thisLayer, playerLayer, true)
// Collider triggers only if hit by PlayerProjectiles layer — then destroy self
// Result: player walks through, spells are blocked
```

**Layer setup required:** Confirm `PlayerProjectiles` layer exists (it does — referenced in `PblobController.SpawnPhase2Circles()`).

### Pattern 5: Coin Shield Burst (Enrage 3)

**What:** Boss turns translucent yellow (SpriteRenderer color tint) + pulsates blue (lerp coroutine), then instantiates N orange projectiles in a spread pattern around itself.

**When to use:** On a cooldown timer at Enrage 3. Frequency is Claude's discretion.

```csharp
private IEnumerator CoinShieldRoutine()
{
    // Phase 1: Show shield VFX (color lerp on spriteRenderer)
    yield return ShieldChargeCoroutine(GameBalance.Bosses.Niggel.ShieldChargeDuration);

    // Phase 2: Burst orange coins in spread
    int count = GameBalance.Bosses.Niggel.ShieldBurstCount;
    for (int i = 0; i < count; i++)
    {
        float angle = (360f / count) * i;
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        // Instantiate coin projectile, rotate to dir
    }
}
```

### Pattern 6: Rich Dash (arena-bounded)

**What:** Boss dashes in a direction across the arena. Must clamp end position to arena bounds. Same clamp logic as PblobController (arenaCenter ± arenaClampX/Y).

```csharp
// Pick direction, compute target, clamp to arena
Vector2 dashDir = (playerTransform.position - transform.position).normalized;
Vector3 rawTarget = transform.position + (Vector3)(dashDir * GameBalance.Bosses.Niggel.DashDistance);
rawTarget.x = Mathf.Clamp(rawTarget.x, arenaCenter.x - arenaClampX, arenaCenter.x + arenaClampX);
rawTarget.y = Mathf.Clamp(rawTarget.y, arenaCenter.y - arenaClampY, arenaCenter.y + arenaClampY);

// Move via coroutine over dashDuration seconds
```

Dash direction options per Claude's discretion: raycast-based avoidance or fixed clamp (fixed clamp is simpler and matches Pblob's approach).

### Anti-Patterns to Avoid

- **Using BossBase.TakeDamage(int) directly:** It doesn't know about CoinVault, momentum, or enrage. Override completely or bypass; don't call `base.TakeDamage()`.
- **Magic numbers in NiggelController:** Every constant (coin amounts, speeds, thresholds, timing) must live in `GameBalance.Bosses.Niggel`.
- **Healing reverting enrage:** Enrage level is a one-way integer. `HealCoinVault()` only increments `coinVault`; it never touches `enrageLevel`.
- **Player coin meter in BossHealthBarUI:** Keep them separate — BossHealthBarUI tracks Niggel's CoinVault; NiggelCoinMeterUI tracks player's stolen coins. They're different values.
- **Coroutine leak on attack interruption:** Each attack coroutine must guard against `isDead` and store its ref so it can be stopped on state change (pattern: `if (attackCoroutine != null) StopCoroutine(attackCoroutine)`).

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Projectile hitting player | Custom collision script | `EnemyProjectile` subclass + tag system | Already handles Boss/EnemyProjectile exclusion, player damage, and self-destruct |
| Bounce/pulse VFX | Shader or particle system | SpriteRenderer color lerp coroutine | Matches project's visual style (PblobController uses color changes for state) |
| Arena bounds clamping | Raycast-based pathfinding | Mathf.Clamp on arenaCenter ± clamp values | Pblob already validated this approach; simple and reliable |
| HUD layout | New Canvas | Add UI element to existing boss arena canvas | UI.prefab and BossArena scenes already have a canvas hierarchy |

---

## Common Pitfalls

### Pitfall 1: CoinVault vs. Player Currency Confusion

**What goes wrong:** The existing `Attack3_StealStats()` stub calls `ProgressionManager.Instance.SpendCurrency()` to "steal" from the player. This is the *old design* — the new design has Niggel's HP *as* the CoinVault (a separate integer on NiggelController). Player currency (BossCurrency in ProgressionManager) is unrelated to this fight.

**Why it happens:** The stub predates the CONTEXT.md decision that CoinVault = Niggel's HP.

**How to avoid:** Delete or gut `Attack3_StealStats()`. The steal mechanic is now implicit: player hits Niggel → `TakeDamage()` drains CoinVault → player gains momentum. There is no "steal" attack; that code is vestigial.

**Warning signs:** If `SpendCurrency()` is called during the fight, something is wrong.

### Pitfall 2: BossBase.maxHealth Mismatch

**What goes wrong:** `BossBase` has `[SerializeField] protected int maxHealth = 100`. NiggelController inherits this. But CoinVault starts at 200 and is conceptually a coin count, not a generic HP value. If you use `base.TakeDamage()` it subtracts from `currentHealth` (the inherited int), not from your coin tracking.

**How to avoid:** Either (a) keep BossBase inheritance but override `TakeDamage()` entirely and maintain a separate `int coinVault` field, or (b) switch to a standalone MonoBehaviour like PblobController. Option (b) is cleaner.

**Warning sign:** `BossHealthBarUI` polls `trackedBoss.CurrentHealthPublic` (the inherited `currentHealth`). If CoinVault is stored separately, BossHealthBarUI won't update correctly — you'll need to either synchronize `currentHealth` with `coinVault` or extend BossHealthBarUI to accept a direct integer feed.

### Pitfall 3: One-Way Enrage Guard Missing

**What goes wrong:** If `HealCoinVault()` re-crosses a threshold (e.g., healing coin pushes vault from 95 back to 110), and the enrage check is a simple `if (coinVault <= 100)`, it might fire `EnterEnrage(2)` again and reset cooldowns or re-spawn barriers.

**How to avoid:** Always gate enrage with `if (enrageLevel < N && coinVault <= threshold)`. Never check enrage inside `HealCoinVault()`.

### Pitfall 4: Momentum Applied to Wrong Property

**What goes wrong:** Directly setting `PlayerController.moveSpeed` is fragile — if anything else reads or resets it (e.g., GameFlowManager, scene reload), the momentum bonus persists or corrupts.

**How to avoid:** Maintain a `float momentumSpeedMultiplier` in a separate script (or on NiggelController), and either patch it into PlayerController.Update via a public property or use a `NiggelMomentumModifier` MonoBehaviour on the player. Clear all modifiers in `Die()`.

### Pitfall 5: Black Dot Barriers Blocking Player Movement

**What goes wrong:** If the barrier's Collider2D layer collision is misconfigured, the player gets physically blocked instead of walking through.

**How to avoid:** In `BlackDotBarrier.Awake()`, call:
```csharp
Physics2D.IgnoreLayerCollision(
    LayerMask.NameToLayer("BossBarrier"),   // new layer
    LayerMask.NameToLayer("Default"),        // player layer
    true);
```
Only the `PlayerProjectiles` layer should collide with barriers.

### Pitfall 6: Healing Coin Healing Above CoinVault Max

**What goes wrong:** `HealCoinVault(30)` could push coinVault above 200.

**How to avoid:** `coinVault = Mathf.Min(coinVault + amount, GameBalance.Bosses.Niggel.CoinVaultMax)`.

---

## Code Examples

### GameBalance.Bosses.Niggel — Expanded Section

```csharp
// Source: Assets/Scripts/Core/GameBalance.cs — expand existing Niggel class
public static class Niggel
{
    // CoinVault (boss HP)
    public const int CoinVaultMax = 200;
    public const int Enrage1Threshold = 150;
    public const int Enrage2Threshold = 100;
    public const int Enrage3Threshold = 50;

    // Movement
    public const float BaseMoveSpeed = 2.5f;
    public const float Enrage1SpeedBonus = 0.1f;   // +10%
    public const float Enrage3SpeedBonus = 0.2f;   // +20%

    // Dash
    public const float BaseDashCooldown = 20f;
    public const float Enrage2DashCooldown = 12f;
    public const float DashDistance = 5f;
    public const float DashDuration = 0.25f;

    // Coin bag (Attack 1)
    public const float CoinBagSpeed = 3f;
    public const float Enrage1CoinBagSpeed = 5f;
    public const float CoinBagDamage = 1f;

    // Healing coin (Enrage 2+)
    public const float HealingCoinSpeed = 7f;
    public const int HealingCoinAmount = 30;

    // Coin shield burst (Enrage 3)
    public const float ShieldCooldown = 8f;
    public const float ShieldChargeDuration = 1.5f;
    public const int ShieldBurstCount = 8;
    public const float ShieldBurstDamage = 2f;

    // Momentum (player side)
    public const int MaxMomentum = 5;
    public const float MomentumResetDelay = 4f;
    public const float MomentumDamagePerStack = 0.05f;   // +5% per stack
    public const float MomentumSpeedPerStack = 0.03f;    // +3% per stack

    // Legacy (keep for compatibility)
    public const float AttackInterval = 3f;
    public const float StealRange = 2.5f;
    public const int CurrencyStealAmount = 10;
    public const float SpeedBuffPerSteal = 0.2f;
}
```

### NiggelCoinMeterUI — Player Stolen Coin Meter

```csharp
// Source: pattern from ManaUI.cs (vertical bar approach)
// Lives below mana bar in HUD canvas
// Polls NiggelController for (CoinVaultMax - currentCoinVault) = player's stolen coins
public class NiggelCoinMeterUI : MonoBehaviour
{
    public Image fillBar;
    public TMPro.TextMeshProUGUI coinCountText;

    private NiggelController boss;

    private void OnEnable()
    {
        boss = FindObjectOfType<NiggelController>();
    }

    private void Update()
    {
        if (boss == null) return;
        int stolen = boss.CoinVaultMax - boss.CurrentCoinVault;
        float ratio = (float)stolen / boss.CoinVaultMax;
        if (fillBar != null) fillBar.fillAmount = ratio;
        if (coinCountText != null) coinCountText.text = stolen.ToString();
    }
}
```

### BossHealthBarUI — Display Raw Coin Count

The existing `BossHealthBarUI.UpdateBar()` already does:
```csharp
healthText.text = $"{trackedBoss.CurrentHealthPublic}/{cachedMaxHealth}";
```
If `NiggelController` synchronizes `currentHealth` (the inherited field) with `coinVault` on each damage, the existing UI will display the coin count with no changes. This is the lowest-friction approach.

---

## Integration Map

| Integration Point | Existing Code | What Phase 19 Adds |
|-------------------|---------------|-------------------|
| `NiggelController.TakeDamage()` | Stub in BossBase | Full override: drain coinVault, trigger momentum, check enrage |
| `GameBalance.Bosses.Niggel` | 4 constants | ~15 new constants (see Code Examples) |
| `BossHealthBarUI` | Polls `CurrentHealthPublic` | Optionally displays coin count via healthText (already wired if currentHealth = coinVault) |
| HUD Canvas (BossArena_2.unity) | HP hearts, mana bar | New NiggelCoinMeterUI element below mana bar |
| `EnemyProjectile` subclasses | Base class exists | 3 new subclasses: CoinBag, HealingCoin, CoinShieldBurst |
| Arena bounds clamping | Pattern from Pblob | Dash and movement clamp to arenaCenter ± arenaClampX/Y |
| Player layer collision | PlayerProjectiles layer exists | New BossBarrier layer for black dot barriers |
| Boss defeat | BossBase.Die() → AvanzarFase() | Override Die() to also clear momentum modifiers |

---

## Open Questions

1. **Which BossArena scene hosts Niggel?**
   - What we know: There are `BossArena_1.unity` through `BossArena_4.unity`. Niggel is Boss 2 (phase 19). Pblob (Boss 1) used `BossArena_1`.
   - What's unclear: Whether `BossArena_2.unity` already has the Niggel prefab and canvas set up, or is blank.
   - Recommendation: Open `BossArena_2.unity` in first plan task to inspect existing canvas/prefab placement before writing UI code.

2. **Does `NiggelController` need to keep `BossBase` inheritance?**
   - What we know: BossBase provides `OnBossDefeated` event, `AddCurrency` via `ProgressionManager`, and `isDead` flag. These are useful.
   - What's unclear: Whether keeping BossBase and overriding `TakeDamage()` is cleanest vs. mirroring Pblob's self-contained approach.
   - Recommendation: Keep BossBase inheritance (less refactor risk). Override `TakeDamage(int)` completely — maintain `int coinVault` alongside inherited `currentHealth`, setting `currentHealth = coinVault` on each hit so `BossHealthBarUI` stays compatible.

3. **How does the player momentum multiplier integrate with spell damage?**
   - What we know: Player spells inherit from `CharacterProjectile` or combo scripts; there's no global damage multiplier field.
   - What's unclear: Whether a `NiggelMomentumModifier` MonoBehaviour on the player is feasible or if a static float on `NiggelController` is simpler.
   - Recommendation: Use a `public static float PlayerDamageMultiplier` on NiggelController (set to 1.0 normally, set to momentum-adjusted value during fight). Spell scripts can read it during the boss fight. Reset to 1.0 in `Die()`.

---

## Sources

### Primary (HIGH confidence)
- Direct codebase inspection — `NiggelController.cs`, `BossBase.cs`, `PblobController.cs`, `EnemyProjectile.cs`, `GameBalance.cs`, `BossHealthBarUI.cs`, `ManaUI.cs`, `PlayerController.cs`, `PlayerHPController.cs`, `ProgressionManager.cs`, `CharacterHPUI.cs`
- CONTEXT.md (phase 19) — all design decisions

### Secondary (MEDIUM confidence)
- Phase 18 summary (18-05-SUMMARY.md) — confirmed arena bounds, layer collision, and coroutine patterns still active

### Tertiary (LOW confidence)
- None

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — verified from existing code; all libraries in use
- Architecture patterns: HIGH — directly derived from PblobController and EnemyProjectile patterns
- Pitfalls: HIGH — CoinVault/BossBase mismatch identified from direct code reading

**Research date:** 2026-03-01
**Valid until:** 2026-04-01 (project codebase is stable between phases)
