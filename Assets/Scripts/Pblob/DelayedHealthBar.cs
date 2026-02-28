using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DelayedHealthBar : MonoBehaviour
{
    [Header("Health Bar References")]
    public RectTransform redBarRect;      // Foreground moving immediately
    public RectTransform orangeBarRect;   // Background moving slowly
    
    [Header("Animation Settings")]
    public float delayDuration = 0.5f;
    public float orangeBarSpeed = 0.5f;   // Speed in percent per second
    
    private float targetHealthRatio = 1f;
    private Coroutine orangeBarCoroutine;

    void Start()
    {
        if (redBarRect == null || orangeBarRect == null)
        {
            RectTransform[] children = GetComponentsInChildren<RectTransform>(true);
            foreach (var child in children)
            {
                string n = child.gameObject.name.ToLower();
                if (n.Contains("front") || n.Contains("red")) redBarRect = child;
                if (n.Contains("back") || n.Contains("orange")) orangeBarRect = child;
            }
        }

        if (redBarRect == null || orangeBarRect == null)
        {
            Debug.LogError("Health Bar RectTransforms are not assigned and could not be found!");
            return;
        }

        // Setup Anchors so changing anchorMax.x stretches them correctly
        SetupAnchor(redBarRect);
        SetupAnchor(orangeBarRect);
    }
    
    private void SetupAnchor(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void SetHealth(float healthPercentage)
    {
        targetHealthRatio = Mathf.Clamp01(healthPercentage);

        // Animate Red bar immediately
        redBarRect.anchorMax = new Vector2(targetHealthRatio, 1f);
        
        // Schedule Orange bar
        if (orangeBarCoroutine != null) StopCoroutine(orangeBarCoroutine);
        orangeBarCoroutine = StartCoroutine(AnimateOrangeBar(targetHealthRatio));
    }
    
    private IEnumerator AnimateOrangeBar(float targetRatio)
    {
        yield return new WaitForSeconds(delayDuration);
        
        while (Mathf.Abs(orangeBarRect.anchorMax.x - targetRatio) > 0.001f)
        {
            float currentRatio = orangeBarRect.anchorMax.x;
            float newRatio = Mathf.MoveTowards(currentRatio, targetRatio, orangeBarSpeed * Time.deltaTime);
            orangeBarRect.anchorMax = new Vector2(newRatio, 1f);
            yield return null;
        }
        
        orangeBarRect.anchorMax = new Vector2(targetRatio, 1f);
    }
    
    public void SetHealth(float current, float max)
    {
        SetHealth(current / max);
    }
}