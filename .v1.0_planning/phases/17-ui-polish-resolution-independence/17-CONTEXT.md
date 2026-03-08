# Phase 17: UI Polish & Resolution Independence - Context

**Gathered:** 2026-02-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Final UI pass across all scenes: enforce Scale With Screen Size on every Canvas, verify no clipping at four target resolutions (1920×1080, 1366×768, 2560×1440, 1280×720), and apply visual polish to Shop buttons, element selection, mana bar, boss health bars, and dialogue box layering. No new features — only correctness and polish on existing UI.

</domain>

<decisions>
## Implementation Decisions

### Canvas scale mode
- Every Canvas must use Scale With Screen Size (1920×1080 reference, match 0.5)
- `CanvasScalerSetup.cs` already exists — run `GSD > Setup Canvas Scalers` as the first step; verify and fix anything it misses
- After running the tool, do a manual pass to confirm no Canvas was skipped

### Shop buttons
- Uniform button sizes and spacing — all 4 upgrade buttons (wand, potion, mana, exit) must match
- Add hover and press color tint feedback (Unity Button ColorBlock, no custom components needed)
- No icons or additional visual elements — just sizing + interaction states

### Element selection layout
- Tier-based slot count already works logically (Tier 1 = 1 slot, Tier 2 = 2, Tier 3 = 3)
- Polish goal: cleaner button sizing and consistent spacing
- No locked/unlocked slot indicators — slots simply don't appear if not unlocked (current behavior is correct)

### Mana bar
- Current implementation (vertical fill, bottom→top, teal) is correct — do not change color logic
- Polish goal: verify correct sizing and positioning at all four target resolutions
- No low-mana warning color, no gradient — keep it simple

### Boss health bars
- BossHealthBarUI is generic and shared across all 4 boss scenes — visual consistency is already implied
- Polish goal: verify bars are correctly positioned and sized across all boss scenes at target resolutions
- No design changes — just anchor/sizing correctness

### Dialogue box overlap
- DialogUI is a full-screen overlay — during non-combat scenes this is fine
- There are currently no dialogues during boss fights, and no plans for them in the near term
- Decision deferred: if future phases add combat dialogue, the rule will be "center-only — bars stay visible at edges"
- This phase: no overlap fix needed (not a real problem yet)

### Claude's Discretion
- Exact pixel values for button sizes and padding (match visually, no specific spec)
- Whether to fix anchors via Unity MCP tools or an Editor script (follow whatever worked in Phase 16)
- Order of resolution testing (suggested: native 1920×1080 first, then downscale)

</decisions>

<specifics>
## Specific Ideas

- No specific references — standard Unity UI conventions apply
- CanvasScalerSetup already handles the bulk of the Canvas scale work; don't duplicate it

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Editor/CanvasScalerSetup.cs` — bulk Canvas scale mode fixer, run via `GSD > Setup Canvas Scalers`
- `Assets/Scripts/UI/ManaUI.cs` — already correct vertical fill; only needs resize verification
- `Assets/Scripts/UI/BossHealthBarUI.cs` — generic, used by all 4 boss scenes; one fix applies everywhere
- `Assets/Scripts/UI/ElementSelectionUI.cs` — tier-based button generation works; container layout is the only polish target

### Established Patterns
- All UI scripts check `LocalizationManager.Instance` before setting text — localization already integrated
- ShopUI auto-creates token counter at runtime if not wired in Inspector (fragile — Inspector wiring preferred)
- BossHealthBarUI subscribes to `OnLanguageChanged` for boss name updates

### Integration Points
- Shop scene: ShopUI + ElementSelectionUI + EquipSelectorUI all in same canvas — spacing changes must not break their relative layout
- UIManager canvas hosts DialogUI, CharacterHPUI, ManaUI, PotionUI — anchoring fixes here affect all overlaid HUD elements

</code_context>

<deferred>
## Deferred Ideas

- Combat dialogue overlap rule (bars stay visible at edges) — relevant only when a future phase adds boss-fight dialogue
- Controller navigation on UI elements — Phase 29

</deferred>

---

*Phase: 17-ui-polish-resolution-independence*
*Context gathered: 2026-02-28*
