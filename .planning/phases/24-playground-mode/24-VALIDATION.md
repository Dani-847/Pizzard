---
phase: 24
slug: playground-mode
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-08
---

# Phase 24 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Runner (EditMode + PlayMode) |
| **Config file** | none — Unity built-in |
| **Quick run command** | `Unity Test Runner > EditMode` |
| **Full suite command** | `Unity Test Runner > All Tests` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Check Unity console for compile errors
- **After every plan wave:** Run full Unity Test Runner suite
- **Before `/gsd:verify-work`:** Full suite must be green + manual play test
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 24-01-01 | 01 | 1 | PLAY-01 | manual | Open PlaygroundScene in Editor | ❌ W0 | ⬜ pending |
| 24-01-02 | 01 | 1 | PLAY-02 | manual | Enter playground, verify 10 tokens, buy spell | ❌ W0 | ⬜ pending |
| 24-01-03 | 01 | 1 | PLAY-03 | manual | Cast spells at dummy, verify DPS counter updates | ❌ W0 | ⬜ pending |
| 24-01-04 | 01 | 1 | PLAY-04 | manual | Stand in falling projectile path, take damage | ❌ W0 | ⬜ pending |
| 24-01-05 | 01 | 1 | PLAY-05 | manual | Fresh launch: Playground button blinks | ❌ W0 | ⬜ pending |
| 24-01-06 | 01 | 1 | ONBD-01 | manual | Click Playground button: pulse stops, never blinks again | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Verify Unity console has zero compile errors before starting
- [ ] Confirm PlaygroundScene can be created fresh (no template required)

*Playground Mode is primarily a Unity scene/prefab task — most verification is manual play testing.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Playground button appears on main menu | PLAY-01 | Unity scene/UI verification | Enter Play Mode on MainMenu scene, confirm button visible |
| Playground tokens isolated from main game | PLAY-02 | Runtime state verification | Note main game token count, enter Playground, buy spell, exit, verify main tokens unchanged |
| DPS counter updates live | PLAY-03 | Runtime combat verification | Cast spells at dummy, observe DPS number updating each second |
| Falling projectile damages player | PLAY-04 | Runtime combat verification | Walk under falling projectile, verify HP decreases |
| Button blinks on first launch | PLAY-05 | PlayerPrefs state verification | Delete PlayerPrefs, launch, confirm pulse animation plays |
| Button stops blinking after first click | ONBD-01 | Persistent state verification | Click Playground button, exit, relaunch, confirm no pulse |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
