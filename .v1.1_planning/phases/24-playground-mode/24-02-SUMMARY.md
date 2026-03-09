---
phase: 24-playground-mode
plan: "02"
subsystem: UI/Menu
tags: [playground, menu, animation, unity-animator]
dependency_graph:
  requires: []
  provides: [playground-menu-entry, playground-pulse-animation]
  affects: [MenuUI, MainMenu]
tech_stack:
  added: []
  patterns: [Unity Animator state machine, in-memory flag for per-session pulse]
key_files:
  modified:
    - Assets/Scripts/UI/MenuUI.cs
  created:
    - Assets/Animations/UI/PlaygroundButtonPulse.anim
    - Assets/Animations/UI/PlaygroundButtonAnimator.controller
decisions:
  - Pulse is in-memory only (_playgroundPulseActive = true) — resets each launch, no PlayerPrefs
  - AnimatorUpdateMode.UnscaledTime ensures pulse continues at Time.timeScale = 0
metrics:
  duration: ~15 min
  completed: 2026-03-08
  tasks_completed: 2
  files_changed: 3
---

# Phase 24 Plan 02: Playground Menu Entry Summary

**One-liner:** Playground button added to MenuUI.cs with in-memory per-session pulse via Unity Animator controller (Idle/Pulsing states, Pulsing bool parameter).

## What Was Built

MenuUI.cs updated with `botonPlayground` and `playgroundButtonAnimator` fields. On Start(), if `_playgroundPulseActive` (always true at launch), the animator is set to UnscaledTime mode and `Pulsing` bool is enabled. `OnClickPlayground()` stops the pulse for that session and calls `SceneLoader.LoadScene("PlaygroundScene")`.

A Unity Animator Controller (`PlaygroundButtonAnimator.controller`) was created with:
- **Idle** state: default, no animation
- **Pulsing** state: references `PlaygroundButtonPulse.anim` — looping 0.8s scale 1.0→1.05→1.0 with smooth tangents
- **Pulsing bool** parameter drives transitions (Any→Pulsing on true, Pulsing→Idle on false), both 0.1s with no exit time

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add botonPlayground to MenuUI.cs | 77a05bf | Assets/Scripts/UI/MenuUI.cs |
| 2 | Create PlaygroundButton Animator Controller | 5ccafb8 | Assets/Animations/UI/PlaygroundButtonAnimator.controller, PlaygroundButtonPulse.anim |

## Deviations from Plan

None — plan executed exactly as written.

## Scene Wiring Note

The `playgroundButtonAnimator` field and `botonPlayground` field are Inspector-wirable but not yet wired in the MainMenu scene. Scene wiring is deferred to Plan 04 (scene assembly), as noted in the plan itself.

## Self-Check

- [x] Assets/Scripts/UI/MenuUI.cs contains botonPlayground, playgroundButtonAnimator, _playgroundPulseActive, OnClickPlayground
- [x] Assets/Animations/UI/PlaygroundButtonAnimator.controller exists with Pulsing bool parameter
- [x] Assets/Animations/UI/PlaygroundButtonPulse.anim exists with keyframes at 0s, 0.4s, 0.8s
- [x] No PlayerPrefs references added to MenuUI.cs
- [x] SceneLoader.LoadScene("PlaygroundScene") called in OnClickPlayground

## Self-Check: PASSED
