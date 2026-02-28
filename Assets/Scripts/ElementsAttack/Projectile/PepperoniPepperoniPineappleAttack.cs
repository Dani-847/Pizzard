using System.Collections;
using UnityEngine;

public class PepperoniPepperoniPineappleAttack : CharacterProjectile
{
    [Header("Spawner Settings")]
    public GameObject pepperoniAttackPrefab; // Prefab de PepperoniAttack
    public float initialMoveTime = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPineapple.InitialMoveTime;
    public float stickDuration = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPineapple.StickDuration;
    public float spawnInterval = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPineapple.SpawnInterval;
    public float rotationSpeed = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPineapple.RotationSpeed; // grados por segundo mientras está fijo
    public float spawnedProjectileSpeed = Pizzard.Core.GameBalance.Spells.PepperoniPepperoniPineapple.SpawnedProjectileSpeed; // velocidad para los PepperoniAttack instanciados

    private bool isStuck = false;

    protected override void Start()
    {
        // No llamar base.Start() porque queremos control total de la física inicial
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        if (rb != null && speed > 0)
            rb.velocity = transform.right * speed;

        float minLifetime = initialMoveTime + stickDuration + 0.5f;
        float finalLifetime = Mathf.Max(lifetime, minLifetime);
        Destroy(gameObject, finalLifetime);

        StartCoroutine(LifeCycleRoutine());
    }

    void Update()
    {
        if (isStuck)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator LifeCycleRoutine()
    {
        // Avanza un poco
        yield return new WaitForSeconds(initialMoveTime);

        // Parar y empezar a girar
        if (rb != null) rb.velocity = Vector2.zero;
        isStuck = true;

        float elapsed = 0f;
        while (elapsed < stickDuration)
        {
            SpawnPepperoniAttack();
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }

        isStuck = false;
        Destroy(gameObject);
    }

    private void SpawnPepperoniAttack()
    {
        if (pepperoniAttackPrefab == null) return;

        // Dirección aleatoria
        float angleDeg = Random.Range(0f, 360f);
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.forward);
        GameObject go = Instantiate(pepperoniAttackPrefab, transform.position, rot);

        CharacterProjectile cp = go.GetComponent<CharacterProjectile>();
        if (cp != null)
        {
            cp.speed = spawnedProjectileSpeed;
            cp.Initialize(dir);
        }
        else
        {
            Debug.LogWarning("⚠️ El prefab instanciado no contiene CharacterProjectile/PepperoniAttack.");
        }
    }
}