using System.Collections;
using UnityEngine;

public class PepperoniPepperoniQuesoAttack : CharacterProjectile
{
    [Header("Pepperoni Queso Settings")]
    public GameObject direTrailPrefab;
    public float initialMoveTime = 0.1f;
    public float stickDuration = 2f;
    public float spawnInterval = 0.2f;
    public float spawnAreaRadius = 3f;    // mayor área para queso

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "CharacterProjectile";

        if (rb != null && speed > 0)
            rb.velocity = transform.right * speed;

        float minLifetime = initialMoveTime + stickDuration + 0.5f;
        float finalLifetime = Mathf.Max(lifetime, minLifetime);
        Destroy(gameObject, finalLifetime);

        StartCoroutine(LifeCycleRoutine());
    }

    private IEnumerator LifeCycleRoutine()
    {
        yield return new WaitForSeconds(initialMoveTime);

        if (rb != null)
            rb.velocity = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < stickDuration)
        {
            SpawnDireTrail();
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }

        Destroy(gameObject);
    }

    private void SpawnDireTrail()
    {
        if (direTrailPrefab == null) return;

        Vector2 offset = Random.insideUnitCircle * spawnAreaRadius;
        Vector3 spawnPos = transform.position + (Vector3)offset;
        GameObject go = Instantiate(direTrailPrefab, spawnPos, Quaternion.identity);

        FireTrail ft = go.GetComponent<FireTrail>();
        if (ft != null)
        {
            // Aumentar radio y duración para efecto "queso"
            ft.Initialize(1, 0.3f, 2f, 2f, 4f);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar colisiones con FireTrail para que el proyectil no desaparezca al pasar por zonas de quemado
        if (other.CompareTag("FireTrail"))
            return;

        base.OnTriggerEnter2D(other);
    }
}