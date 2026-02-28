using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DelayedHealthBar : MonoBehaviour
{
    [Header("Health Bar References")]
    public Image redBar;          // Foreground moving immediately
    public Image orangeBar;       // Background moving slowly
    
    [Header("Animation Settings")]
    public float delayDuration = 0.5f;
    public float orangeBarSpeed = 0.5f;   // Speed in percent per second
    
    private float targetHealthRatio = 1f;
    private Coroutine orangeBarCoroutine;

    void Start()
    {
        if (redBar == null || orangeBar == null)
        {
            Image[] children = GetComponentsInChildren<Image>(true);
            foreach (var child in children)
            {
                string n = child.gameObject.name.ToLower();
                if (n.Contains("front") || n.Contains("red")) redBar = child;
                if (n.Contains("back") || n.Contains("orange")) orangeBar = child;
            }
        }

        if (redBar == null || orangeBar == null)
        {
            Debug.LogError("Health Bar Images are not assigned and could not be found!");
            return;
        }

        if (redBar != null)
        {
            redBar.type = Image.Type.Filled;
            redBar.fillMethod = Image.FillMethod.Horizontal;
            redBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if (orangeBar != null)
        {
            orangeBar.type = Image.Type.Filled;
            orangeBar.fillMethod = Image.FillMethod.Horizontal;
            orangeBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }

    public void SetHealth(float healthPercentage)
    {
        targetHealthRatio = Mathf.Clamp01(healthPercentage);

        if (redBar != null)
            redBar.fillAmount = targetHealthRatio;
        
        if (orangeBarCoroutine != null) StopCoroutine(orangeBarCoroutine);
        orangeBarCoroutine = StartCoroutine(AnimateOrangeBar(targetHealthRatio));
    }
    
    private IEnumerator AnimateOrangeBar(float targetRatio)
    {
        yield return new WaitForSeconds(delayDuration);
        
        while (orangeBar != null && Mathf.Abs(orangeBar.fillAmount - targetRatio) > 0.001f)
        {
            float currentRatio = orangeBar.fillAmount;
            float newRatio = Mathf.MoveTowards(currentRatio, targetRatio, orangeBarSpeed * Time.deltaTime);
            orangeBar.fillAmount = newRatio;
            yield return null;
        }
        
        if (orangeBar != null)
            orangeBar.fillAmount = targetRatio;
    }
    
    public void SetHealth(float current, float max)
    {
        SetHealth(current / max);
    }
}