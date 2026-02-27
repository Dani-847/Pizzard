using UnityEngine;
using UnityEngine.UI;
using Pizzard.Core;

namespace Pizzard.UI
{
    /// <summary>
    /// Vertical bar representing remaining Mana (renamed from Elemental Fatigue).
    /// Uses a bright cyan color that contrasts with the dark blue game background.
    /// </summary>
    public class ManaUI : MonoBehaviour
    {
        public Image manaFillBar; // Set this in inspector to vertically filled image
        
        // Bright cyan color — stands out against dark blue background
        private static readonly Color ManaColor = new Color(0.2f, 1f, 0.85f, 1f); // bright teal/cyan

        private void Awake()
        {
            if (manaFillBar == null)
                manaFillBar = GetComponent<Image>();
        }
        
        private void Start()
        {
            // Force a visible color on the fill bar
            if (manaFillBar != null)
            {
                manaFillBar.color = ManaColor;
            }
        }

        private void Update()
        {
            if (ManaSystem.Instance != null && manaFillBar != null)
            {
                float current = ManaSystem.Instance.CurrentMana;
                float max = ManaSystem.Instance.MaxMana;
                
                // Protect division by 0
                if (max > 0)
                {
                    manaFillBar.fillAmount = current / max;
                }
            }
        }
    }
}
