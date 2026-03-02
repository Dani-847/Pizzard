using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Horizontal boss health bar — exact ManaUI pattern rotated 90°.
/// Place this on a RectTransform with a fixed sizeDelta (not stretch-anchored).
/// Position/size the rect manually in the Inspector to sit inside the border image.
/// Foreground anchored LEFT, scales WIDTH as HP drains.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    [Header("Auto-created at runtime — no inspector wiring needed")]
    public RectTransform foreground;

    private static readonly Color BarColor = new Color(0.85f, 0.10f, 0.10f, 1f);
    private static readonly Color BgColor  = new Color(0.1f, 0.1f, 0.15f, 0.8f);

    private float fullWidth;

    private void Start()
    {
        BuildBar();
    }

    private void BuildBar()
    {
        var rt = GetComponent<RectTransform>();

        // --- Background (static, dark) ---
        var bgGO = new GameObject("HealthBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGO.transform.SetParent(transform, false);
        bgGO.transform.SetAsFirstSibling(); // Render behind siblings like the border
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgGO.GetComponent<Image>().color = BgColor;

        // --- Foreground (scales with HP) ---
        var fgGO = new GameObject("HealthFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fgGO.transform.SetParent(transform, false);
        // Set index to 1 so it renders above the background (index 0) but still behind the border
        fgGO.transform.SetSiblingIndex(1); 
        foreground = fgGO.GetComponent<RectTransform>();
        // Anchor to left-stretch so we only change width (mirrors ManaUI bottom-stretch)
        foreground.anchorMin = new Vector2(0f, 0f);
        foreground.anchorMax = new Vector2(1f, 1f); // Start at 100% full stretch
        foreground.pivot     = new Vector2(0f, 0.5f);
        foreground.offsetMin = Vector2.zero;
        foreground.offsetMax = Vector2.zero;
        // removed sizeDelta override so it freely stretches to its anchors
        fgGO.GetComponent<Image>().color = BarColor;

        // Hide the parent Image so only the two children render
        var parentImg = GetComponent<Image>();
        if (parentImg != null)
            parentImg.color = Color.clear;
    }

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        if (foreground == null) return;
        // Dynamically reduce the right-anchor to shrink the bar
        foreground.anchorMax = new Vector2(ratio, 1f);
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }
}
