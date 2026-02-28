using System.Collections;
using UnityEngine;

public class PepperoniProjectile : CharacterProjectile
{
    [Header("Pepperoni Settings")]
    public GameObject direTrailPrefab;
    public float initialMoveTime = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPepperoni.InitialMoveTime;
    public float stickDuration = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPepperoni.StickDuration;          // tiempo que se queda en el sitio
    public float spawnInterval = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPepperoni.SpawnInterval;        // cada 0.2s dispara un diretrail
    public float spawnAreaRadius = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPepperoni.SpawnAreaRadius;        // radio del área donde aparecerán los diretrails

    protected override void Start()
    {
        // Reimplementamos Start para controlar el tiempo de vida y el comportamiento
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        if (rb != null && speed > 0)
        {
            rb.velocity = transform.right * speed;
        }

        // Asegurar que no se destruya antes de tiempo
        float minLifetime = initialMoveTime + stickDuration + 0.5f;
        float finalLifetime = Mathf.Max(lifetime, minLifetime);
        Destroy(gameObject, finalLifetime);

        StartCoroutine(LifeCycleRoutine());
    }

    private IEnumerator LifeCycleRoutine()
    {
        // Dejar que avance un poco
        yield return new WaitForSeconds(initialMoveTime);

        // Parar en el sitio
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Durante stickDuration, cada spawnInterval crear un diretrail en un punto aleatorio dentro del radio
        float elapsed = 0f;
        while (elapsed < stickDuration)
        {
            SpawnDireTrail();
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }

        // Terminar el proyectil
        Destroy(gameObject);
    }

    private void SpawnDireTrail()
    {
        if (direTrailPrefab == null) return;

        Vector2 offset = Random.insideUnitCircle * spawnAreaRadius;
        Vector3 spawnPos = transform.position + (Vector3)offset;
        GameObject go = Instantiate(direTrailPrefab, spawnPos, Quaternion.identity);

        // Si el prefab usa FireTrail, opcionalmente configurarlo aquí
        FireTrail ft = go.GetComponent<FireTrail>();
        if (ft != null)
        {
            // Valores ajustables: charges, damageInterval, duration, radius, effectDuration
            ft.Initialize(1, 0.3f, 1f, 0.5f, 2f);
        }
    }
}
