using UnityEngine;

public class HairTile : MonoBehaviour
{
    [Header("Visual References")]
    public SpriteRenderer tileRenderer;
    public Sprite safeSprite;
    public Sprite dangerousSprite;
    public GameObject hairEffect; // EFECTO VISUAL OPCIONAL
    
    private bool hasHair = false;
    private bool isRevealed = false;
    
    public void SetHasHair(bool hasHair)
    {
        this.hasHair = hasHair;
        UpdateVisual();
    }
    
    public void RevealSafe()
    {
        if (!hasHair)
        {
            isRevealed = true;
            if (tileRenderer != null)
                tileRenderer.sprite = safeSprite;
        }
    }
    
    public void RevealDangerous()
    {
        if (hasHair)
        {
            isRevealed = true;
            if (tileRenderer != null)
                tileRenderer.sprite = dangerousSprite;
            if (hairEffect != null)
                hairEffect.SetActive(true); // ACTIVAR EFECTO VISUAL
        }
    }
    
    public void HideHair()
    {
        isRevealed = false;
        // VOLVER AL SPRITE POR DEFECTO
        if (hairEffect != null)
            hairEffect.SetActive(false);
    }
    
    private void UpdateVisual()
    {
        if (isRevealed)
        {
            if (hasHair)
            {
                RevealDangerous();
            }
            else
            {
                RevealSafe();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && hasHair && !isRevealed)
        {
            // DAÑO AL JUGADOR POR PISAR LOSA CON PELO
            Debug.Log("¡Jugador pisó losa con pelo!");
            RevealDangerous(); // MOSTRAR QUE ERA PELIGROSA
            
            // AQUÍ APLICAR DAÑO AL JUGADOR
            // Ej: other.GetComponent<PlayerHealth>().TakeDamage(15);
        }
    }
}