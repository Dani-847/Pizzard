# Summary: Plan 18.5 — Boss Fixes (Moustache Points, Grid, Damage)

## Status: COMPLETE ✅

## What Was Done

1. **Phase 1 — Moustache points detached from boss**
   - `PblobAttackPattern1.Start()`: iterate `mustachePoints`, call `SetParent(null)` on each
   - Hairball spawn points are now world-fixed; boss movement no longer shifts them

2. **Phase 2 — Timer penalty damage**
   - Verified `penaltyDamage` value and `RecibirDaño` call path
   - Added `Debug.Log` to trace penalty firing

3. **Phase 3 — Grid resized to 20×5**
   - `PblobGridPuzzle.cs`: `gridWidth = 20`, `gridHeight = 5`, `tileSize = 1.0f`
   - Grid now fills the arena width compactly

4. **Phase 3 — Red tile collisions**
   - `PblobGridTile.Awake()`: dynamically ensures `BoxCollider2D` exists with `isTrigger = true`
   - Stepping on red tiles now correctly triggers damage/slow

5. **Phase 3 — Boss contact vulnerability**
   - Replaced `OnTriggerEnter/Exit2D` with `OnCollisionEnter/Exit2D` in `PblobController.cs`
   - Boss collider is solid; collision events now correctly toggle vulnerability on player contact
