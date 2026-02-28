# Phase 12: Shop Progression, UI Anchors & Core Game Loop Rework

## Objective
Rebuild and polish the Shop UI, Player Stats UI, and Progression logic to perfectly match the mandatory game loop document, ensuring strict state progression, safe UI anchoring, and save mechanics.

## Strict Mandatory Rules to Implement:

### 1. Token & Economy System
- **Earnings**: Boss 1 drops 1 token, Bosses 2-4 drop 2 tokens each. Tokens accumulate and carry over.
- **UI Element**: Token counter must be visible in the Shop at all times and update live.
- **Wand Upgrades**: Tier 1 allows 1 element, Tier 2 allows 2 elements, Tier 3 allows 3 elements.
- **Fatigue Upgrades**: Each purchase increases `maxFatigue` by 1.5x. Visual bar remains identical, just drains slower.
- **Potion Upgrades**: Each purchase adds +1 to potion count indefinitely.

### 2. Shop 1 (Pre-Boss 1) Logic
- **Starting State**: 1 token, Wand Tier 0, Element selection grayed out and disabled.
- **Purchase Constraint**: Player CANNOT exit until Wand is upgraded to Tier 1.
- **After Tier 1 Purchase**: Element selector immediately highlights, letting the player pick 1 element. Exit button becomes enabled.

### 3. Shop 2+ (Pre-Boss 2-4) Logic
- **Multiple Choices**: Player can spend tokens on any mix of upgrades (Wand, Fatigue, Potion).
- **Exit Warning**: Exit button requires a double-click. First click opens a warning dialogue: *"If you leave now, you will face the boss without further upgrades."* Second click exits.

### 4. Boss Placeholder UI
- Bosses 2, 3, and 4 currently lack health bars.
- Must create Canvas Scaler-compatible placeholder bars (Unity UI Image, RectTransform, Colored fill) anchored correctly to screen bounds for these bosses before proceeding. 

### 5. UI Safe Anchoring & Element Selection
- All Canvas elements must use `Canvas Scaler (Scale With Screen Size)`.
- All RectTransforms must be securely anchored.
- Element selection must strictly respect Tier limits, offering a clear visual highlight and a "Deselect" functionality.

### 6. Save Flow
- Auto-save is triggered when a Boss is defeated and when the Shop is closed. 
- Save must accurately hold: Tokens, Wand Tier, Potion Count, Fatigue Max, Boss Index, Language Setting.

## Execution Steps (Waves)

### Wave 1 — Foundation
- GameState enum + state machine in GameManager
- Token data model + persistence
- Save/Load system updates

### Wave 2 — Shop Core
- Shop 1 hard-lock logic (exit blocked until Tier 1)
- Token counter UI
- Wand upgrade button + tier progression
- Fatigue upgrade button + maxFatigue * 1.5 logic
- Potion upgrade button
- Element UI reactive binding to wandTier
- Deselect button

### Wave 3 — Shop 2+ & Loop
- Double-click exit with warning dialogue + reset
- Boss token reward integration (Boss 1 = 1, Boss 2-4 = 2)
- Auto-save triggers on boss defeat + shop close

### Wave 4 — UI & Visual
- Canvas Scaler setup on all canvases
- RectTransform anchor fixes (all UI elements)
- BossHealthBarUI reusable prefab
- Fatigue bar UI (blue, left side, anchored)
- Screenshot verification requests
