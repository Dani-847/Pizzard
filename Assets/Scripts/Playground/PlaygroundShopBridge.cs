// Assets/Scripts/Playground/PlaygroundShopBridge.cs
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Created by PlaygroundHUDController before loading Shop scene.
/// Survives via DontDestroyOnLoad, waits for Shop scene to load,
/// then shows ShopUI wired to the isolated playground token balance.
/// Destroys itself once the player returns to PlaygroundScene.
/// </summary>
public class PlaygroundShopBridge : MonoBehaviour
{
    private static PlaygroundShopBridge _instance;
    private static readonly string[] _combatPanels = { "Elementos", "HealthUI", "PotionUI", "ManaUI" };

    public static void Activate(int currentTokens)
    {
        if (_instance != null)
            Destroy(_instance.gameObject);

        var go = new GameObject("PlaygroundShopBridge");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<PlaygroundShopBridge>();
        PlaygroundManager.BeginShopSession(currentTokens);

        // Hide combat HUD panels while in shop
        SetCombatPanelsActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Shop")
        {
            // GameFlowManager hides all UI when any scene loads — wait one frame for it to settle
            StartCoroutine(ShowShopNextFrame());
        }
        else if (scene.name == "PlaygroundScene")
        {
            // Hide the shop UI — UIManager.Start() won't re-run since it's persistent
            var shopUI = FindObjectOfType<ShopUI>(true);
            if (shopUI != null && shopUI.gameObject.activeSelf)
                shopUI.Hide(suppressSave: true);

            // Restore combat HUD panels for playground gameplay
            SetCombatPanelsActive(true);

            Destroy(gameObject);
            _instance = null;
        }
    }

    private System.Collections.IEnumerator ShowShopNextFrame()
    {
        yield return null; // let GameFlowManager.Start() / any other Awake run first

        var shopUI = FindObjectOfType<ShopUI>(true);
        if (shopUI == null)
        {
            Debug.LogWarning("[PlaygroundShopBridge] ShopUI not found in Shop scene.");
            yield break;
        }

        shopUI.SetTokenSource(new PlaygroundTokenProxy());
        shopUI.gameObject.SetActive(true);
        shopUI.Show();

        // Hook the exit button to return to PlaygroundScene instead of advancing the game
        if (shopUI.btnShopExit != null)
        {
            shopUI.btnShopExit.onClick.RemoveListener(OnShopExit);
            shopUI.btnShopExit.onClick.AddListener(OnShopExit);
        }

        Debug.Log("[PlaygroundShopBridge] Shop wired to playground tokens.");
    }

    private void OnShopExit()
    {
        var shopUI = FindObjectOfType<ShopUI>(true);
        if (shopUI?.btnShopExit != null)
            shopUI.btnShopExit.onClick.RemoveListener(OnShopExit);

        // Capture chosen elements before the shop scene (and its EquipableObjects) is destroyed
        var playerEquip = FindObjectOfType<PlayerEquip>(true);
        if (playerEquip != null && playerEquip.equipedObject != null)
        {
            PlaygroundManager.PlaygroundWandElements = new System.Collections.Generic.List<ElementType>(playerEquip.equipedObject.elements);
        }

        PlaygroundManager.EndShopSession(PlaygroundManager.GetCachedTokens());
        Pizzard.Core.SceneLoader.LoadScene("PlaygroundScene");
    }

    /// <summary>
    /// Shows or hides combat HUD panels (potions, mana, elements, health) on UIManager.
    /// </summary>
    private static void SetCombatPanelsActive(bool active)
    {
        if (UIManager.Instance == null) return;
        Transform uiRoot = UIManager.Instance.transform;
        foreach (string panelName in _combatPanels)
        {
            Transform panel = uiRoot.Find(panelName);
            if (panel != null)
                panel.gameObject.SetActive(active);
        }
    }
}

/// <summary>
/// Thin ITokenSource proxy reading/writing the static PlaygroundManager token cache.
/// Used when Shop scene is loaded during a playground session.
/// </summary>
public class PlaygroundTokenProxy : ITokenSource
{
    public int GetTokens() => PlaygroundManager.GetCachedTokens();
    public bool SpendTokens(int amount) => PlaygroundManager.SpendCachedTokens(amount);
}
