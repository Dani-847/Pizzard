using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPiñaProjectile : CharacterProjectile
{
    [Header("Small Piña Settings")]
    public bool hasSpawned = false;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ EVITAR que los subproyectiles generen más proyectiles
        if (hasSpawned)
        {
            base.OnTriggerEnter2D(other);
            return;
        }

        base.OnTriggerEnter2D(other);
    }
}