# Phase 19: Boss 2 — Niggel Worthington - Context

**Gathered:** 2026-03-01
**Status:** Ready for planning

<domain>
## Phase Boundary

Full implementation of the Niggel Worthington boss fight. Core loop: player hits Niggel → player gains momentum buffs AND steals coins (Niggel's HP). Niggel's HP draining causes him to enrage through 3 thresholds. New mechanics only — no UI overhaul, no progression changes beyond this fight.

</domain>

<decisions>
## Implementation Decisions

### Core Mechanic — CoinVault (Niggel HP)
- Niggel's HP = **CoinVault**, starts at **200 coins**
- HP bar shows a **visible number** (coin count) alongside the standard bar
- Player has a **coin meter displayed below the mana bar** — shows coins stolen so far
- Player hits Niggel → steals coins (damages CoinVault) → player gains momentum
- Goal: drain CoinVault to 0

### Player Momentum System
- Every successful hit on Niggel: `playerMomentum += 1`
- `maxMomentum = 5`
- Per stack: **+5% damage, +3% movement speed**
- At max (5 stacks): **+25% damage, +15% speed**
- If player does NOT hit Niggel for **4 seconds**: `playerMomentum = 0` (full reset)
- All values go to `GameBalance.cs`

### Enrage Thresholds
Three thresholds trigger as CoinVault drops:

**Base (200–150 coins):**
- Niggel moves slowly
- Throws coin bags: yellow circles, launched slowly toward player, disappear on wall contact
- Rare dash: every 20s, short dash (must stay inside arena bounds)

**Enrage Level 1 (≤150 coins):**
- Niggel gets irritated
- +10% movement speed
- Coin bag projectiles move faster
- Spawns randomly placed rows of static black dots — barriers that **block player projectiles only** (player can walk through)

**Enrage Level 2 (≤100 coins):**
- Niggel gets angry
- Dash cooldown reduced
- Dash distance slightly increased (must remain arena-bounded)
- Can now throw a **healing coin**: fast yellow projectile pulsating orange — if it hits the player, **heals Niggel for 30 coins**
- Healing above 100 coins does NOT revert the enrage phase (one-way thresholds)

**Enrage Level 3 (≤50 coins):**
- Niggel is furious
- +20% movement speed
- **Coin shield**: occasionally wraps himself in a translucent yellow shield pulsating blue → then bursts all-direction orange coin projectiles
- Dash used more frequently

### Projectile Visuals
- **Coin bag**: yellow circle, slow arc, destroys on wall contact
- **Healing coin**: fast, yellow body pulsating orange, flies toward player
- **Coin shield burst**: orange coins fired in all directions (spread pattern)
- **Black dot barriers** (Enrage 1): static, row-based placement, player projectile blockers only

### Balance Constraints
- All tunable values go in `GameBalance.Bosses.Niggel` — no magic numbers in controller
- No default attack damages exceed **2 DMG**
- Enrage thresholds are one-way (healing above a threshold does not un-enrage)

### Claude's Discretion
- Exact number of black dot barriers per row and row count (tune for feel)
- Coin shield activation frequency at Enrage 3 (tune for pressure vs fairness)
- Momentum reset timer implementation (coroutine vs Update timer)
- Coin bag arc trajectory (straight vs slight curve)
- Whether dash direction avoids arena edges via raycast or fixed-direction logic

</decisions>

<specifics>
## Specific Ideas

- The fight is a **role-reversal**: normally bosses get stronger over time, here the *player* gets stronger while Niggel panics
- Enrage levels are **one-way** — Niggel cannot calm down once threshold crossed, even if healing coin restores coins above threshold
- Niggel's HP bar must show the **raw coin number** visibly (not just a bar fill)
- Player coin meter lives **below the mana bar** in the HUD

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- `NiggelController.cs`: has `AttackRoutine()` coroutine, `speedMultiplier` field, `Attack1_ThrowMoney()` / `Attack2_RichDash()` / `Attack3_StealStats()` stubs — all need real implementation
- `ProgressionManager.SpendCurrency()`: used in steal stub — still relevant for coin meter tracking
- `EnemyProjectile.cs`: base class for coin bag and healing coin projectiles
- `PlayerHPController.RecibirDaño()`: established damage path
- `GameBalance.Bosses.Niggel`: has `AttackInterval`, `StealRange`, `CurrencyStealAmount`, `SpeedBuffPerSteal` — needs expansion

### Established Patterns
- All balance values in `GameBalance.cs` static classes — follow same pattern
- Boss state via boolean flags and coroutines (see `PblobController` for reference)
- Enemy projectiles extend `EnemyProjectile`, set tag `"EnemyProjectile"`

### Integration Points
- Player HUD: coin meter needs a new UI element below mana bar
- BossHealthBarUI: needs to display raw coin number alongside fill bar
- `NiggelController.TakeDamage()`: must trigger momentum gain + enrage threshold checks

</code_context>

<deferred>
## Deferred Ideas

- None — discussion stayed within phase scope

</deferred>

---

*Phase: 19-boss-2-niggel-worthington-ai*
*Context gathered: 2026-03-01*
