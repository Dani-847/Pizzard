---
phase: 18
plan: 4
wave: 2
---

# Plan 18.4.3 — Phase 3 Combat AI (Boss Rage Mode)

## Objective
Fix Phase 3 combat so the boss aggressively chases the player instead of re-running Phase 1 attack patterns.

## Context
- `Assets/Scripts/Pblob/PblobController.cs`
- `Assets/Scripts/Core/GameBalance.cs` (Phase3MoveSpeed = 6f already added in plan 18.4.1)
- `Assets/Scripts/Pblob/PblobAttackPattern*.cs` — verify what patterns exist

## Tasks

<task type="auto">
  <name>Implement Phase3CombatRoutine — aggressive chase AI</name>
  <files>Assets/Scripts/Pblob/PblobController.cs</files>
  <action>
    Add a new coroutine Phase3CombatRoutine():
    ```csharp
    private IEnumerator Phase3CombatRoutine()
    {
        float speed = GameBalance.Bosses.Pblob.Phase3MoveSpeed;
        
        // Boss is permanently vulnerable in Phase 3
        MakeVulnerable();
        
        // Use attackPatterns[1] if it exists (Phase 3 specific), else skip
        if (attackPatterns.Length > 1 && attackPatterns[1] != null)
            attackPatterns[1].StartPattern();
        
        while (currentState == PblobState.Phase3_Combat)
        {
            if (playerTransform != null)
            {
                // Aggressive non-stop chase
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    speed * Time.deltaTime
                );
            }
            yield return null;
        }
        
        // Cleanup
        if (attackPatterns.Length > 1 && attackPatterns[1] != null)
            attackPatterns[1].StopPattern();
    }
    ```
    
    Update ChangeState() case Phase3_Combat:
    - Remove the old `attackPatterns[0].StartPattern()` call
    - Set: `stateCoroutine = StartCoroutine(Phase3CombatRoutine());`
    
    The boss should NOT use attackPatterns[0] in Phase 3 at all.
    attackPatterns[0] = Phase 1 patterns only.
    attackPatterns[1] = Phase 3 patterns (if exists, otherwise pure chase).
  </action>
  <verify>Trigger Phase 3 combat (call FinishGridPuzzle() or lower HP past 0%) → boss moves directly toward player at full speed without stopping. Does NOT fire Phase 1 patterns.</verify>
  <done>Phase 3 boss chases player continuously. attackPatterns[0] NOT active. Boss permanently vulnerable (player can damage it freely). No Phase 1 attack visual effects visible.</done>
</task>

<task type="checkpoint:human-verify">
  <name>Full boss fight playthrough verification</name>
  <action>
    Manual test of complete fight:
    1. Start BossArena_1 scene in Play mode
    2. Attack boss → health bar red section shrinks, orange drains slowly
    3. Boss HP drops to ~66% → circles appear at 120° spread, player can walk inside them
    4. Circle timer expires → repeat or take damage
    5. Boss HP drops to ~33% → boss moves to top, player relocates to bottom, grid appears
    6. Grid reveals green path → walk the path → boss drops to Phase 3 combat
    7. Phase 3: boss chases player at full speed, player can freely damage boss
    8. Boss HP = 0 → boss defeated
    
    Press Esc at any point → game pauses. Press Esc again → resumes.
  </action>
  <verify>All phases flow correctly end-to-end. No console errors during any phase.</verify>
  <done>Complete boss fight runs without errors. All 3 phases function as intended.</done>
</task>

## Success Criteria
- [ ] Phase 3 combat: boss chases player, never stops
- [ ] Phase 3 combat: boss permanently vulnerable (health bar keeps decreasing)
- [ ] Phase 3 combat: Phase 1 projectile patterns NOT active
- [ ] Full boss fight completable from start to defeat
