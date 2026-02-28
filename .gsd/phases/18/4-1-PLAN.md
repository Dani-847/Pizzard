---
phase: 18
plan: 4
wave: 1
---

# Plan 18.4.1 — Health Bar Fix & Debug Key Removal

## Objective
Fix the boss health bar so it correctly shows HP drain when the boss is hit, remove duplicate child objects, and delete the debug keyboard shortcuts that shouldn't ship.

## Context
- `Assets/Scripts/Pblob/DelayedHealthBar.cs`
- `Assets/Scripts/Pblob/PblobUI.cs`
- `Assets/Scripts/Core/GameBalance.cs`
- `Assets/Prefabs/Base/UI.prefab` — contains PblobUI > BackHealthBar, FrontHealthBar, PblobBorderHealthBar

## Tasks

<task type="auto">
  <name>Rebuild DelayedHealthBar — clear duplicates, use stable names</name>
  <files>Assets/Scripts/Pblob/DelayedHealthBar.cs</files>
  <action>
    Complete rewrite of DelayedHealthBar using ManaUI's approach:
    1. In Start(): Call ClearBars() which destroys ALL children EXCEPT "PblobBorderHealthBar"
    2. CreateBar("HealthBackBar", OrangeColor, border) — orange, placed first (lower render order)
    3. CreateBar("HealthFrontBar", RedColor, border) — red, placed second (renders over orange)
    4. PblobBorderHealthBar.SetAsLastSibling() — border renders over both
    5. Both bars: anchorMin=(0,0), anchorMax=(1,1), offsetMin/Max=Vector2.zero, sprite=null, type=Filled, fillMethod=Horizontal, fillOrigin=Left, fillAmount=1f
    6. Guard: `if (frontImg != null) return;` at top of BuildBars() to prevent duplicate creation
    7. SetHealth(float ratio): frontImg.fillAmount = ratio immediately; start DrainBack coroutine for backImg
    8. DrainBack: WaitForSeconds(GameBalance.Bosses.Pblob.HealthBarDrainDelay), then MoveTowards at GameBalance.Bosses.Pblob.HealthBarDrainSpeed per second
    - Do NOT use anchorMax.x clipping — that only works without sprites, was tried and failed
    - Do NOT call BuildBars() from SetHealth() — only call from Start() and guard with null check
  </action>
  <verify>Play BossArena_1 → check hierarchy: PblobUI has exactly 3 children: HealthBackBar, HealthFrontBar, PblobBorderHealthBar</verify>
  <done>No duplicates. Health bar visible (full red bar at start). Taking damage shrinks red bar; orange bar drains slowly after ~0.5s</done>
</task>

<task type="auto">
  <name>Fix PblobUI init order — remove redundant UpdateHealthBar(1f) from Start()</name>
  <files>Assets/Scripts/Pblob/PblobUI.cs</files>
  <action>
    Remove the `UpdateHealthBar(1f)` call from PblobUI.Start() entirely.
    Reason: DelayedHealthBar.Start() already initializes bars to fillAmount=1f.
    Calling SetHealth(1f) from PblobUI.Start() would restart coroutines unnecessarily.
    Keep the boss reference + event subscription logic in OnEnable/Start.
  </action>
  <verify>Compile with no errors. No double-init of health bar at scene load.</verify>
  <done>PblobUI.Start() has no UpdateHealthBar call — health bar starts full via DelayedHealthBar.Start() alone</done>
</task>

<task type="auto">
  <name>Remove debug keys from PblobController + add GameBalance constants</name>
  <files>Assets/Scripts/Pblob/PblobController.cs, Assets/Scripts/Core/GameBalance.cs</files>
  <action>
    In PblobController.cs:
    1. Delete entire [Header("DEBUG")] section: `debugMode`, `damageKey`, `nextPhaseKey` fields
    2. Delete the entire `Update()` method (was only used for debug keys)
    3. Delete `ForceTakeDamage(float)` private method
    4. Delete `ForceNextPhase()` private method
    5. Remove `if (debugMode)` log statements in Start() and AutoDetectArenaBounds() — keep the logs but remove the condition (always log in development, or just remove all debugMode references)

    In GameBalance.cs, inside Bosses.Pblob:
    Add:
    - `public const float HealthBarDrainDelay = 0.5f;`
    - `public const float HealthBarDrainSpeed = 0.35f;`
    - `public const float CircleScale = 1.5f;`
    - `public const float CircleSpawnRadius = 3.5f;`
    - `public const float Phase3MoveSpeed = 6f;`
    - `public const float GridSpawnOffsetY = -5f;`

    Update PblobController.SpawnPhase2Circles() to use GameBalance values:
    - Replace hardcoded `* 1.5f` scale with `* GameBalance.Bosses.Pblob.CircleScale`
    - Replace hardcoded `3.5f` radius with `GameBalance.Bosses.Pblob.CircleSpawnRadius`
  </action>
  <verify>Compile with 0 errors. In Play mode: pressing T or P does nothing. No debug console warnings about missing methods.</verify>
  <done>No debug keys. All magic numbers extracted to GameBalance. PblobController compiles cleanly.</done>
</task>

## Success Criteria
- [ ] Health bar starts full and drains visibly when boss takes damage
- [ ] PblobUI hierarchy has exactly 3 children (no duplicates)
- [ ] T/P debug keys have no effect in Play mode
- [ ] GameBalance has HealthBarDrainDelay, HealthBarDrainSpeed, CircleScale, CircleSpawnRadius, Phase3MoveSpeed, GridSpawnOffsetY
