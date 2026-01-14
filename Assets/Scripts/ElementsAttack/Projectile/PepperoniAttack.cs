using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepperoniAttack : CharacterProjectile
{
    [Header("Pepperoni Effects")]
    public StatusType statusEffect = StatusType.picante;
    public float effectDuration = 7f;
    public int initialStacks = 2; // 2 cargas de picante como solicitaste

    protected override void Start()
    {
        // Llamar al Start() base primero
        base.Start();
        
        Debug.Log($"🌶️ PepperoniAttack inicializado: {initialStacks} stacks de {statusEffect} por {effectDuration}s");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🌶️ PepperoniAttack collision con: {other.name} (Tag: {other.tag})");

        // Aplicar efecto picante al BOSS
        if (other.CompareTag("Boss"))
        {
            StatusEffectSystem statusSystem = other.GetComponent<StatusEffectSystem>();
            if (statusSystem != null)
            {
                statusSystem.ApplyEffect(statusEffect, effectDuration, gameObject, initialStacks);
                Debug.Log($"🌶️ Pepperoni aplicó {initialStacks} cargas de efecto picante al Boss por {effectDuration} segundos");
            }
        }
        
        // Llamar a la lógica base para daño y destrucción
        base.OnTriggerEnter2D(other);
    }
}