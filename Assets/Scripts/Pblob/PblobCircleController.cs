using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2 floor circles.
/// - Generates a circle sprite if none is assigned.
/// - Uses a Trigger collider so the player walks inside (not blocked).
/// - Rigidbody2D gravity is zeroed out so circles float in place.
/// - MoveRandomly coroutine guards against destroyed-object exceptions.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class PblobCircleController : MonoBehaviour
{
    public enum CircleType { Red, Green }
    public CircleType type;

    private SpriteRenderer sr;
    public bool playerInside = false;
    private Coroutine moveRoutine;

    // ------------------------------------------------------------------ lifecycle

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // ---- Sprite: generate procedural circle if no sprite assigned ----
        if (sr.sprite == null)
        {
            const int SIZE = 128;
            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            Color[] px = new Color[SIZE * SIZE];
            Vector2 c = new Vector2(SIZE / 2f, SIZE / 2f);
            float r = SIZE / 2f;
            for (int y = 0; y < SIZE; y++)
                for (int x = 0; x < SIZE; x++)
                    px[y * SIZE + x] = Vector2.Distance(c, new Vector2(x, y)) <= r ? Color.white : Color.clear;
            tex.SetPixels(px);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), (float)SIZE / 2f);
        }

        // Render above the arena tilemap
        sr.sortingOrder = 15;

        // ---- Collider: make it a trigger so player walks inside ----
        var col = GetComponent<CircleCollider2D>();
        if (col != null) col.isTrigger = true;

        // ---- Rigidbody2D: disable gravity so circles float ----
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale  = 0f;
            rb.isKinematic   = true;   // position driven by script, not physics
            rb.constraints   = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void Start()
    {
        SetColor(Color.white);    // start hidden / white
    }

    private void OnDestroy()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
    }

    // ------------------------------------------------------------------ triggers

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        RevealColor();

        if (type == CircleType.Green)
        {
            var boss = FindObjectOfType<PblobController>();
            if (boss != null && boss.currentState == PblobController.PblobState.Phase2)
                boss.MakeVulnerable();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        SetColor(Color.white);

        if (type == CircleType.Green)
        {
            var boss = FindObjectOfType<PblobController>();
            if (boss != null && boss.currentState == PblobController.PblobState.Phase2)
                boss.MakeInvulnerable();
        }
    }

    // ------------------------------------------------------------------ movement

    /// Called by PblobController during Phase 2 to make circles wander for `duration` seconds.
    public Coroutine StartMoving(float duration, float speed)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRandomly(duration, speed));
        return moveRoutine;
    }

    public IEnumerator MoveRandomly(float duration, float speed)
    {
        var boss = FindObjectOfType<PblobController>();
        if (boss == null) yield break;

        Vector3 center = boss.arenaCenter;
        float clampX   = boss.arenaClampX;
        float clampY   = boss.arenaClampY;
        float wanderR  = Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius;

        Vector3 targetPos = RandomTarget(center, clampX, clampY, wanderR);
        float   elapsed   = 0f;

        // Allow kinematic movement during wander
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation; // allow XY movement

        while (elapsed < duration)
        {
            // Guard: abort if this object was destroyed
            if (this == null || gameObject == null) yield break;

            if (Vector3.Distance(transform.position, targetPos) < 0.15f)
                targetPos = RandomTarget(center, clampX, clampY, wanderR);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Freeze in place after wander finishes
        if (this != null && gameObject != null)
        {
            if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    // ------------------------------------------------------------------ helpers

    private Vector3 RandomTarget(Vector3 center, float clampX, float clampY, float radius)
    {
        Vector3 t = center + (Vector3)(Random.insideUnitCircle.normalized * radius);
        t.x = Mathf.Clamp(t.x, center.x - clampX, center.x + clampX);
        t.y = Mathf.Clamp(t.y, center.y - clampY, center.y + clampY);
        return t;
    }

    private void RevealColor()
    {
        if (type == CircleType.Red)   SetColor(Color.red);
        if (type == CircleType.Green) SetColor(Color.green);
    }

    private void SetColor(Color c)
    {
        if (sr != null) sr.color = c;
    }
}
