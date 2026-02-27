using UnityEngine;
using UnityEngine.UI;
using Pizzard.Core;

namespace Pizzard.UI
{
    /// <summary>
    /// Two-layer vertical mana bar: dark background + colored foreground that
    /// scales its height to reflect current mana.  Anchored bottom so the bar
    /// drains downward and refills upward.
    /// </summary>
    public class ManaUI : MonoBehaviour
    {
        [Header("Auto-created at runtime — no inspector wiring needed")]
        public RectTransform foreground;

        private static readonly Color BarColor = new Color(0.2f, 1f, 0.85f, 1f);
        private static readonly Color BgColor  = new Color(0.1f, 0.1f, 0.15f, 0.8f);

        private float fullHeight;

        private void Start()
        {
            BuildBar();
        }

        private void BuildBar()
        {
            var rt = GetComponent<RectTransform>();
            fullHeight = rt.sizeDelta.y;

            // --- Background (static, dark) ---
            var bgGO = new GameObject("ManaBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGO.transform.SetParent(transform, false);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgGO.GetComponent<Image>().color = BgColor;

            // --- Foreground (scales with mana) ---
            var fgGO = new GameObject("ManaFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fgGO.transform.SetParent(transform, false);
            foreground = fgGO.GetComponent<RectTransform>();
            // Anchor to bottom-stretch so we only change height
            foreground.anchorMin = new Vector2(0f, 0f);
            foreground.anchorMax = new Vector2(1f, 0f);
            foreground.pivot = new Vector2(0.5f, 0f);
            foreground.offsetMin = Vector2.zero;
            foreground.offsetMax = Vector2.zero;
            foreground.sizeDelta = new Vector2(0f, fullHeight);
            fgGO.GetComponent<Image>().color = BarColor;

            // Hide the parent Image so only the two children render
            var parentImg = GetComponent<Image>();
            if (parentImg != null)
                parentImg.color = Color.clear;
        }

        private void Update()
        {
            if (foreground == null || ManaSystem.Instance == null) return;

            float max = ManaSystem.Instance.MaxMana;
            if (max <= 0f) return;

            float ratio = Mathf.Clamp01(ManaSystem.Instance.CurrentMana / max);
            foreground.sizeDelta = new Vector2(foreground.sizeDelta.x, fullHeight * ratio);
        }
    }
}
