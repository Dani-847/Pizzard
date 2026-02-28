using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar: solid-color Image.Filled bars built at runtime.
/// - Destroys ALL children except PblobBorderHealthBar at Start()
/// - Creates HealthBackBar (orange) and HealthFrontBar (red) from scratch
/// - SetHealth() instantly moves red bar; orange drains slowly after delay
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    // Runtime-built bars
    private Image frontImg;  // Red  — instant
    private Image backImg;   // Orange — delayed drain

    private static readonly Color RedColor    = new Color(0.85f, 0.10f, 0.10f, 1f);
    private static readonly Color OrangeColor = new Color(0.95f, 0.55f, 0.10f, 1f);

    private Coroutine drainRoutine;
    private bool barsBuilt = false;

    private void Start()
    {
        BuildBars();
    }

    // ──────────────────────────────────── Public API

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        if (!barsBuilt) BuildBars();

        frontImg.fillAmount = ratio;

        if (drainRoutine != null) StopCoroutine(drainRoutine);
        if (gameObject.activeInHierarchy)
            drainRoutine = StartCoroutine(DrainBack(ratio));
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ──────────────────────────────────── Build logic

    private void BuildBars()
    {
        if (barsBuilt) return;
        barsBuilt = true;

        // Step 1: find border BEFORE clearing — we need its rect as reference
        RectTransform border = FindBorder();

        // Step 2: destroy every child that is NOT the border
        ClearAllExceptBorder();

        // Step 3: freshly re-find border after destroy (it's still there)
        border = FindBorder();

        // Step 4: create bars (back first → renders below front)
        backImg  = CreateBar("HealthBackBar",  OrangeColor, border);
        frontImg = CreateBar("HealthFrontBar", RedColor,    border);

        // Step 5: border always on top
        if (border != null) border.SetAsLastSibling();
    }

    private RectTransform FindBorder()
    {
        foreach (Transform c in transform)
            if (c.name == "PblobBorderHealthBar") return c as RectTransform;
        return null;
    }

    private void ClearAllExceptBorder()
    {
        // Collect to avoid modification-during-iteration
        var toDestroy = new System.Collections.Generic.List<Transform>();
        foreach (Transform c in transform)
            if (c.name != "PblobBorderHealthBar") toDestroy.Add(c);
        foreach (var c in toDestroy) Destroy(c.gameObject);
    }

    private Image CreateBar(string goName, Color color, RectTransform reference)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        if (reference != null)
        {
            rt.anchorMin = reference.anchorMin;
            rt.anchorMax = reference.anchorMax;
            rt.offsetMin = reference.offsetMin;
            rt.offsetMax = reference.offsetMax;
            rt.pivot     = reference.pivot;
        }
        else
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        var img = go.GetComponent<Image>();
        img.color      = color;
        img.sprite     = null;
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = 1f;
        return img;
    }

    // ──────────────────────────────────── Drain coroutine

    private IEnumerator DrainBack(float target)
    {
        yield return new WaitForSeconds(Pizzard.Core.GameBalance.Bosses.Pblob.HealthBarDrainDelay);

        float speed = Pizzard.Core.GameBalance.Bosses.Pblob.HealthBarDrainSpeed;
        while (backImg != null && Mathf.Abs(backImg.fillAmount - target) > 0.001f)
        {
            backImg.fillAmount = Mathf.MoveTowards(backImg.fillAmount, target, speed * Time.deltaTime);
            yield return null;
        }

        if (backImg != null) backImg.fillAmount = target;
    }
}