using UnityEngine;
using Pizzard.Core;

public class PinaPepperoniPepperoniAttack : PineapplePepperoniAttack
{
    private void Awake()
    {
        explosionRadius = GameBalance.Spells.PinaPepperoniPepperoni.ExplosionRadius;
        explosionDamage = GameBalance.Spells.PinaPepperoniPepperoni.ExplosionDamage;
    }
}
