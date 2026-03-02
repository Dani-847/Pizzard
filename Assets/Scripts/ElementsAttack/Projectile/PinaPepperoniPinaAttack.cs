using UnityEngine;
using Pizzard.Core;

public class PinaPepperoniPinaAttack : PineapplePepperoniAttack
{
    private void Awake()
    {
        explosionRadius = GameBalance.Spells.PinaPepperoniPina.ExplosionRadius;
        explosionDamage = GameBalance.Spells.PinaPepperoniPina.ExplosionDamage;
    }
}
