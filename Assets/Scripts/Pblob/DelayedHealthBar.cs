using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar — ManaUI-style runtime fill bar.
/// Reads PblobBorderHealthBar's RectTransform to know size/position.
/// Creates a single foreground bar (red) that scales its WIDTH to track HP.
/// The border sprite is the background — it stays untouched.
/// For now: just the red fill bar. Orange delayed drain can be added later.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    private static readonly Color BarColor = new Color(0.85f, 0.10f, 0.10f, 1f);

    private RectTransform foreground;
    private float fullWidth;

    private void Start()
    {
        BuildBar();
    }

    // ──────────────────────────────────── Public API

    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        if (foreground == null) return;
        foreground.sizeDelta = new Vector2(fullWidth * ratio, foreground.sizeDelta.y);
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ──────────────────────────────────── Build

    private void BuildBar()
    {
        // Find the border child — it defines the size and position
        RectTransform border = null;
        foreach (Transform child in transform)
        {
            if (child.name == "PblobBorderHealthBar")
            {
                border = child as RectTransform;
                break;
            }
        }

        if (border == null)
        {
            Debug.LogError("[DelayedHealthBar] PblobBorderHealthBar not found as child!");
            return;
        }

        // Read the border's dimensions
        fullWidth = border.sizeDelta.x;  // 566.23
        float barHeight = border.sizeDelta.y;  // 100

        // Create the foreground fill bar
        var fgGO = new GameObject("HealthFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fgGO.transform.SetParent(transform, false);

        foreground = fgGO.GetComponent<RectTransform>();

        // Same anchors/pivot as border so it sits in the exact same spot
        foreground.anchorMin = border.anchorMin;
        foreground.anchorMax = border.anchorMax;
        foreground.pivot = new Vector2(0f, 0.5f); // Left-anchored pivot for width scaling

        // Position: border center offset, adjusted for left-pivot
        // Border is center-pivoted at (-15, 139), so left edge = -15 - fullWidth/2
        foreground.anchoredPosition = new Vector2(
            border.anchoredPosition.x - fullWidth / 2f,
            border.anchoredPosition.y
        );

        foreground.sizeDelta = new Vector2(fullWidth, barHeight);

        var img = fgGO.GetComponent<Image>();
        img.color = BarColor;

        // Ensure fill bar renders BEHIND the border frame
        foreground.SetAsFirstSibling();

        Debug.Log($"[DelayedHealthBar] Built — width={fullWidth:F0} height={barHeight:F0} pos={foreground.anchoredPosition}");
    }
}