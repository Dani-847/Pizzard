using UnityEngine;
using System.Collections;
using Pizzard.Core;

public class PinaPepperoniQuesoAttack : PineapplePepperoniAttack
{
    public float reflectionSpeedMultiplier = GameBalance.Spells.PinaPepperoniQueso.ReflectionSpeedMultiplier;

    private void Awake()
    {
        explosionRadius = GameBalance.Spells.PinaPepperoniQueso.ExplosionRadius;
        explosionDamage = GameBalance.Spells.PinaPepperoniQueso.ExplosionDamage;
    }

    protected override void CreateExplosion(Vector3 position)
    {
        // LLamamos a la base para la explosión normal
        base.CreateExplosion(position);

        // Reflejar proyectiles
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (Collider2D collider in hits)
        {
            if (collider.CompareTag("EnemyProjectile"))
            {
                ReflectProjectile(collider.gameObject, position);
            }
        }
    }

    private void ReflectProjectile(GameObject projectile, Vector3 centerPosition)
    {
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Invertir la velocidad o enviarlo lejos del centro
            Vector2 reflectDirection = (projectile.transform.position - centerPosition).normalized;
            if (reflectDirection == Vector2.zero) reflectDirection = Vector2.up;

            float currentSpeed = rb.velocity.magnitude;
            if (currentSpeed < 1f) currentSpeed = 5f; // Velocidad mínima

            // Asignamos la nueva velocidad
            rb.velocity = reflectDirection * (currentSpeed * reflectionSpeedMultiplier);
            
            // Cambiar el tag para que pueda dañar a los enemigos
            projectile.tag = "PlayerProjectile";
            projectile.layer = LayerMask.NameToLayer("PlayerAttacks"); 
            
            // Opcional: Desactivar la lógica de EnemyProjectile temporalmente si existe
            MonoBehaviour[] scripts = projectile.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                // Si es el script base de proyectil enemigo que pueda tener
                if (script.GetType().Name.Contains("EnemyProjectile"))
                {
                    StartCoroutine(DisableScriptTemporarily(script, 0.5f));
                }
            }

            Debug.Log($"🪞 Proyectil {projectile.name} reflejado por explosión!");
        }
    }

    private IEnumerator DisableScriptTemporarily(MonoBehaviour script, float duration)
    {
        if (script != null)
        {
            script.enabled = false;
            yield return new WaitForSeconds(duration);
            if (script != null)
            {
                script.enabled = true;
            }
        }
    }
}
