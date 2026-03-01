# Phase 20: Spell Polish — Tier 1 - Context

**Gathered:** 2026-03-01
**Status:** Ready for planning

<domain>
## Phase Boundary

Verify the 3 Tier 1 spells (Queso/CheeseShield, Pepperoni, Piña) are mechanically correct and their values match GameBalance.cs. No VFX, no SFX, no animations — those are v2.0 scope.

</domain>

<decisions>
## Implementation Decisions

### Scope
- **VFX deferred to v2** — no particle effects, no visual trails, no impact animations
- **SFX deferred to v2** — no sound clips, no cast/impact audio hookup
- Polish = mechanics only: damage values, mana costs, status effect application, GameBalance alignment

### Per-Spell Verify Checklist
- Queso (CheeseShield): orbits correctly, reflects projectiles, contact damage ticks match GameBalance, shield duration correct, mana cost deducted
- Pepperoni (PepperoniAttack): damage matches GameBalance.Spells.Pepperoni, burn DoT applies correct stacks/duration, mana cost deducted
- Piña (PiñaAttack): projectile speed correct, damage matches GameBalance, mana cost deducted

### Claude's Discretion
- Any minor bug fixes discovered during verification
- Order of spell verification

</decisions>

<specifics>
## Specific Ideas

No specific requirements — verification pass only.

</specifics>

<deferred>
## Deferred Ideas

- VFX: particle effects (spawn, travel, impact) for all 3 spells — v2.0
- SFX: cast and impact sound clips — v2.0
- Burn indicator visual on boss when Pepperoni DoT is active — v2.0
- Animations for all spells — v2.0

</deferred>

---

*Phase: 20-spell-polish-tier-1*
*Context gathered: 2026-03-01*
