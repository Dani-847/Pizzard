using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar — rebuilds its own bars at runtime as fresh solid-color rectangles.
/// Keeps PblobBorderHealthBar as the visual frame.
/// Destroys old sprite-based FrontHealthBar / BackHealthBar and replaces them
/// with clean Image.Type.Filled objects that respond correctly to fillAmount.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    [Header("Animation")]
    public float delayDuration = 0.5f;
    public float drainSpeed    = 0.4f;   // fillAmount units/sec

    // Built at runtime
    private Image frontImg;   // Red  — instant
    private Image backImg;    // Orange — delayed

    private static readonly Color RedColor    = new Color(0.85f, 0.10f, 0.10f, 1f);
    private static readonly Color OrangeColor = new Color(0.95f, 0.55f, 0.10f, 1f);

    private Coroutine drainRoutine;

    private void Start()
    {
        BuildBars();
        SetImmediate(1f);
    }

    // ------------------------------------------------------------------ public API

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        if (frontImg == null) BuildBars();
        frontImg.fillAmount = ratio;

        if (drainRoutine != null) StopCoroutine(drainRoutine);
        if (gameObject.activeInHierarchy)
            drainRoutine = StartCoroutine(DrainBack(ratio));
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ------------------------------------------------------------------ internals

    private void BuildBars()
    {
        // --- Step 1: Remove old sprite-based bars ---
        RemoveOldBar("FrontHealthBar");
        RemoveOldBar("BackHealthBar");

        // Step 2: Find the border (keep it, re-use its RectTransform for sizing reference)
        RectTransform border = null;
        foreach (RectTransform rt in GetComponentsInChildren<RectTransform>(true))
        {
            if (rt.gameObject.name == "PblobBorderHealthBar") { border = rt; break; }
        }

        // Step 3: Build back (orange) bar first so it renders BELOW front
        backImg  = CreateBar("HealthBackBar",  OrangeColor, border);
        frontImg = CreateBar("HealthFrontBar", RedColor,    border);

        // Step 4: Bring border to top so it renders over both bars
        if (border != null)
            border.SetAsLastSibling();
    }

    private void RemoveOldBar(string childName)
    {
        Transform t = transform.Find(childName);
        if (t != null) Destroy(t.gameObject);
    }

    /// Creates a Filled Image child that stretches to match the border (or parent).
    private Image CreateBar(string goName, Color color, RectTransform reference)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();

        if (reference != null)
        {
            // Mirror border's anchors and offsets so bars stay locked to it
            rt.anchorMin  = reference.anchorMin;
            rt.anchorMax  = reference.anchorMax;
            rt.offsetMin  = reference.offsetMin;
            rt.offsetMax  = reference.offsetMax;
            rt.pivot      = reference.pivot;
        }
        else
        {
            // Full-stretch fallback
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        var img = go.GetComponent<Image>();
        img.color      = color;
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = 1f;
        img.sprite     = null;  // pure solid color — no sprite interference

        return img;
    }

    private void SetImmediate(float ratio)
    {
        if (frontImg != null) frontImg.fillAmount = ratio;
        if (backImg  != null) backImg.fillAmount  = ratio;
    }

    private IEnumerator DrainBack(float target)
    {
        yield return new WaitForSeconds(delayDuration);

        while (backImg != null && Mathf.Abs(backImg.fillAmount - target) > 0.002f)
        {
            backImg.fillAmount = Mathf.MoveTowards(backImg.fillAmount, target, drainSpeed * Time.deltaTime);
            yield return null;
        }

        if (backImg != null) backImg.fillAmount = target;
    }
}