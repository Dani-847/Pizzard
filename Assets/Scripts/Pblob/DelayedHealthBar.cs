using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar using anchor-clipping (same approach as ManaUI height scaling).
/// Children FrontHealthBar and BackHealthBar must have their RectTransform
/// set up as STRETCH (anchorMin.x=0, anchorMax.x=1, offsetMin.x=0, offsetMax.x=0).
/// We scale down anchorMax.x to clip the right side — no Image.fillAmount needed.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    [Header("Bar References (auto-found if empty)")]
    public RectTransform frontBar;    // Red   — clips instantly
    public RectTransform backBar;     // Orange — clamps slowly after delay

    [Header("Animation")]
    public float delayDuration  = 0.5f;
    public float drainSpeed     = 0.4f;   // anchor units/sec  (0..1 per second)

    private Coroutine drainRoutine;
    private float currentRatio = 1f;

    private void Start()
    {
        AutoFindBars();
        SetupBars();
        ApplyRatio(frontBar, 1f);
        ApplyRatio(backBar,  1f);
    }

    // ------------------------------------------------------------------ public API

    public void SetHealth(float ratio)
    {
        currentRatio = Mathf.Clamp01(ratio);
        AutoFindBars();

        // Front bar: instant
        if (frontBar != null) ApplyRatio(frontBar, currentRatio);

        // Orange bar: delayed drain
        if (drainRoutine != null) StopCoroutine(drainRoutine);
        if (gameObject.activeInHierarchy)
            drainRoutine = StartCoroutine(DrainBackBar(currentRatio));
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ------------------------------------------------------------------ internals

    private void AutoFindBars()
    {
        if (frontBar != null && backBar != null) return;

        foreach (RectTransform rt in GetComponentsInChildren<RectTransform>(true))
        {
            if (rt == GetComponent<RectTransform>()) continue; // skip self
            string n = rt.gameObject.name.ToLower();
            if (frontBar == null && (n.Contains("front") || n.Contains("red") || n.Contains("fill")))
                frontBar = rt;
            if (backBar == null && (n.Contains("back") || n.Contains("orange") || n.Contains("delay")))
                backBar = rt;
        }

        if (frontBar != null)
            Debug.Log($"[HealthBar] frontBar found: {frontBar.name}");
        else
            Debug.LogWarning("[HealthBar] frontBar NOT found — check child names contain 'front' or 'red'");
        if (backBar != null)
            Debug.Log($"[HealthBar] backBar found: {backBar.name}");
        else
            Debug.LogWarning("[HealthBar] backBar NOT found — check child names contain 'back' or 'orange'");
    }

    /// Ensure each child bar is set up for horizontal anchor clipping
    private void SetupBars()
    {
        SetupBar(frontBar);
        SetupBar(backBar);
    }

    private void SetupBar(RectTransform rt)
    {
        if (rt == null) return;
        // Left-anchor so only the right edge moves
        rt.anchorMin   = new Vector2(0f, 0f);
        rt.anchorMax   = new Vector2(1f, 1f);
        rt.offsetMin   = Vector2.zero;
        rt.offsetMax   = Vector2.zero;
    }

    /// Set the visible width ratio (0=empty, 1=full) via anchorMax.x clipping
    private void ApplyRatio(RectTransform rt, float ratio)
    {
        if (rt == null) return;
        rt.anchorMax = new Vector2(Mathf.Clamp01(ratio), rt.anchorMax.y);
    }

    private IEnumerator DrainBackBar(float targetRatio)
    {
        yield return new WaitForSeconds(delayDuration);

        while (backBar != null)
        {
            float cur = backBar.anchorMax.x;
            if (Mathf.Abs(cur - targetRatio) < 0.002f) break;
            float next = Mathf.MoveTowards(cur, targetRatio, drainSpeed * Time.deltaTime);
            ApplyRatio(backBar, next);
            yield return null;
        }

        if (backBar != null) ApplyRatio(backBar, targetRatio);
    }
}