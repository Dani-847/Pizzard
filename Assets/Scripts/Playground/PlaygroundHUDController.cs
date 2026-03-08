// Assets/Scripts/Playground/PlaygroundHUDController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Start()
    {
        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMenu);
        RefreshTokenDisplay();
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

        shopUI.SetTokenSource(PlaygroundManager.Instance);
        Time.timeScale = 0f;
        shopUI.Show();
        RefreshTokenDisplay();
    }

    /// <summary>
    /// Closes the shop without triggering the main-game save.
    /// Resumes time (Time.timeScale = 1).
    /// </summary>
    public void CloseShop()
    {
        shopUI.Hide(suppressSave: true);
        Time.timeScale = 1f;
        RefreshTokenDisplay();
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
        // Ensure time is not paused when leaving (e.g., if shop was open)
        Time.timeScale = 1f;
        Pizzard.Core.SceneLoader.LoadScene("MainMenu");
    }
}
