---
phase: 15
plan: 2
wave: 1
---

# Plan 15.2: Death Screen Restyle & Credits Removal

## Objective
Restyle the DeathUI with a dark overlay, centered image placeholder, and bottom-aligned buttons. Remove unused Credits scene and state.

## Context
- Assets/Scripts/UI/DeathUI.cs
- Assets/Scripts/GameFlowManager/GameFlowManager.cs
- Assets/Scripts/Editor/BuildSettingsSetup.cs
- Assets/FlowScenes/Credits.unity

## Tasks

<task type="auto">
  <name>Restyle DeathUI — dark overlay + image + bottom buttons</name>
  <files>Assets/Scripts/UI/DeathUI.cs</files>
  <action>
    Add two optional serialized fields to DeathUI:
    - `[SerializeField] Image darkOverlay` — full-screen black overlay (alpha ~0.7)
    - `[SerializeField] Image deathImage` — centered image in upper half (placeholder)
    
    In MostrarPantallaMuerte(), enable both overlays.
    In OcultarPantallaMuerte(), disable both.
    Keep existing button logic unchanged.
    
    Create scene objects via Unity MCP on the DeathUI panel:
    - "DarkOverlay" — Image, stretch anchors 0-1, color black a=0.7
    - "DeathImage" — Image, center-top, ~300x300, placeholder magenta color
    - Move existing Reintentar + SalirAlMenu buttons to bottom of panel
    - Wire darkOverlay and deathImage references
  </action>
  <verify>MCP read DeathUI component → darkOverlay and deathImage non-null</verify>
  <done>Death screen has dark overlay, centered image, and bottom buttons</done>
</task>

<task type="auto">
  <name>Remove Credits scene and state</name>
  <files>
    Assets/Scripts/GameFlowManager/GameFlowManager.cs
    Assets/Scripts/Editor/BuildSettingsSetup.cs
    Assets/FlowScenes/Credits.unity
  </files>
  <action>
    1. In GameFlowManager.cs:
       - In AvanzarFase() PostBossDialogue case: change `ChangeState(GameState.Credits)` → `VolverAlMenu()`
       - Remove the `case GameState.Credits:` block from AvanzarFase()
       - Remove Credits from GetSceneForState()
       - Remove Credits from transition guards
       - Do NOT remove GameState.Credits enum value yet (keep for future)
    
    2. In BuildSettingsSetup.cs: Remove "Assets/FlowScenes/Credits.unity" from requiredScenes
    
    3. Move Credits.unity to Archive/ folder (outside Assets)
  </action>
  <verify>
    grep "Credits.unity" BuildSettingsSetup.cs → 0 results
    AvanzarFase Boss 4 path calls VolverAlMenu()
  </verify>
  <done>Credits scene removed, Boss 4 ending goes to Main Menu</done>
</task>

## Success Criteria
- [ ] DeathUI has dark overlay + image placeholder + bottom buttons
- [ ] Credits.unity moved to Archive
- [ ] Boss 4 ending → Main Menu (not Credits)
- [ ] 0 compile errors
