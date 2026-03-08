// Assets/Scripts/Playground/PlaygroundHUDController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires the Playground HUD: shop button (loads Shop scene with isolated token economy),
/// back-to-menu button, and a live token counter.
/// </summary>
public class PlaygroundHUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tokenCounterText;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button backToMenuButton;

    // Persistent UI objects we hide while in the Playground
    private MenuUI _persistentMenuUI;

    private IEnumerator Start()
    {
        // Hide persistent main-menu UI that carries over via DontDestroyOnLoad
        _persistentMenuUI = FindObjectOfType<MenuUI>(true);
        if (_persistentMenuUI != null)
            _persistentMenuUI.gameObject.SetActive(false);

        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMenu);

        RefreshTokenDisplay();

        // Wait one frame so UIManager.Start() finishes hiding all panels (it resets to MainMenu state)
        yield return null;

        // Re-enable player HUD panels that normally only show during Combat state
        if (UIManager.Instance != null)
        {
            Transform uiRoot = UIManager.Instance.transform;
            foreach (string panelName in new[] { "Elementos", "HealthUI", "PotionUI", "ManaUI" })
            {
                Transform panel = uiRoot.Find(panelName);
                if (panel != null)
                    panel.gameObject.SetActive(true);
            }
            // Make sure boss-specific panels stay hidden
            foreach (string bossPanel in new[] { "PblobUI", "NiggelBossUI", "CoinMeterUI" })
            {
                Transform panel = uiRoot.Find(bossPanel);
                if (panel != null)
                    panel.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        RefreshTokenDisplay();
    }

    /// <summary>
    /// Saves current token count into static cache and loads the Shop scene.
    /// ShopPhaseManager detects IsPlaygroundSession and routes the exit back here.
    /// </summary>
    public void OpenShop()
    {
        if (PlaygroundManager.Instance == null)
        {
            Debug.LogWarning("[PlaygroundHUDController] PlaygroundManager.Instance is null — cannot open shop.");
            return;
        }

        PlaygroundShopBridge.Activate(PlaygroundManager.Instance.PlaygroundTokens);
        Pizzard.Core.SceneLoader.LoadScene("Shop");
    }

    /// <summary>
    /// Updates the HUD token counter to match the current PlaygroundManager balance.
    /// </summary>
    public void RefreshTokenDisplay()
    {
        if (PlaygroundManager.Instance != null && tokenCounterText != null)
            tokenCounterText.text = $"{PlaygroundManager.Instance.PlaygroundTokens} / 10";
    }

    private void BackToMenu()
    {
        // Restore persistent UI before leaving
        if (_persistentMenuUI != null)
            _persistentMenuUI.gameObject.SetActive(true);

        // Hide combat HUD panels so they don't bleed into the main menu
        if (UIManager.Instance != null)
        {
            Transform uiRoot = UIManager.Instance.transform;
            foreach (string panelName in new[] { "Elementos", "HealthUI", "PotionUI", "ManaUI" })
            {
                Transform panel = uiRoot.Find(panelName);
                if (panel != null)
                    panel.gameObject.SetActive(false);
            }
        }

        // Reset player systems modified during playground (they are DontDestroyOnLoad)
        var hp = FindObjectOfType<PlayerHPController>(true);
        if (hp != null)
            hp.RestaurarVidaCompleta();

        var potionSys = FindObjectOfType<HealthPotionSystem>(true);
        if (potionSys != null)
        {
            potionSys.maxPociones = Pizzard.Core.GameBalance.Potions.StartingMax;
            potionSys.RecargarPociones();
        }

        if (Pizzard.Core.ManaSystem.Instance != null)
            Pizzard.Core.ManaSystem.Instance.LoadMaxManaFromSave(Pizzard.Core.GameBalance.Mana.MaxMana);

        PlaygroundManager.ClearSession();
        Time.timeScale = 1f;
        Pizzard.Core.SceneLoader.LoadScene("MainMenu");
    }
}
