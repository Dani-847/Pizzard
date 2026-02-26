using UnityEngine;
using UnityEngine.UI;
using Pizzard.Core;

namespace Pizzard.UI
{
    /// <summary>
    /// Vertical blue bar representing remaining Elemental Fatigue.
    /// </summary>
    public class FatigueUI : MonoBehaviour
    {
        public Image fatigueFillBar; // Set this in inspector to blue vertically filled image

        private void Update()
        {
            if (FatigueSystem.Instance != null && fatigueFillBar != null)
            {
                float current = FatigueSystem.Instance.CurrentFatigue;
                float max = FatigueSystem.Instance.MaxFatigue;
                
                // Protect division by 0
                if (max > 0)
                {
                    fatigueFillBar.fillAmount = current / max;
                }
            }
        }
    }
}
