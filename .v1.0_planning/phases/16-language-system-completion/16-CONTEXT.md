# Phase 16: Language System Completion - Context

**Gathered:** 2026-02-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Complete the EN/ES translation system so every UI text uses localization keys and language can be toggled live from the Options menu. The `LocalizationManager` and `LocalizedText` infrastructure already exists — this phase fills in all missing keys, replaces all hardcoded strings, writes real dialogue content, and handles dynamic text with runtime values.

</domain>

<decisions>
## Implementation Decisions

### Dialogue Content
- Claude drafts all dialogue in English, user reviews. Claude also writes Spanish translations.
- Dialogue length varies by moment: intro is longer (5+ lines), pre-boss is short (1-2 taunt lines), post-boss medium (2-3 lines), shop dialogue is brief.
- Tone: mix — Raberto is humorous/quirky in shop and guidance scenes, but boss intros and encounters feel more dramatic.
- Speaking characters: Raberto + bosses speak. Each boss has taunt/intro lines. Player is silent protagonist.
- Boss names and personalities: Claude finds boss references in the codebase and uses whatever names/themes exist there.
- Boss names: localized (translated into Spanish where it makes sense — e.g. "Pepperoni Dragon" → "Dragón de Pepperoni").
- Spanish variant: neutral/universal Spanish — avoid region-specific terms, understandable everywhere.

### Key Naming & Coverage
- Localize everything in the shop: upgrade names, descriptions, state labels ("Max Wand Level"), token count label.
- Character names like "Raberto" stay hardcoded — proper nouns, not localization keys.
- Mystery placeholders like "???" stay hardcoded — universal symbols.
- Game Over and Credits screens: Claude scans codebase to find what exists and adds keys for all text found.
- Spell/element names: localize them (Spanish players should see Spanish spell names).

### Language Save Integration
- Keep PlayerPrefs as the storage mechanism — language is a user preference, not game progress. Simpler and persists without a save file.
- Default language: always start English. User switches to Spanish manually in Options.

### Dynamic Text Handling
- Format strings in JSON: e.g. `"tokens_count": "Tokens: {0}"`. Code calls `string.Format(GetText("tokens_count"), count)`.
- Dynamic text (tokens, health, mana) handled in code via `LocalizationManager.GetText()` + `string.Format()`. No LocalizedText component on dynamic elements — code-only approach.
- Immediate update on language switch: all text including dynamic HUD labels refreshes when language changes. Combat scripts subscribe to OnLanguageChanged.
- No special number formatting — numbers display the same in both languages.

### Claude's Discretion
- Exact key naming convention (prefix style like `shop_`, `combat_`, `boss_`)
- How to structure multi-line dialogues in JSON (array of entries vs numbered keys)
- Loading/error handling for missing keys
- Whether to add a helper method for format strings or keep it as raw string.Format calls

</decisions>

<specifics>
## Specific Ideas

- Raberto should feel like a quirky pizza-shop NPC — think Undertale-style humor for the shop scenes
- Boss encounters should feel dramatic enough to build tension before fights
- The existing `OnLanguageChanged` event pattern should be extended to dynamic text — same subscription model

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- `LocalizationManager` (Assets/Scripts/Languaje/LocalizationManager.cs): Singleton, loads JSON from Resources/Languages/, has `OnLanguageChanged` event, `GetText(key)` method. Fully functional — just needs more keys in JSON.
- `LocalizedText` (Assets/Scripts/Languaje/LocalizedText.cs): Component auto-refreshes on language change. Supports both TMP_Text and legacy Text. Uses coroutine to wait for manager. Ready to attach to static UI elements.
- `OptionsUI` (Assets/Scripts/UI/OptionsUI.cs): Already has language dropdown wired to `LocalizationManager.SetLanguage()`. Working end-to-end.

### Established Patterns
- JSON format: `{"entries": [{"key": "...", "value": "..."}]}` using JsonUtility with wrapper class. Must keep this format.
- Language stored as `PlayerPrefs.GetInt("Idioma", 0)` — 0=EN, 1=ES.
- Folder is named `Languaje` (Spanish spelling) — keep this to avoid breaking references.

### Integration Points
- **ShopUI.cs** (line 135, 240): Hardcoded "Max Wand Level" and "Tokens: 0" — replace with localized + format strings.
- **ElementSelectorUI.cs** (line 45): Hardcoded "Reset" — replace with localized key.
- **ElementSelectionUI.cs** (line 79): Hardcoded "Deselect All" — replace with localized key.
- **CombinationsUI.cs** (line 149, 152): "???" placeholders — keep hardcoded per decision.
- **DialogUI.cs** (line 167): Hardcoded "Raberto" speaker name — keep hardcoded per decision.
- Combat HUD scripts: Need to find health/mana/potion display scripts and wire in localized labels.
- Game Over / Credits screens: Need codebase scan to identify all text elements.

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 16-language-system-completion*
*Context gathered: 2026-02-28*
