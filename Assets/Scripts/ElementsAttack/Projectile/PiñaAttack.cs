using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiñaAttack : CharacterProjectile
{
    // Este es un proyectil simple que solo hace daño al impactar
    // No genera subproyectiles

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🍍 Piña simple colisionó con: {other.name} (Tag: {other.tag})");

        // ✅ EVITAR colisiones con el jugador y sus partes
        if (other.CompareTag("Player") || other.CompareTag("CastPoint") || other.CompareTag("Weapon") || other.CompareTag("CharacterProjectile"))
        {
            Debug.Log($"🛡️ Piña simple ignorada por: {other.tag}");
            return;
        }

        // ✅ Llamar a la lógica base para el daño y destrucción
        base.OnTriggerEnter2D(other);
    }
}