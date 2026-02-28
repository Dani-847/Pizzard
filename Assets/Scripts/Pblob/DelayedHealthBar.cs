using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar — ManaUI-style runtime bar creation.
/// Destroys old BackHealthBar / FrontHealthBar scene children.
/// Creates width-scaled fill bars anchored left so they drain right-to-left.
/// PblobBorderHealthBar (the frame) stays untouched.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    private static readonly Color RedColor    = new Color(0.85f, 0.10f, 0.10f, 1f);
    private static readonly Color OrangeColor = new Color(0.95f, 0.55f, 0.10f, 1f);

    private RectTransform frontRT;   // Red  — instant HP tracking (width scales)
    private RectTransform backRT;    // Orange — delayed drain animation (width scales)
    private float fullWidth;

    private Coroutine drainRoutine;

    private void Start()
    {
        BuildBars();
    }

    // ──────────────────────────────────── Public API

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        if (frontRT == null) return;

        // Instantly resize the red bar
        frontRT.sizeDelta = new Vector2(fullWidth * ratio, frontRT.sizeDelta.y);

        // Start the orange drain animation
        if (drainRoutine != null) StopCoroutine(drainRoutine);
        if (gameObject.activeInHierarchy)
            drainRoutine = StartCoroutine(DrainBack(ratio));
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ──────────────────────────────────── Build

    private void BuildBars()
    {
        // Step 1: remove old scene-placed bars
        DestroyChildByName("FrontHealthBar");
        DestroyChildByName("BackHealthBar");
        DestroyChildByName("HealthFrontBar");
        DestroyChildByName("HealthBackBar");

        // Step 2: measure parent for fullWidth
        var parentRT = GetComponent<RectTransform>();
        fullWidth = parentRT.rect.width;
        if (fullWidth <= 0f) fullWidth = parentRT.sizeDelta.x;
        if (fullWidth <= 0f) fullWidth = 480f; // fallback

        float barHeight = parentRT.rect.height;
        if (barHeight <= 0f) barHeight = parentRT.sizeDelta.y;
        if (barHeight <= 0f) barHeight = 80f;

        // Step 3: create orange (back) bar first — renders behind red
        backRT  = CreateFillBar("HealthBackBar",  OrangeColor, barHeight);
        frontRT = CreateFillBar("HealthFrontBar", RedColor,    barHeight);

        // Step 4: push border to top of sibling order
        Transform border = transform.Find("PblobBorderHealthBar");
        if (border != null) border.SetAsLastSibling();

        Debug.Log($"[DelayedHealthBar] Built — fullWidth={fullWidth:F0} height={barHeight:F0}");
    }

    private RectTransform CreateFillBar(string goName, Color color, float height)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        // Anchor left-stretch vertically so width controls fill
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(fullWidth, 0f); // full width, height from anchors

        var img = go.GetComponent<Image>();
        img.color = color;

        return rt;
    }

    private void DestroyChildByName(string childName)
    {
        Transform t = transform.Find(childName);
        if (t != null) Destroy(t.gameObject);
    }

    // ──────────────────────────────────── Drain

    private IEnumerator DrainBack(float targetRatio)
    {
        yield return new WaitForSeconds(Pizzard.Core.GameBalance.Bosses.Pblob.HealthBarDrainDelay);

        float targetWidth = fullWidth * targetRatio;
        float speed = Pizzard.Core.GameBalance.Bosses.Pblob.HealthBarDrainSpeed * fullWidth;

        while (backRT != null && Mathf.Abs(backRT.sizeDelta.x - targetWidth) > 1f)
        {
            float newW = Mathf.MoveTowards(backRT.sizeDelta.x, targetWidth, speed * Time.deltaTime);
            backRT.sizeDelta = new Vector2(newW, backRT.sizeDelta.y);
            yield return null;
        }

        if (backRT != null)
            backRT.sizeDelta = new Vector2(targetWidth, backRT.sizeDelta.y);
    }
}