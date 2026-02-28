using System.Collections;
using UnityEngine;

public class PblobCircleController : MonoBehaviour
{
    public enum CircleType { Red, Green }
    public CircleType type;

    private SpriteRenderer spriteRenderer;
    public bool playerInside = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            Texture2D tex = new Texture2D(128, 128);
            Color[] pixels = new Color[128 * 128];
            Vector2 center = new Vector2(64, 64);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    pixels[y * 128 + x] = Vector2.Distance(center, new Vector2(x, y)) <= 64f ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 64f);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 15; // Force render over the arena tilemap
        }
    }

    private void Start()
    {
        // Start hidden (White)
        SetColor(Color.white);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            RevealColor();

            if (type == CircleType.Green)
            {
                PblobController boss = FindObjectOfType<PblobController>();
                if (boss != null && boss.currentState == PblobController.PblobState.Phase2)
                {
                    boss.MakeVulnerable();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            SetColor(Color.white);

            if (type == CircleType.Green)
            {
                PblobController boss = FindObjectOfType<PblobController>();
                if (boss != null && boss.currentState == PblobController.PblobState.Phase2)
                {
                    boss.MakeInvulnerable();
                }
            }
        }
    }

    private void RevealColor()
    {
        if (type == CircleType.Red)
            SetColor(Color.red);
        else if (type == CircleType.Green)
            SetColor(Color.green);
    }

    private void SetColor(Color c)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = c;
    }

    public IEnumerator MoveRandomly(float duration, float speed)
    {
        global::PblobController boss = FindObjectOfType<global::PblobController>();
        if (boss == null) yield break;

        Vector3 center = boss.arenaCenter;
        float clampX = boss.arenaClampX;
        float clampY = boss.arenaClampY;

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = center + (Vector3)(Random.insideUnitCircle.normalized * Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius);
        targetPos.x = Mathf.Clamp(targetPos.x, center.x - clampX, center.x + clampX);
        targetPos.y = Mathf.Clamp(targetPos.y, center.y - clampY, center.y + clampY);

        while (elapsed < duration)
        {
            // Pick a new target if we arrived
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                startPos = transform.position;
                targetPos = center + (Vector3)(Random.insideUnitCircle.normalized * Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius);
                targetPos.x = Mathf.Clamp(targetPos.x, center.x - clampX, center.x + clampX);
                targetPos.y = Mathf.Clamp(targetPos.y, center.y - clampY, center.y + clampY);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
