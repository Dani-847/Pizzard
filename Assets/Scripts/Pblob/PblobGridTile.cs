using System.Collections;
using UnityEngine;
using Pizzard.Core;

public class PblobGridTile : MonoBehaviour
{
    public bool isSafePath = false;
    private SpriteRenderer spriteRenderer;
    private Coroutine damageCoroutine;
    private Pizzard.Player.PlayerController playerMove;
    private PlayerHPController playerHealth;

    private float originalSpeed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Generate a solid square sprite if none assigned
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            const int SIZE = 64;
            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            Color[] px = new Color[SIZE * SIZE];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            // Create sprite with PPU equal to SIZE so it perfectly equates to 1x1 Unity World Units
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), pixelsPerUnit: SIZE, 0, SpriteMeshType.FullRect);
        
            // Force rendering behind the player
            spriteRenderer.sortingLayerName = "Background";
            spriteRenderer.sortingOrder = -5;
        }

        var col = GetComponent<BoxCollider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        // Make the tile collider slightly smaller so you have to actually step inside it, not just brush the edge
        col.size = new Vector2(0.7f, 0.7f);

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
            playerHealth = collision.GetComponent<PlayerHPController>();

            // Apply slow debuff
            if (playerMove != null)
            {
                originalSpeed = playerMove.moveSpeed;
                playerMove.moveSpeed = GameBalance.Player.MoveSpeed * 0.5f;
            }

            if (playerHealth != null)
                damageCoroutine = StartCoroutine(DamageTickRoutine());
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
                playerHealth.RecibirDaño(damage);
            yield return new WaitForSeconds(tickRate);
        }
    }
}
