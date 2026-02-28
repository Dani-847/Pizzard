using System.Collections;
using UnityEngine;

public class PblobCircleController : MonoBehaviour
{
    public enum CircleType { Red, Green }
    public CircleType type;

    private SpriteRenderer spriteRenderer;
    private bool playerInside = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)(Random.insideUnitCircle.normalized * Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius);

        while (elapsed < duration)
        {
            // Pick a new target if we arrived
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                startPos = transform.position;
                targetPos = startPos + (Vector3)(Random.insideUnitCircle.normalized * Pizzard.Core.GameBalance.Bosses.Pblob.WanderRadius);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
