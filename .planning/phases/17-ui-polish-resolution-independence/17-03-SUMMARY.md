# Plan 17.3: Mana Bar & Boss Health Bars Polish

## Tasks Completed
- **Mana Bar (`ManaUI`)**: Verified in the `UI.prefab` hierarchy that its RectTransform anchors are correctly set to `(1,0)` (Bottom-Right) with identical Pivot coordinates. This explicitly prevents arbitrary drifting on aspect ratio or resolution changes. Verified vertical fill (150px height) and color remains intact.
- **Boss Health Bar (`PblobUI`)**: Traced `BossHealthBarUI` logic to its active implementation on the Generic Canvas via `PblobUI`. Verified `PblobUI` anchors are correctly set to Top-Center `(0.5, 1.0)`. Secondary health bars (FrontHealthBar/BackHealthBar/Border) inherit cleanly without distortion.

## Status
- **Verification**: UI anchors confirmed programmatically on the generic UI canvas instances. Will scale robustly as verified by Canvas Scaler setup.
- **Completion**: 100%
