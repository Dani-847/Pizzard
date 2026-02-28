using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Two-layer horizontal health bar — mirrors ManaUI approach but horizontal.
/// Orange layer drains slowly after a delay; Red (front) layer drains instantly.
/// Operates on RectTransform.sizeDelta.x — no FillAmount needed.
/// Auto-wires to FrontHealthBar / BackHealthBar children by name.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    [Header("Bar References (auto-found if empty)")]
    public RectTransform frontBar;    // Red   — drains instantly
    public RectTransform backBar;     // Orange — drains slowly after delay

    [Header("Animation")]
    public float delayDuration  = 0.5f;
    public float drainSpeed     = 120f;   // px/sec for orange bar

    private float fullWidth = -1f;
    private Coroutine drainRoutine;

    private void Start()
    {
        AutoFindBars();
        CacheFullWidth();
        SetImmediate(1f);         // Start bar fully filled
    }

    // ------------------------------------------------------------------ public API

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        AutoFindBars();
        CacheFullWidth();

        float targetPx = fullWidth * ratio;

        // Front bar: instant
        if (frontBar != null)
            SetWidth(frontBar, targetPx);

        // Orange bar: delayed drain
        if (drainRoutine != null) StopCoroutine(drainRoutine);
        drainRoutine = StartCoroutine(DrainOrange(targetPx));
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
            string n = rt.gameObject.name.ToLower();
            if (frontBar == null && (n.Contains("front") || n.Contains("red") || n.Contains("fill")))
                frontBar = rt;
            if (backBar == null && (n.Contains("back") || n.Contains("orange") || n.Contains("delay")))
                backBar = rt;
        }
    }

    private void CacheFullWidth()
    {
        if (fullWidth > 0f) return;               // already cached
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && rt.rect.width > 1f)
            fullWidth = rt.rect.width;

        // Fallback: read from child size
        if (fullWidth <= 0f && frontBar != null && frontBar.rect.width > 1f)
            fullWidth = frontBar.rect.width;

        if (fullWidth <= 0f) fullWidth = 300f;    // last-resort default
    }

    /// Set both bars immediately (used at init or when you want instant snap).
    private void SetImmediate(float ratio)
    {
        float px = fullWidth * Mathf.Clamp01(ratio);
        if (frontBar != null) SetWidth(frontBar, px);
        if (backBar  != null) SetWidth(backBar,  px);
    }

    private void SetWidth(RectTransform rt, float w)
    {
        rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
    }

    private IEnumerator DrainOrange(float targetPx)
    {
        yield return new WaitForSeconds(delayDuration);

        while (backBar != null)
        {
            float current = backBar.sizeDelta.x;
            if (Mathf.Abs(current - targetPx) < 0.5f) break;
            float next = Mathf.MoveTowards(current, targetPx, drainSpeed * Time.deltaTime);
            SetWidth(backBar, next);
            yield return null;
        }

        if (backBar != null) SetWidth(backBar, targetPx);
    }
}