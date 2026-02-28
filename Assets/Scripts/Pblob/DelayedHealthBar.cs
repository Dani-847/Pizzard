using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar.
/// Uses Image.fillAmount on FrontHealthBar (red, instant) and
/// BackHealthBar (orange, delayed drain) — as nature intended.
/// Auto-finds children by name. Works regardless of anchor setup.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    [Header("Bar References (auto-found by name if left empty)")]
    public Image frontBar;   // Red   — drains instantly
    public Image backBar;    // Orange — drains with delay

    [Header("Animation")]
    public float delayDuration = 0.5f;
    public float drainSpeed    = 0.35f;   // fillAmount units/sec

    private Coroutine drainRoutine;

    private void Start()
    {
        AutoFindBars();
        InitBars();
    }

    // ------------------------------------------------------------------ public API

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        AutoFindBars();
        InitBars();

        if (frontBar != null) frontBar.fillAmount = ratio;

        if (drainRoutine != null) StopCoroutine(drainRoutine);
        if (gameObject.activeInHierarchy)
            drainRoutine = StartCoroutine(DrainBack(ratio));
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ------------------------------------------------------------------ internals

    private void AutoFindBars()
    {
        if (frontBar != null && backBar != null) return;

        foreach (Image img in GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject == gameObject) continue;  // skip self if any
            string n = img.gameObject.name.ToLower();
            if (frontBar == null && (n.Contains("front") || n.Contains("red")))
                frontBar = img;
            else if (backBar == null && (n.Contains("back") || n.Contains("orange")))
                backBar = img;
        }
    }

    private void InitBars()
    {
        SetupImage(frontBar);
        SetupImage(backBar);
    }

    private void SetupImage(Image img)
    {
        if (img == null) return;
        img.type         = Image.Type.Filled;
        img.fillMethod   = Image.FillMethod.Horizontal;
        img.fillOrigin   = (int)Image.OriginHorizontal.Left;
        img.fillAmount   = 1f;
    }

    private IEnumerator DrainBack(float target)
    {
        yield return new WaitForSeconds(delayDuration);

        while (backBar != null && Mathf.Abs(backBar.fillAmount - target) > 0.002f)
        {
            backBar.fillAmount = Mathf.MoveTowards(backBar.fillAmount, target, drainSpeed * Time.deltaTime);
            yield return null;
        }

        if (backBar != null) backBar.fillAmount = target;
    }
}