---
phase: 15
plan: 1
wave: 1
---

# Plan 15.1: Boss Loop Wiring & Continue Button

## Objective
Fix the critical BossArenaManager bypass and add Continue button to Main Menu. These are the two core flow changes that complete the 4-boss progression loop.

## Context
- .gsd/ROADMAP.md (Phase 15 definition)
- Assets/Scripts/GameFlowManager/BossArenaManager.cs
- Assets/Scripts/GameFlowManager/GameFlowManager.cs
- Assets/Scripts/UI/MenuUI.cs
- Assets/Scripts/Progression/SaveManager.cs

## Tasks

<task type="auto">
  <name>Fix BossArenaManager — call AvanzarFase()</name>
  <files>Assets/Scripts/GameFlowManager/BossArenaManager.cs</files>
  <action>
    In `HandleBossDefeated()`, replace:
      `GameFlowManager.Instance.ChangeState(GameState.Shop);`
    With:
      `GameFlowManager.Instance.AvanzarFase();`
    
    This single change wires the complete post-boss flow:
    - Token reward (1 after Boss 1, 2 after Boss 2-3)
    - Auto-save
    - Post-boss dialogue overlay
    - Boss index increment
    - Transition to next shop or credits
    
    Do NOT change any other logic in this file.
  </action>
  <verify>grep "AvanzarFase" Assets/Scripts/GameFlowManager/BossArenaManager.cs → confirms fix</verify>
  <done>BossArenaManager calls AvanzarFase() instead of ChangeState(Shop)</done>
</task>

<task type="auto">
  <name>Add ContinuarJuego() and HasSavedGame() to GameFlowManager</name>
  <files>Assets/Scripts/GameFlowManager/GameFlowManager.cs</files>
  <action>
    Add two methods to GameFlowManager:
    
    1. `public bool HasSavedGame()` — returns true if save file exists AND bossIndex > 1 (meaning player has progressed beyond first shop)
    
    2. `public void ContinuarJuego()` — loads save data, sets currentBossIndex from SaveData.bossIndex, transitions to GameState.Shop (resume at shop before next boss)
    
    Place both methods near IniciarJuego() for clarity.
    Do NOT duplicate the ResetSave logic — ContinuarJuego loads, not resets.
  </action>
  <verify>grep "ContinuarJuego\|HasSavedGame" Assets/Scripts/GameFlowManager/GameFlowManager.cs → 2+ results</verify>
  <done>Both methods exist and compile cleanly</done>
</task>

<task type="auto">
  <name>Add Continue button to MenuUI</name>
  <files>Assets/Scripts/UI/MenuUI.cs</files>
  <action>
    Add a serialized Button field `continueButton`.
    In Awake/Start, check HasSavedGame() — set continueButton active/inactive accordingly.
    Wire onClick to call GameFlowManager.Instance.ContinuarJuego().
    
    Also create the button GameObject in the MainMenu scene via Unity MCP:
    - Create "ContinueButton" as child of the menu panel
    - Position below or above the Play button
    - Add Button + TMP_Text ("Continue") components
    - Wire to MenuUI.continueButton field
  </action>
  <verify>Unity console 0 errors after refresh + Continue button visible in scene hierarchy</verify>
  <done>Continue button exists, wired, and conditionally visible</done>
</task>

## Success Criteria
- [ ] BossArenaManager calls AvanzarFase()
- [ ] ContinuarJuego() and HasSavedGame() compile
- [ ] Continue button in MainMenu scene, wired to MenuUI
- [ ] 0 compile errors
