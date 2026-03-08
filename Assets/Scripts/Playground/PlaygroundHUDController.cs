// Assets/Scripts/Playground/PlaygroundHUDController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pizzard.UI;

/// <summary>
/// Wires the Playground HUD: shop button (opens ShopUI with PlaygroundManager token source),
/// back-to-menu button, and a live token counter.
/// </summary>
public class PlaygroundHUDController : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private TextMeshProUGUI tokenCounterText;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button backToMenuButton;

    // Persistent UI objects we hide while in the Playground
    private MenuUI _persistentMenuUI;
    private GameObject _persistentCoinMeter;

    private void Start()
    {
        // Find ShopUI from the scene (it lives in DontDestroyOnLoad) if not wired in Inspector
        if (shopUI == null)
            shopUI = FindObjectOfType<ShopUI>(true);

        // Hide persistent main-menu UI that carries over via DontDestroyOnLoad
        _persistentMenuUI = FindObjectOfType<MenuUI>(true);
        if (_persistentMenuUI != null)
            _persistentMenuUI.gameObject.SetActive(false);

        // Hide the persistent coin/token meter from the main game
        var coinMeter = FindObjectOfType<NiggelCoinMeterUI>(true);
        if (coinMeter != null)
        {
            _persistentCoinMeter = coinMeter.gameObject;
            _persistentCoinMeter.SetActive(false);
        }

        // Token counter only visible inside the shop
        if (tokenCounterText != null)
            tokenCounterText.gameObject.SetActive(false);

        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMenu);
    }

    /// <summary>
    /// Opens the shop using PlaygroundManager as the isolated token source.
    /// Pauses time (Time.timeScale = 0) while the shop is open.
    /// </summary>
    public void OpenShop()
    {
        if (PlaygroundManager.Instance == null)
        {
            Debug.LogWarning("[PlaygroundHUDController] PlaygroundManager.Instance is null — cannot open shop.");
            return;
        }

        if (shopUI == null)
        {
            Debug.LogWarning("[PlaygroundHUDController] shopUI is null — cannot open shop.");
            return;
        }

        shopUI.SetTokenSource(PlaygroundManager.Instance);
        Time.timeScale = 0f;
        shopUI.Show();

        // Hook the shop's own exit button so CloseShop() is called when it closes
        if (shopUI.btnShopExit != null)
        {
            shopUI.btnShopExit.onClick.RemoveListener(CloseShop);
            shopUI.btnShopExit.onClick.AddListener(CloseShop);
        }

        if (tokenCounterText != null)
        {
            tokenCounterText.gameObject.SetActive(true);
            RefreshTokenDisplay();
        }
    }

    /// <summary>
    /// Closes the shop without triggering the main-game save.
    /// Resumes time (Time.timeScale = 1).
    /// </summary>
    public void CloseShop()
    {
        if (shopUI != null)
            shopUI.Hide(suppressSave: true);
        Time.timeScale = 1f;

        if (tokenCounterText != null)
            tokenCounterText.gameObject.SetActive(false);
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
        if (_persistentCoinMeter != null)
            _persistentCoinMeter.SetActive(true);

        // Ensure time is not paused when leaving
        Time.timeScale = 1f;
        Pizzard.Core.SceneLoader.LoadScene("MainMenu");
    }
}
