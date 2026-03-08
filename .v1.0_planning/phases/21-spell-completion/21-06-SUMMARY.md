# Phase 21, Plan 06 - Summary

## Objective Completed
Implemented the three T3 piña|pepperoni teleport variants.

## Changes Made
- Modified `PineapplePepperoniAttack.cs` base class to allow subclass overriding by making `CreateExplosion` protected virtual and updating its logic to support `BossBase.TakeDamage`.
- Created `PinaPepperoniPinaAttack.cs`: Replaces explosion constants with its specific ones.
- Created `PinaPepperoniPepperoniAttack.cs`: Replaces explosion constants with bigger radius and damage.
- Created `PinaPepperoniQuesoAttack.cs`: Expands the explosion effect to also reflect `EnemyProjectile` objects caught in the blast, pushing them away from the center.
- Duplicated `piñapepperoniAttack.prefab` into three new prefabs (`PinaPepperoniPinaAttack.prefab`, `PinaPepperoniPepperoniAttack.prefab`, `PinaPepperoniQuesoAttack.prefab`) and assigned the new scripts.
- Added all three combinations to `CombinationDatabase.asset`.

## Next Steps
Proceed to Plan 07.
