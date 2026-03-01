using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pizzard.Bosses;

namespace Pizzard.UI
{
    /// <summary>
    /// Player-side HUD element displayed below the mana bar in BossArena_2.
    /// Shows how many coins the player has stolen from Niggel (CoinVaultMax - CurrentCoinVault).
    /// The fill bar runs left-to-right from empty (0 stolen) to full (all stolen).
    /// </summary>
    public class NiggelCoinMeterUI : MonoBehaviour
    {
        [SerializeField] private Image fillBar;
        [SerializeField] private TextMeshProUGUI coinCountText;

        private NiggelController boss;

        private void OnEnable()
        {
            boss = FindObjectOfType<NiggelController>();
        }

        private void Update()
        {
            if (boss == null)
            {
                boss = FindObjectOfType<NiggelController>();
                return;
            }

            int stolen = boss.CoinVaultMax - boss.CurrentCoinVault;
            float ratio = boss.CoinVaultMax > 0 ? (float)stolen / boss.CoinVaultMax : 0f;

            if (fillBar != null) fillBar.fillAmount = ratio;
            if (coinCountText != null) coinCountText.text = stolen.ToString();
        }
    }
}
