# Phase 16: Language System Completion - Research

**Researched:** 2026-02-28
**Domain:** Unity UI Localization / C# string management
**Confidence:** HIGH — infrastructure is fully readable in-codebase; no external libraries involved.

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Dialogue Content**
- Claude drafts all dialogue in English, user reviews. Claude also writes Spanish translations.
- Dialogue length varies by moment: intro is longer (5+ lines), pre-boss is short (1-2 taunt lines), post-boss medium (2-3 lines), shop dialogue is brief.
- Tone: mix — Raberto is humorous/quirky in shop and guidance scenes, but boss intros and encounters feel more dramatic.
- Speaking characters: Raberto + bosses speak. Each boss has taunt/intro lines. Player is silent protagonist.
- Boss names and personalities: Claude finds boss references in the codebase and uses whatever names/themes exist there.
- Boss names: localized (translated into Spanish where it makes sense — e.g. "Pepperoni Dragon" → "Dragón de Pepperoni").
- Spanish variant: neutral/universal Spanish — avoid region-specific terms, understandable everywhere.

**Key Naming & Coverage**
- Localize everything in the shop: upgrade names, descriptions, state labels ("Max Wand Level"), token count label.
- Character names like "Raberto" stay hardcoded — proper nouns, not localization keys.
- Mystery placeholders like "???" stay hardcoded — universal symbols.
- Game Over and Credits screens: Claude scans codebase to find what exists and adds keys for all text found.
- Spell/element names: localize them (Spanish players should see Spanish spell names).

**Language Save Integration**
- Keep PlayerPrefs as the storage mechanism — language is a user preference, not game progress. Simpler and persists without a save file.
- Default language: always start English. User switches to Spanish manually in Options.

**Dynamic Text Handling**
- Format strings in JSON: e.g. `"tokens_count": "Tokens: {0}"`. Code calls `string.Format(GetText("tokens_count"), count)`.
- Dynamic text (tokens, health, mana) handled in code via `LocalizationManager.GetText()` + `string.Format()`. No LocalizedText component on dynamic elements — code-only approach.
- Immediate update on language switch: all text including dynamic HUD labels refreshes when language changes. Combat scripts subscribe to OnLanguageChanged.
- No special number formatting — numbers display the same in both languages.

### Claude's Discretion
- Exact key naming convention (prefix style like `shop_`, `combat_`, `boss_`)
- How to structure multi-line dialogues in JSON (array of entries vs numbered keys)
- Loading/error handling for missing keys
- Whether to add a helper method for format strings or keep it as raw string.Format calls

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope.
</user_constraints>

---

## Summary

The localization infrastructure is **complete and working**. `LocalizationManager` (singleton, DontDestroyOnLoad) loads JSON from `Resources/Languages/en.json` and `es.json`, exposes `GetText(key)`, and fires `OnLanguageChanged` when `SetLanguage(int)` is called. `LocalizedText` is a drop-on component that subscribes to that event and refreshes any `TMP_Text` or legacy `Text` on the same GameObject. `OptionsUI` already wires the dropdown to `SetLanguage()`.

The gap is **content**: the JSON files have placeholder values ("Dialog 1 (EN)"), many keys are missing entirely (shop upgrades, potion/mana labels, element names, boss names), and several scripts still write hardcoded strings directly to TMP fields. This phase is a content + wiring completion job — no new systems needed.

The four bosses confirmed in the codebase are: **Hec'kiel** (`HeckielController`), **Pomodoro** (`PomodoroController`), **Niggel** (`NiggelController`), and **P'blob** (`PblobController`). The element types are: `queso`, `pepperoni`, `piña` (plus `None`). These drive the dialogue key structure and localization keys for spell/element names.

**Primary recommendation:** Complete the JSON files with real content first, then wire the three code-touch points (ShopUI dynamic text, ElementSelectionUI button labels, ElementSelectorUI Reset button) to use `GetText()` + `OnLanguageChanged`, then attach `LocalizedText` components to all static UI text objects in the Unity scene.

---

## Standard Stack

### Core
| Component | Location | Purpose | Status |
|-----------|----------|---------|--------|
| `LocalizationManager` | `Assets/Scripts/Languaje/LocalizationManager.cs` | Singleton; loads JSON, exposes `GetText(key)`, fires `OnLanguageChanged` event | Fully functional |
| `LocalizedText` | `Assets/Scripts/Languaje/LocalizedText.cs` | Drop-on component; auto-subscribes to `OnLanguageChanged`, refreshes `TMP_Text` or `Text` | Fully functional |
| `en.json` / `es.json` | `Assets/Resources/Languages/` | String tables; `{"entries":[{"key":"...","value":"..."}]}` JsonUtility format | Incomplete content |
| `OptionsUI` | `Assets/Scripts/UI/OptionsUI.cs` | Language dropdown → `SetLanguage(int index)` already wired | Working end-to-end |

### JSON Format (MUST NOT CHANGE)
```json
{
  "entries": [
    { "key": "menu_play", "value": "Play" }
  ]
}
```
This is parsed by `JsonUtility.FromJson<LocalizationWrapper>()`. Any format change breaks deserialization.

### Dynamic Text Pattern (decided)
```csharp
// In code that displays dynamic values:
txtTokenCount.text = string.Format(
    LocalizationManager.Instance.GetText("shop_token_count"), tokens);

// Subscribe to language changes:
void OnEnable() => LocalizationManager.Instance.OnLanguageChanged += RefreshDynamicText;
void OnDisable() => LocalizationManager.Instance.OnLanguageChanged -= RefreshDynamicText;
void RefreshDynamicText() {
    // re-call the same display logic
}
```

---

## Architecture Patterns

### Recommended Key Prefix Convention (Claude's Discretion)
Use category prefixes consistently:
- `menu_` — main menu buttons and labels
- `options_` — options panel labels
- `shop_` — all shop UI text
- `combat_` — in-combat HUD labels (health, mana, potions)
- `element_` — element/spell names
- `boss_` — boss names
- `dialog_intro_`, `dialog_prebossN_`, `dialog_postbossN_`, `dialog_deathshop_` — dialogue lines (already established, keep)
- `death_` — death screen buttons/text
- `gameover_` — game over text (if a separate screen exists)

### Multi-Line Dialogue Structure (Claude's Discretion — Recommended)
Use numbered keys per line, which `DialogUI` already reads via `GetLocalizedLines(keyPrefix, fallbackLines)`:
```json
{ "key": "dialog_intro_1", "value": "Ah, a new apprentice..." },
{ "key": "dialog_intro_2", "value": "Raberto's Pizza Academy welcomes you!" },
{ "key": "dialog_intro_3", "value": "But first — you'll need a wand. Let's shop." }
```
This matches the existing `fallbackLines.Length` loop in `DialogUI.GetLocalizedLines()`. The number of keys in JSON must match the number of entries in `DialogUI`'s Inspector array fields — **both must be updated together**.

### Static Text: LocalizedText Component
For any `TMP_Text` or `Text` component showing a fixed label:
1. Add `LocalizedText` component to the same GameObject
2. Set the `key` field in Inspector
3. Remove or clear the hardcoded text value

The component's `WaitForLocalizationManager()` coroutine handles startup ordering safely.

### Dynamic Text: Code Subscription
For scripts that compute text at runtime (ShopUI tokens, BossHealthBarUI health numbers, PotionUI count):
- These already write `text =` directly in `Update()` or event handlers
- Subscribe `RefreshDynamicText()` to `OnLanguageChanged` for the label portion
- Keep number formatting as `string.Format("{0}", value)` — no special locale formatting needed

---

## Complete Hardcoded String Audit

### Scripts requiring code changes:

| Script | File | Hardcoded Text | Action |
|--------|------|----------------|--------|
| `ShopUI` | line 135 | `"Max Wand Level"` | Replace with `GetText("shop_wand_max_level")` |
| `ShopUI` | line 143 | `"Buy next wand:\n\"{nextWandName}\""` | Replace with `string.Format(GetText("shop_wand_buy"), nextWandName)` — OR keep wand names hardcoded since they're proper nouns |
| `ShopUI` | line 240 | `"Tokens: 0"` (initial value) | Replace with format string using `GetText("shop_token_count")` |
| `ShopUI` | line 281 | `$"Tokens: {tokens}"` | Replace with `string.Format(GetText("shop_token_count"), tokens)` |
| `ElementSelectorUI` | line 45 | `"Reset"` | Replace with `GetText("shop_reset")` |
| `ElementSelectionUI` | line 79 | `"Deselect All"` | Replace with `GetText("shop_deselect_all")` |
| `ElementSelectionUI` | lines 66-67 | `element.ToString()` for button labels | Replace with `GetText("element_" + element.ToString())` |
| `ElementSelectorUI` | line 36 | `element.ToString()` for button labels | Replace with `GetText("element_" + element.ToString())` |
| `BossHealthBarUI` | line 33 | `trackedBoss.gameObject.name` | Boss names come from GameObject name in scene — add `LocalizationManager.GetText("boss_" + trackedBoss.gameObject.name.ToLower().Replace(" ", "_"))` with fallback to gameObject.name |

### Scripts with UI text in Inspector (scene wiring required, no code change):
These scripts have TMP_Text references but text is set in the Unity scene/prefab. Attach `LocalizedText` + key in the Inspector:

| Script/UI | Text Elements | Keys Needed |
|-----------|--------------|------------|
| `MenuUI` | Play, Options/Settings, Exit, Continue buttons | `menu_play`, `menu_options`, `menu_exit`, `menu_continue` |
| `OptionsUI` | Language label, Volume label, Combinations label, Accept button | `options_language`, `options_volume`, `options_combinations`, `options_accept` |
| `DeathUI` | Retry button, Exit to Menu button | `death_retry`, `death_exit_menu` |
| `CombinationsUI` | Back button | `options_back` |
| Combat HUD | Health label (if any), Mana label (if any), Potion label (if any) | `combat_health`, `combat_mana`, `combat_potions` |

Note: `PotionUI` only shows a number (`potionText.text = currentPotions.ToString()`) — the label text (if one exists separately) needs `LocalizedText`. `ManaUI` is a pure visual bar with no text labels. `BossHealthBarUI` has optional `healthText` (numeric) and `bossNameText`.

### Scripts that do NOT need changes:
- `DialogUI` — speaker name "Raberto" hardcoded by design decision
- `CombinationsUI` lines 149/152 — `"???"` hardcoded by design decision
- `PlayerHealth` — no UI text
- `ManaUI` — no text, pure bar

---

## Boss Names and Dialogue Keys

### Boss roster (from codebase):
| Boss | Class | Boss Index (assumed) | Localization Key | Spanish Translation |
|------|-------|---------------------|-----------------|---------------------|
| Hec'kiel | `HeckielController` | 1 | `boss_heckiel` | `Hec'kiel` (keep — fantasy proper noun) |
| Pomodoro | `PomodoroController` | 2 | `boss_pomodoro` | `Pomodoro` (keep — Italian/universal) |
| Niggel | `NiggelController` | 3 | `boss_niggel` | `Niggel` (keep — fantasy proper noun) |
| P'blob | `PblobController` | 4 | `boss_pblob` | `P'blob` (keep — fantasy proper noun) |

Note: Boss name localization is useful for display text like "Derrotaste a [boss_name]!" rather than translating the name itself. The `BossHealthBarUI` uses `trackedBoss.gameObject.name` which is the scene GameObject name — this will need to be matched to a localization key.

### Dialogue key structure (established + to complete):
```
dialog_intro_1 ... dialog_intro_N          (Raberto's opening — 5+ lines)
dialog_preboss1_1 ... dialog_preboss1_N    (before Boss 1 — 1-2 lines)
dialog_postboss1_1 ... dialog_postboss1_N  (after Boss 1 — 2-3 lines)
dialog_preboss2_1 ... dialog_postboss4_N   (same pattern for bosses 2-4)
dialog_deathshop_1 ... dialog_deathshop_N  (death return — brief)
```
The `DialogUI.preBossDialogKeyPrefix` concatenates boss index: `"dialog_preboss" + manager.currentBossIndex + "_"`. Verify what values `currentBossIndex` takes during gameplay before writing keys.

### Element names:
```
element_queso     → "Queso" / "Queso"
element_pepperoni → "Pepperoni" / "Pepperoni"
element_piña      → "Pineapple" / "Piña"
```
Note: `piña` is already Spanish. In English it should display as "Pineapple". Element enum value is `piña` — the key is `element_piña` regardless.

---

## Complete Key Inventory (to add to both JSON files)

### Currently in JSON (partial, needs real content):
- `menu_play`, `menu_options`, `menu_exit` — exist, have real values
- `options_language`, `options_volume`, `options_combinations`, `options_accept` — exist
- `dialog_intro_1`, `dialog_preboss1_1` through `dialog_postboss4_1`, `dialog_deathshop_1` — exist as placeholders
- `shop_warning_exit` — exists, has real content

### Keys to ADD:
```
# Menu
menu_continue

# Shop
shop_upgrade_wand
shop_upgrade_potion
shop_upgrade_mana
shop_exit_shop
shop_token_count        (format: "Tokens: {0}")
shop_wand_max_level     (= "Max Wand Level" / "Nivel Máximo de Varita")
shop_wand_buy           (format: "Buy next wand:\n\"{0}\"" or similar)
shop_reset              (= "Reset" / "Reiniciar")
shop_deselect_all       (= "Deselect All" / "Deseleccionar Todo")

# Elements
element_queso
element_pepperoni
element_piña

# Combat HUD (if label text objects exist in scene)
combat_health
combat_mana
combat_potions

# Boss names (for display text)
boss_heckiel
boss_pomodoro
boss_niggel
boss_pblob

# Death screen
death_retry
death_exit_menu

# Dialogue — fill in real content for all dialog_* placeholder keys
# + add additional numbered lines (dialog_intro_2, dialog_intro_3, etc.)
```

---

## Common Pitfalls

### Pitfall 1: DialogUI fallback array length mismatch
**What goes wrong:** `GetLocalizedLines()` iterates `fallbackLines.Length` times. If JSON has more keys than the array length, extra lines are silently ignored. If array is longer than JSON keys, fallback strings show.
**How to avoid:** When adding dialogue lines to JSON (e.g., `dialog_intro_1` through `dialog_intro_5`), also update the Inspector array field in `DialogUI` (the `introDialogLines` array) to match the count — even with placeholder strings. The array length gates how many lines are shown.
**Warning signs:** Dialogue ends early or shows "(EN)" placeholder text in localized language.

### Pitfall 2: OnLanguageChanged subscription in dynamic scripts
**What goes wrong:** Scripts that update text in `Update()` or on events won't refresh their label when language changes — only the dynamic value part updates, leaving a stale English label.
**How to avoid:** Any script that writes a localized string to a TMP field must subscribe to `LocalizationManager.Instance.OnLanguageChanged`. Subscribe in `OnEnable`/`Start` after null-checking Instance, unsubscribe in `OnDisable`/`OnDestroy`.
**Warning signs:** Language switches but "Tokens:" label stays in old language while the number updates.

### Pitfall 3: LocalizationManager not yet initialized when scene loads
**What goes wrong:** `LocalizationManager` uses `DontDestroyOnLoad` singleton. If `LocalizedText` components try to call `GetText` before `Awake()` completes on LocalizationManager, they get null reference.
**How to avoid:** Already handled — `LocalizedText` uses `WaitForLocalizationManager()` coroutine that yields until `Instance != null`. For code-based subscriptions, always null-check before subscribing and consider doing it in `Start()` not `Awake()`.

### Pitfall 4: JSON key typos produce [key] in-game
**What goes wrong:** `GetText(key)` returns `"[key]"` for missing keys. This is the fallback behavior by design, but typos in key strings will silently show bracketed keys in production.
**How to avoid:** Define key constants or use a single source of truth. When adding a key to JSON, immediately update the calling code. Test by switching language in-game and looking for `[...]` brackets.

### Pitfall 5: Element button text uses `element.ToString()` (lowercase enum values)
**What goes wrong:** `element.ToString()` gives `"queso"`, `"pepperoni"`, `"piña"` — lowercase. If the key is `element_queso` then `"element_" + element.ToString()` = `"element_queso"` — this matches. But if EN display value should be "Queso" (capitalized), that's stored in the JSON value, not derived from the key.
**How to avoid:** Key = `"element_" + element.ToString().ToLower()`. JSON value = properly capitalized display string. This is clean.

### Pitfall 6: BossHealthBarUI uses GameObject name for boss display
**What goes wrong:** `bossNameText.text = trackedBoss.gameObject.name` — this is the raw scene object name (may be "HeckielController", "Boss_Heckiel", etc.), not a localization key.
**How to avoid:** Add a `[SerializeField] private string bossLocalizationKey;` to `BossHealthBarUI` (or to `BossBase`), set it per boss in Inspector. If `LocalizationManager.Instance.GetText(bossLocalizationKey)` returns a valid string, use it; otherwise fall back to `gameObject.name`.

---

## Code Examples

### Subscribe to OnLanguageChanged in a dynamic script (ShopUI pattern)
```csharp
// In ShopUI.cs
private void OnEnable()
{
    if (LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged += RefreshLocalized;
}

private void OnDisable()
{
    if (LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged -= RefreshLocalized;
}

private void RefreshLocalized()
{
    RefreshTokens();        // re-runs the display logic
    UpdateWandButtonUI();   // re-runs wand text
}

// In RefreshTokens():
public void RefreshTokens()
{
    if (txtTokenCount != null && Pizzard.Progression.ProgressionManager.Instance != null)
    {
        int tokens = Pizzard.Progression.ProgressionManager.Instance.BossCurrency;
        string fmt = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText("shop_token_count")
            : "Tokens: {0}";
        txtTokenCount.text = string.Format(fmt, tokens);
    }
}
```

### Format string in JSON
```json
{ "key": "shop_token_count", "value": "Tokens: {0}" }
```
```json
{ "key": "shop_token_count", "value": "Fichas: {0}" }
```

### Localized element button text (ElementSelectionUI pattern)
```csharp
// Replace: buttonGO.GetComponentInChildren<Text>().text = element.ToString();
// With:
string elementKey = "element_" + element.ToString().ToLower();
string displayText = LocalizationManager.Instance != null
    ? LocalizationManager.Instance.GetText(elementKey)
    : element.ToString();
buttonGO.GetComponentInChildren<Text>().text = displayText;
```
Note: These buttons are regenerated by `GenerateButtons()` which is called from `OpenSelection()`. To update on language change, call `GenerateButtons()` again from a `OnLanguageChanged` subscription.

### Boss name localization in BossHealthBarUI
```csharp
// Add to BossBase or BossHealthBarUI:
[SerializeField] private string bossLocalizationKey = "";

// In BossHealthBarUI.OnEnable():
if (bossNameText != null)
{
    string displayName = trackedBoss.gameObject.name;
    if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(bossLocalizationKey))
    {
        string loc = LocalizationManager.Instance.GetText(bossLocalizationKey);
        if (!loc.StartsWith("[")) displayName = loc;
    }
    bossNameText.text = displayName;
}
```

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Localization system | Custom manager | Existing `LocalizationManager` | Already fully functional |
| Text auto-refresh | Manual refresh calls | `LocalizedText` component | Already handles coroutine wait and subscription |
| Language persistence | Custom save field | `PlayerPrefs.GetInt("Idioma", 0)` | Already established, consistent with OptionsUI |
| JSON parsing | Custom parser | `JsonUtility.FromJson<LocalizationWrapper>()` | Already working, format is locked |

**Key insight:** Every building block exists. The entire phase is content creation + completing wiring of existing hooks.

---

## Validation Architecture

> Skipped — `workflow.nyquist_validation` is `false` in `.planning/config.json`.

---

## Open Questions

1. **What is `currentBossIndex` value for each boss?**
   - What we know: `DialogUI.ShowPreBossDialog()` uses `manager.currentBossIndex` to form keys like `dialog_preboss1_1`. BossControllers are Hec'kiel, Pomodoro, Niggel, P'blob.
   - What's unclear: Are bossIndex values 1-4, or 0-3? Need to verify in `GameFlowManager`.
   - Recommendation: Read `GameFlowManager.cs` at plan time to confirm index mapping before writing dialogue keys.

2. **Does a "Game Over" screen exist as a separate scene/panel, or is `DeathUI` the only death UI?**
   - What we know: `DeathUI.cs` exists with retry and exit-to-menu buttons. No `GameOver*.cs` file found via glob.
   - What's unclear: Are there text labels (e.g., "You Died", "Game Over") inside the DeathUI panel that need localization keys?
   - Recommendation: During plan, scan the DeathUI prefab/scene for text objects and add keys accordingly.

3. **Do Combat HUD label text objects (health, mana, potion) exist as separate GameObjects?**
   - What we know: `ManaUI` is a pure visual bar (no text). `PotionUI` shows a raw number. `BossHealthBarUI` shows a number and optional boss name.
   - What's unclear: Whether there are separate static label GameObjects (e.g., a "Health:" text next to the health bar) that need `LocalizedText` components.
   - Recommendation: Check the scene hierarchy for the Combat state in UIManager for static label objects.

4. **Are wand names ("Tier 2 Wand", "Tier 3 Wand") to be localized or treated as proper nouns?**
   - What we know: `ShopUI.UpdateWandButtonUI()` hardcodes `"Tier 2 Wand"` and `"Tier 3 Wand"` in English.
   - What's unclear: Design intent for wand tier names in Spanish.
   - Recommendation: Localize them — add `shop_wand_tier_2` and `shop_wand_tier_3` keys. Spanish: "Varita de Nivel 2", "Varita de Nivel 3".

---

## Sources

### Primary (HIGH confidence)
- Direct codebase read: `Assets/Scripts/Languaje/LocalizationManager.cs` — API surface, event model, JSON format
- Direct codebase read: `Assets/Scripts/Languaje/LocalizedText.cs` — component lifecycle, coroutine wait pattern
- Direct codebase read: `Assets/Resources/Languages/en.json` + `es.json` — current key set
- Direct codebase read: All identified UI scripts — hardcoded string locations confirmed by line number

### Secondary (MEDIUM confidence)
- CONTEXT.md decisions — user locked choices on dynamic text pattern, PlayerPrefs storage, neutral Spanish
- Boss class names from `Assets/Scripts/Bosses/*.cs` — Hec'kiel, Pomodoro, Niggel, P'blob confirmed
- Element types from `Assets/Scripts/Elements/ElementType.cs` — `queso`, `pepperoni`, `piña`, `None`

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — read directly from source
- Architecture patterns: HIGH — existing code is the pattern, confirmed by reading
- Pitfalls: HIGH — derived from actual code structure, not speculation
- Dialogue content: LOW until written — quality depends on creative judgment, not code

**Research date:** 2026-02-28
**Valid until:** 2026-03-30 (stable — no external dependencies, pure codebase)
