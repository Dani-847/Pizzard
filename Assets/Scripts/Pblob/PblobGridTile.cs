using System.Collections;
using UnityEngine;
using Pizzard.Core;

public class PblobGridTile : MonoBehaviour
{
    public bool isSafePath = false;
    private SpriteRenderer spriteRenderer;
    private Coroutine damageCoroutine;
    private Pizzard.Player.PlayerController playerMove;
    private Pizzard.Player.PlayerHealth playerHealth;

    private float originalSpeed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetColor(Color.gray);
    }

    public void SetColor(Color c)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = c;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isSafePath)
        {
            playerMove = collision.GetComponent<Pizzard.Player.PlayerController>();
            playerHealth = collision.GetComponent<Pizzard.Player.PlayerHealth>();

            // Apply slow debuff
            if (playerMove != null)
            {
                originalSpeed = playerMove.moveSpeed;
                playerMove.moveSpeed = GameBalance.Player.MoveSpeed * 0.5f; // Slow down
            }

            // Start ticking damage
            if (playerHealth != null)
            {
                damageCoroutine = StartCoroutine(DamageTickRoutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isSafePath)
        {
            // Remove slow debuff
            if (playerMove != null)
            {
                playerMove.moveSpeed = GameBalance.Player.MoveSpeed; // Restore global default
            }

            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator DamageTickRoutine()
    {
        float tickRate = 1f;
        int damage = Mathf.RoundToInt(GameBalance.Bosses.Pblob.GridDamagePerTick);

        while (true)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            yield return new WaitForSeconds(tickRate);
        }
    }
}
