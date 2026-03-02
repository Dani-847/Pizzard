using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class SmallAbsorbingProjectile : PineappleCheeseProjectile
{
    protected override void Start()
    {
        base.Start();

        // Reduce range/stats per GameBalance constants
        absorptionRadius = GameBalance.Spells.PinaPinaQueso.SubAbsorbRadius;
        impactRadius = GameBalance.Spells.PinaPinaQueso.SubRadius;
        
        // Ensure scale starts small
        transform.localScale = Vector3.one * 0.5f;
    }
}
