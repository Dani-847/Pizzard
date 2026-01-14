using UnityEngine;

public class MovementCircle : MonoBehaviour
{
    [Header("Visual Feedback")]
    public SpriteRenderer circleRenderer;
    public Color safeColor = Color.green;
    public Color dangerousColor = Color.red;
    
    private bool isSafe = false;
    
    public void SetSafe(bool safe)
    {
        isSafe = safe;
        
        // CAMBIAR COLOR VISUAL
        if (circleRenderer != null)
        {
            circleRenderer.color = safe ? safeColor : dangerousColor;
        }
    }
    
    public bool IsSafe()
    {
        return isSafe;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isSafe)
        {
            // DAÑO AL JUGADOR SI ESTÁ FUERA DEL CÍRCULO SEGURO
            Debug.Log("¡Jugador fuera del área segura!");
            // AQUÍ APLICARÍAS DAÑO AL JUGADOR
            // Ej: other.GetComponent<PlayerHealth>().TakeDamage(10);
        }
    }
}