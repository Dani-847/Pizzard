---
status: testing
phase: 18-boss1-pblob
source: 18-01-SUMMARY.md, 18-02-SUMMARY.md, 18-03-SUMMARY.md, 18-04-SUMMARY.md
started: 2026-03-01T12:00:00Z
updated: 2026-03-01T12:00:00Z
---

## Current Test

number: 1
name: Boss starts idle and invulnerable
expected: |
  Enter BossArena_1. Boss is alive but does nothing and cannot be damaged.
  Hitting it with a spell triggers the battle — boss starts moving.
awaiting: user response

## Tests

### 1. Boss starts idle and invulnerable
expected: Enter BossArena_1. Boss is alive but does nothing and cannot be damaged. Hitting it with a spell triggers the battle — boss starts moving.
result: pass

### 2. Phase 1 — Alternating chase and hairballs
expected: During Phase 1 the boss alternates: moves toward player (vulnerable, takes damage) → stands still and spawns static hairball obstacles (invulnerable). Hairballs appear at spawn points and don't move.
result: pass

### 3. Phase 2 triggers at 66% HP
expected: When boss HP drops to 66%, 3 circles appear in the lower arena (1 green, 2 red), move randomly for ~5 seconds, then stop.
result: pass

### 4. Phase 2 — Green circle makes boss vulnerable
expected: Walking into the green circle makes the boss vulnerable (damageable). Leaving it makes the boss invulnerable again.
result: pass

### 5. Phase 2 — Timer penalty
expected: A 30s timer runs. If the player is NOT standing in the green circle when it hits 0, player takes 2 damage. Circles restart.
result: [pending]

### 6. Phase 3 triggers at 33% HP
expected: At 33% HP, boss teleports to top of arena. Player is moved to bottom. A grid of colored tiles fills the arena.
result: [pending]

### 7. Phase 3 — Red tiles damage and slow
expected: Stepping on red tiles slows the player (half speed) and deals 1 damage per second. Green tiles are safe.
result: [pending]

### 8. Phase 3 — Boss contact vulnerability
expected: After grid phase ends (or while in Phase3_Combat), physically touching the boss makes it vulnerable. Stepping away makes it invulnerable again.
result: [pending]

### 9. Boss health bar
expected: A health bar is visible during the fight. It drains visibly as the boss takes damage.
result: [pending]

### 10. Pause mid-fight
expected: Pressing Escape during the boss fight pauses the game and shows a pause menu. Pressing Escape again (or resume) unpauses.
result: [pending]

### 11. Death → retry flow
expected: If the player dies during the boss fight, the death screen appears. Choosing retry/continue resets the run back to the shop.
result: [pending]

## Summary

total: 11
passed: 0
issues: 0
pending: 11
skipped: 0

## Gaps

[none yet]
