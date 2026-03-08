// Assets/Scripts/Playground/PlaygroundHUDController.cs
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

    private void Start()
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

        Time.timeScale = 1f;
        Pizzard.Core.SceneLoader.LoadScene("MainMenu");
    }
}
