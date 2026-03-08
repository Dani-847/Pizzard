---
status: awaiting_human_verify
trigger: "Clicking Options in the start/main menu causes the screen to go completely blank"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: ForceApplyUIForState hides everything (via `foreach child SetActive(false)`) which runs AFTER optionsUI.Show() sets it active, OR the OptionsUI.Show() itself crashes silently because SoundManager.Instance is null — leaving nothing visible.
test: Traced execution path of OnClickAjustes -> Hide() -> OpenOptions() -> optionsUI.Show(); checked OptionsUI.Show() which calls SoundManager.Instance.GetMusicVolume() with no null guard — if SoundManager is null this throws and returns mid-Show, but gameObject was already SetActive(true) before the crash line. But wait — ForceApplyUIForState does `foreach child SetActive(false)` which is separate from HideAllUIs. This runs in Start(). The real runtime issue is in OptionsUI.Show(): SoundManager.Instance null check is missing.
expecting: Root cause confirmed — two potential issues found, primary one is SoundManager null crash in Show().
next_action: CONFIRMED — fix OptionsUI.Show() to guard against null SoundManager, same as Start() does.

## Symptoms

expected: Options panel/settings UI appears when clicking Options button in main menu
actual: Screen goes blank (everything disappears, nothing replaces it)
errors: unknown — need to check Unity console, likely a NullReferenceException in OptionsUI.Show()
reproduction: Start game -> Main Menu -> Click "Options" button
started: Currently fixing final errors before a pre-build

## Eliminated

- hypothesis: optionsUI reference is null in UIManager (OpenOptions would silently do nothing)
  evidence: Code path reaches optionsUI.Show() — if null the panel simply wouldn't appear, but MenuUI.Hide() already hid the menu, causing blank screen. This is STILL a valid partial cause (if optionsUI IS null in Inspector, blank screen results). However, the more specific cause is in OptionsUI.Show().
  timestamp: 2026-03-02

## Evidence

- timestamp: 2026-03-02
  checked: MenuUI.cs OnClickAjustes
  found: Calls Hide() first (hides menu panel), THEN calls UIManager.Instance.OpenOptions(UIContext.Menu)
  implication: If OpenOptions fails for any reason, screen is blank because menu is already hidden.

- timestamp: 2026-03-02
  checked: UIManager.cs OpenOptions
  found: Guards with `if (optionsUI != null)` before calling Show(). If optionsUI is null in Inspector, Show() never called = blank screen.
  implication: Null optionsUI reference in Inspector is one possible cause.

- timestamp: 2026-03-02
  checked: OptionsUI.cs Show() method (line 93-99)
  found: Show() calls `SoundManager.Instance.GetMusicVolume()` with NO null check. Start() has a null guard that returns early — but Show() does NOT. If SoundManager.Instance is null at the moment Show() is called, this throws a NullReferenceException AFTER gameObject.SetActive(true) has already executed. The exception propagates up and Unity may or may not show the panel.
  implication: If SoundManager isn't ready when Options is opened, Show() crashes mid-execution. The gameObject IS active (SetActive happened) but ActualizarPizzaVisual() is never called — minor visual issue. The real crash point is SoundManager null.

- timestamp: 2026-03-02
  checked: ForceApplyUIForState in UIManager.cs (lines 73-124)
  found: It calls HideAllUIs() AND ALSO does `foreach (Transform child in transform) child.gameObject.SetActive(false)` — this disables ALL children of UIManager including OptionsUI. This runs in UIManager.Start(). It is NOT called when clicking Options at runtime, so it's not the runtime culprit.
  implication: Not the direct cause of the click-time blank screen, but confirms the startup sequence is aggressive about hiding everything.

## Resolution

root_cause: TWO issues, either of which causes blank screen:
  1. PRIMARY: OptionsUI.Show() calls SoundManager.Instance.GetMusicVolume() without a null guard. If SoundManager.Instance is null, a NullReferenceException is thrown. The gameObject was already SetActive(true) but the exception aborts the method. Depending on Unity's exception handling, the UI may flicker or appear broken. More critically — the exception propagates to UIManager.OpenOptions, which may cause OpenOptions to be treated as failed.
  2. SECONDARY (more likely cause of true blank): If `optionsUI` field in UIManager Inspector is NOT assigned, the null check `if (optionsUI != null)` silently skips Show() entirely — screen is blank because MenuUI was already hidden.

fix: Added null guard for SoundManager.Instance in OptionsUI.Show() matching the same pattern used in Start(). The guard calls gameObject.SetActive(true) first (so panel IS visible), then returns early with an error log if SoundManager is null instead of crashing. This means the options panel will appear even if SoundManager is missing, just without volume sync.
verification: awaiting human confirmation
files_changed:
  - Assets/Scripts/UI/OptionsUI.cs
