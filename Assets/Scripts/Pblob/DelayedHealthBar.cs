using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DelayedHealthBar : MonoBehaviour
{
    [Header("Health Bar References")]
    public Image redBar;          // Barra roja (frente) - disminuye inmediato
    public Image orangeBar;       // Barra naranja (atrás) - disminuye con delay
    
    [Header("Animation Settings")]
    public float delayDuration = 0.5f;    // Tiempo antes de que empiece a moverse la naranja
    public float orangeBarSpeed = 1f;     // Velocidad de la barra naranja
    public float smoothTime = 0.3f;       // Suavizado del movimiento
    
    private float targetHealth = 1f;
    private Coroutine orangeBarCoroutine;
    
    void Start()
    {
        // Inicializar ambas barras llenas
        redBar.fillAmount = 1f;
        orangeBar.fillAmount = 1f;
        targetHealth = 1f;
    }
    
    public void SetHealth(float healthPercentage)
    {
        // Asegurarse de que el valor esté entre 0 y 1
        healthPercentage = Mathf.Clamp01(healthPercentage);
        
        // Actualizar salud objetivo
        targetHealth = healthPercentage;
        
        // Barra ROJA se actualiza inmediatamente
        redBar.fillAmount = targetHealth;
        
        // Programar movimiento de barra NARANJA
        if (orangeBarCoroutine != null)
            StopCoroutine(orangeBarCoroutine);
            
        orangeBarCoroutine = StartCoroutine(AnimateOrangeBar());
    }
    
    private IEnumerator AnimateOrangeBar()
    {
        // ESPERAR antes de empezar a mover la barra naranja
        yield return new WaitForSeconds(delayDuration);
        
        // ANIMAR barra naranja hasta alcanzar a la roja
        while (Mathf.Abs(orangeBar.fillAmount - redBar.fillAmount) > 0.01f)
        {
            // Mover suavemente la barra naranja hacia la posición de la roja
            orangeBar.fillAmount = Mathf.Lerp(
                orangeBar.fillAmount, 
                redBar.fillAmount, 
                orangeBarSpeed * Time.deltaTime
            );
            
            yield return null;
        }
        
        // Asegurar que sean iguales al final
        orangeBar.fillAmount = redBar.fillAmount;
    }
    
    // Método para actualizar con valores absolutos (opcional)
    public void SetHealth(float current, float max)
    {
        SetHealth(current / max);
    }
    
    void Update()
    {
        // Actualización en tiempo real si necesitas seguimiento constante
        // (opcional, depende de tu implementación)
    }
}