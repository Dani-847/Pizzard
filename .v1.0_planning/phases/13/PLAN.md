---
phase: 13
plan: 1
wave: 1
depends_on: [12]
files_modified:
  - Assets/Scripts/Player/FatigueSystem.cs → ManaSystem.cs
  - Assets/Scripts/UI/FatigueUI.cs → ManaUI.cs
  - Assets/Scripts/UI/ShopUI.cs
  - Assets/Scripts/UI/UIManager.cs
  - Assets/Scripts/Player/PlayerAimAndCast.cs
  - Assets/Scripts/Progression/SaveManager.cs
  - Assets/Scripts/GameFlowManager/GameFlowManager.cs
autonomous: true
user_setup: []

must_haves:
  truths:
    - "FatigueSystem renamed to ManaSystem everywhere (class, file, references)"
    - "FatigueUI renamed to ManaUI everywhere (class, file, references)"
    - "SaveData.fatigueMax renamed to SaveData.manaMax"
    - "Per-spell mana costs defined in a hardcoded dictionary in ManaSystem"
    - "PlayerAimAndCast uses the dictionary to look up costs by combo key"
    - "Casting blocked with console warning when mana insufficient"
    - "ManaUI bar drains in real-time during combat and regenerates"
    - "Shop upgrade button text says 'Mana' not 'Fatigue'"
    - "0 compile errors"
  artifacts:
    - "Assets/Scripts/Player/ManaSystem.cs exists"
    - "Assets/Scripts/UI/ManaUI.cs exists"
    - "Old FatigueSystem.cs and FatigueUI.cs deleted"
---

# Plan 13.1: Mana System — Rename + Spell Costs + Combat Integration

<objective>
Rename the Fatigue system to Mana across all code and UI. Define a hardcoded
per-spell mana cost dictionary. Wire spell casting to check mana before firing
and drain the bar in real-time.

Purpose: Make the resource intuitive ("Mana" vs "Fatigue"), add granular
per-combo costs for balance, and ensure the bar visually reflects combat usage.

Output: ManaSystem.cs, ManaUI.cs, updated PlayerAimAndCast, updated SaveData.
</objective>

<context>
Load for context:
- Assets/Scripts/Player/FatigueSystem.cs (will become ManaSystem.cs)
- Assets/Scripts/UI/FatigueUI.cs (will become ManaUI.cs)
- Assets/Scripts/Player/PlayerAimAndCast.cs (lines 123-136: fatigue check)
- Assets/Scripts/Progression/SaveManager.cs (SaveData.fatigueMax field)
- Assets/Scripts/UI/ShopUI.cs (btnUpgradeElementalFatigue)
- Assets/Scripts/UI/UIManager.cs (FatigueUI references)
- Assets/Scripts/GameFlowManager/GameFlowManager.cs (FatigueSystem auto-create)
</context>

<tasks>

<task type="auto">
  <name>Wave 1: Rename FatigueSystem → ManaSystem + ManaUI</name>
  <files>
    Assets/Scripts/Player/ManaSystem.cs (NEW, replaces FatigueSystem.cs)
    Assets/Scripts/UI/ManaUI.cs (NEW, replaces FatigueUI.cs)
    Assets/Scripts/Player/FatigueSystem.cs (DELETE)
    Assets/Scripts/UI/FatigueUI.cs (DELETE)
    Assets/Scripts/Progression/SaveManager.cs
    Assets/Scripts/UI/ShopUI.cs
    Assets/Scripts/UI/UIManager.cs
    Assets/Scripts/GameFlowManager/GameFlowManager.cs
    Assets/Scripts/Player/PlayerAimAndCast.cs
  </files>
  <action>
    1. Create ManaSystem.cs — copy FatigueSystem.cs, rename:
       - Class: FatigueSystem → ManaSystem
       - Properties: MaxFatigue → MaxMana, CurrentFatigue → CurrentMana
       - Methods: CanCast (keep), ConsumeFatigue → ConsumeMana, 
         UpgradeMaxFatigue → UpgradeMaxMana, LoadMaxFatigueFromSave → LoadMaxManaFromSave
       - Debug logs: "[FatigueSystem]" → "[ManaSystem]"
       - Keep namespace Pizzard.Core
       - Keep DontDestroyOnLoad singleton pattern
       
    2. Add hardcoded mana cost dictionary to ManaSystem:
       ```csharp
       public static readonly Dictionary<string, float> SpellCosts = new()
       {
           // Tier 1 — Single element (cheap)
           { "queso", 10f },
           { "pepperoni", 12f },
           { "piña", 11f },
           
           // Tier 2 — Two elements (moderate)
           { "queso|pepperoni", 20f },
           { "queso|piña", 18f },
           { "pepperoni|queso", 22f },
           { "pepperoni|piña", 20f },
           { "piña|queso", 19f },
           { "piña|pepperoni", 21f },
           
           // Tier 3 — Three elements (expensive)
           // Add specific combos as they're implemented. Default: 30f
       };
       
       public static float GetSpellCost(string comboKey)
       {
           return SpellCosts.TryGetValue(comboKey, out float cost) ? cost : 30f;
       }
       ```
       
    3. Create ManaUI.cs — copy FatigueUI.cs, rename:
       - Class: FatigueUI → ManaUI
       - Property: fatigueFillBar → manaFillBar  
       - Color: Keep bright cyan, rename FatigueColor → ManaColor
       - Update to reference ManaSystem.Instance instead of FatigueSystem.Instance
       
    4. Update SaveManager.cs:
       - SaveData.fatigueMax → SaveData.manaMax
       - Any reference to fatigueMax in load/save logic
       
    5. Update ShopUI.cs:
       - btnUpgradeElementalFatigue → btnUpgradeMana
       - OnBtnUpgradeElementalFatigue → OnBtnUpgradeMana
       - FatigueSystem.Instance → ManaSystem.Instance
       - Debug log strings
       
    6. Update UIManager.cs:
       - transform.Find("FatigueUI") → transform.Find("ManaUI")
       
    7. Update GameFlowManager.cs:
       - FatigueSystem → ManaSystem in auto-create block
       
    8. Update PlayerAimAndCast.cs:
       - Replace lines 123-136 with dictionary lookup:
         float cost = ManaSystem.GetSpellCost(key);
         if (ManaSystem.Instance != null && !ManaSystem.Instance.CanCast(cost)) { warn; return; }
         ManaSystem.Instance.ConsumeMana(cost);
       - Remove baseFatigueCost field if no longer used
       
    9. Delete old files: FatigueSystem.cs, FatigueUI.cs
    
    AVOID: Renaming the Unity GameObjects in the scene (keep "FatigueUI" name 
    in hierarchy for now — UIManager.Find will be updated). We rename the GameObject
    via MCP after scripts compile.
    
    AVOID: Changing the save file format breaking existing saves — ensure backward
    compatibility by checking for both fatigueMax and manaMax in LoadGame().
  </action>
  <verify>
    - Unity compiles with 0 errors after refresh
    - grep -r "FatigueSystem" Assets/Scripts/ returns 0 results
    - grep -r "FatigueUI" Assets/Scripts/ returns 0 results  
    - ManaSystem.SpellCosts dictionary has entries for all Tier 1 and Tier 2 combos
  </verify>
  <done>
    All references to "Fatigue" renamed to "Mana" across scripts. Hardcoded
    cost dictionary exists in ManaSystem. PlayerAimAndCast uses per-combo costs.
    0 compile errors.
  </done>
</task>

<task type="checkpoint:human-verify">
  <name>Wave 2: Visual Verification — Mana Bar in Combat</name>
  <files>None (verification only)</files>
  <action>
    1. Rename the "FatigueUI" GameObject in the scene hierarchy to "ManaUI" via Unity MCP.
    2. Enter Play mode → go through Shop 1 → buy wand → select element → exit to combat.
    3. Cast spells and observe:
       - Mana bar drains per spell cast
       - Mana bar regenerates after 1.5s delay
       - Console shows "[ManaSystem] Not enough mana!" when empty
       - Bar color is bright cyan/teal
    4. Request screenshot from user showing mana bar mid-combat.
  </action>
  <verify>User confirms mana bar drains and regenerates visually during combat</verify>
  <done>
    Mana bar visually responds to spell casting with proper drain and regen.
    User has confirmed via screenshot.
  </done>
</task>

</tasks>

<verification>
After all tasks, verify:
- [ ] No references to "FatigueSystem" or "FatigueUI" remain in any .cs file
- [ ] ManaSystem.Instance is not null during combat
- [ ] Casting a spell reduces CurrentMana by the dictionary cost
- [ ] Mana bar UI updates in real-time
- [ ] Shop "Mana" upgrade works (MaxMana *= 1.5)
- [ ] Save/Load preserves manaMax correctly
- [ ] 0 compile errors
</verification>

<success_criteria>
- [ ] All Fatigue→Mana rename done
- [ ] Per-spell hardcoded cost dictionary functional
- [ ] Mana gating blocks spells when insufficient
- [ ] Visual bar feedback confirmed by user
</success_criteria>
