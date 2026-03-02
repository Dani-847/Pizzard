using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;
using Pizzard.Bosses;

public class SmallBurningProjectile : CharacterProjectile
{
    [Header("Burning Settings")]
    public float burnDuration = GameBalance.Spells.PinaPinaPepperoni.BurnDuration;
    public int burnStacks = GameBalance.Spells.PinaPinaPepperoni.BurnStacks;
    public bool hasSpawned = false;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasSpawned)
        {
            ApplyBurn(other);
            base.OnTriggerEnter2D(other);
            return;
        }

        ApplyBurn(other);
        base.OnTriggerEnter2D(other);
    }

    private void ApplyBurn(Collider2D other)
    {
        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
        {
            StatusEffectSystem status = other.GetComponent<StatusEffectSystem>();
            if (status != null)
            {
                status.ApplyEffect(StatusType.picante, burnDuration, gameObject, burnStacks);
            }
        }
    }
}
