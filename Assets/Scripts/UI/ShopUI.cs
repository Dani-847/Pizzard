using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pizzard.Core;

/// <summary>
/// Gestiona la interfaz de la tienda durante el flujo del juego.
/// Permite al jugador mejorar sus habilidades y equipamiento.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Botones")]
    public Button btnUpgradeMaxPotion;
    public Button btnUpgradeMana;
    public Button btnShopExit;
    public Button btnUpgradeWand;
    public TMPro.TextMeshProUGUI txtUpgradeWand;
    
    [Header("Referencias auxiliares")]
    public HealthPotionSystem healthPotionSystem;
    public ElementSelectionUI elementSelectionUI;
    public EquipSelectorUI equipSelectorUI;
    public PlayerEquip playerEquip;

    private GameFlowManager flowManager;
    private ITokenSource _tokenSource;

    /// <summary>
    /// Sets the token source for this shop. If not called, falls back to ProgressionManager.
    /// Call this from PlaygroundScene to wire the isolated playground token balance.
    /// </summary>
    public void SetTokenSource(ITokenSource source)
    {
        _tokenSource = source;
    }

    void Start()
    {
        if (btnShopExit != null)
            btnShopExit.onClick.AddListener(OnBtnShopExit);
        if (btnUpgradeMaxPotion != null)
            btnUpgradeMaxPotion.onClick.AddListener(OnBtnUpgradeMaxPotion);
        if (btnUpgradeMana != null)
            btnUpgradeMana.onClick.AddListener(OnBtnUpgradeMana);
        if (btnUpgradeWand != null)
            btnUpgradeWand.onClick.AddListener(OnBtnUpgradeWand);
    }

    /// <summary>
    /// Mejora la capacidad máxima de pociones. Consumes 1 Token.
    /// </summary>
    public void OnBtnUpgradeMaxPotion()
    {
        ITokenSource src = _tokenSource ?? Pizzard.Progression.ProgressionManager.Instance as ITokenSource;
        if (src != null && src.SpendTokens(1))
        {
            hasPurchasedInRun1 = true;
            RefreshTokens();
            if (healthPotionSystem != null)
                healthPotionSystem.MejorarCapacidad();
        }
        else
        {
            Debug.LogWarning("[ShopUI] No tokens to upgrade potion!");
        }
    }

    /// <summary>
    /// Upgrades max mana (×1.5). Costs 1 Token.
    /// </summary>
    public void OnBtnUpgradeMana()
    {
        ITokenSource src = _tokenSource ?? Pizzard.Progression.ProgressionManager.Instance as ITokenSource;
        if (src != null && src.SpendTokens(1))
        {
            hasPurchasedInRun1 = true;
            RefreshTokens();
            Debug.Log("[ShopUI] Improved Mana!");
            if (ManaSystem.Instance != null)
            {
                ManaSystem.Instance.UpgradeMaxMana(20);
            }
        }
        else
        {
            Debug.LogWarning("[ShopUI] No tokens to upgrade mana!");
        }
    }

    /// <summary>
    /// Mejora el nivel de la varita (Tier). Consumes 1 Token.
    /// </summary>
    public void OnBtnUpgradeWand()
    {
        ITokenSource src = _tokenSource ?? Pizzard.Progression.ProgressionManager.Instance as ITokenSource;
        if (src != null && src.SpendTokens(1))
        {
            hasPurchasedInRun1 = true;
            RefreshTokens();
            
            if (Pizzard.Progression.SaveManager.Instance != null)
            {
                int savedTier = Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier;
                if (savedTier < 3)
                {
                    Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier = savedTier + 1;
                }
            }

            if (playerEquip != null)
                playerEquip.UpgradeWandTier();
                
            UpdateWandButtonUI();
            
            // --- WAVE 2: IMMEDIATE REACTIVE REFRESH ---
            int newTier = Pizzard.Progression.SaveManager.Instance != null ? Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier : 1;
            if (elementSelectionUI != null)
            {
                elementSelectionUI.RefreshFromWandTier(newTier);
            }
            if (equipSelectorUI != null && equipSelectorUI.gameObject.activeInHierarchy)
            {
                equipSelectorUI.GenerateButtons();
            }
            
            // Refresh exit button (Shop 1 hard-lock may now be releasable)
            RefreshExitButton();
        }
        else
        {
            Debug.LogWarning("[ShopUI] No tokens to upgrade wand!");
        }
    }

    private void UpdateWandButtonUI()
    {
        if (btnUpgradeWand == null) return;
        
        int currentTier = playerEquip != null ? playerEquip.CurrentWandTier 
            : (Pizzard.Progression.SaveManager.Instance != null ? Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier : 1);
        
        ITokenSource wandSrc = _tokenSource ?? Pizzard.Progression.ProgressionManager.Instance as ITokenSource;
        int tokens = wandSrc != null ? wandSrc.GetTokens() : 0;

        if (currentTier >= 3)
        {
            btnUpgradeWand.interactable = false;
            if (txtUpgradeWand != null) txtUpgradeWand.text = LocalizationManager.Instance != null
       ? LocalizationManager.Instance.GetText("shop_wand_max_level")
       : "Max Wand Level";
        }
        else
        {
            btnUpgradeWand.interactable = (tokens > 0);
            string nextWandName = currentTier >= 2 ? "Tier 3 Wand" : "Tier " + (currentTier + 1) + " Wand";
            if (txtUpgradeWand != null)
            {
                int nextTier = currentTier + 1;
                string wandName = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("shop_wand_tier_" + nextTier)
                    : nextWandName;
                string buyFmt = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("shop_wand_buy")
                    : "Buy next wand:\n\"{0}\"";
                txtUpgradeWand.text = string.Format(buyFmt, wandName);
                txtUpgradeWand.color = (tokens > 0) ? Color.white : new Color(1f, 0.5f, 0.5f); // Reddish if can't buy
            }
        }
    }

    [Header("UI General")]
    public TMPro.TextMeshProUGUI txtTokenCount; // ✅ NUEVO: Mostrar tokens actuales

    private int exitClicks = 0;
    public bool hasPurchasedInRun1 = false;

    /// <summary>
    /// Cierra la tienda y pasa al siguiente diálogo.
    /// Run 1: Blocks until hasPurchasedInRun1 is true.
    /// Subsequent: Requires 2 clicks.
    /// </summary>
    public void OnBtnShopExit()
    {
        // --- WAVE 2: SHOP 1 HARD-LOCK (FRONTEND) ---
        if (Pizzard.Progression.SaveManager.Instance != null &&
            Pizzard.Progression.SaveManager.Instance.CurrentSave.bossIndex == 1 &&
            Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier == 0)
        {
            Debug.Log("[ShopUI] FRONTEND BLOCK: Cannot exit Shop 1 without a wand.");
            return;
        }

        exitClicks++;

        if (exitClicks == 1)
        {
            Debug.Log("[ShopUI] Warning: Si sales ahora, te enfrentarás al boss sin terminar de comprar.\nClick again to Exit.");
            var dialog = FindObjectOfType<DialogUI>();
            if (dialog != null)
            {
                dialog.ShowShopWarningDialog();
            }
        }
        else if (exitClicks >= 2)
        {
            exitClicks = 0;
            Hide();
            flowManager?.AvanzarFase();
        }
    }

    /// <summary>
    /// Muestra la tienda (sin integración con GameFlowManager).
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        InitializeShopUI();
    }

    /// <summary>
    /// Muestra la tienda integrada con el flujo del juego.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al cerrar.</param>
    public void Show(GameFlowManager manager)
    {
        flowManager = manager;
        gameObject.SetActive(true);
        InitializeShopUI();
    }

    /// <summary>
    /// Inicializa los elementos de la UI de la tienda.
    /// </summary>
    private void InitializeShopUI()
    {
        // Reset exit warning counter on every shop open
        exitClicks = 0;
        
        // Fallbacks in case Inspector references are lost
        if (elementSelectionUI == null) elementSelectionUI = FindObjectOfType<ElementSelectionUI>(true);
        if (equipSelectorUI == null) equipSelectorUI = FindObjectOfType<EquipSelectorUI>(true);
        if (playerEquip == null) playerEquip = FindObjectOfType<PlayerEquip>(true);
        
        // --- Auto-create token counter if not wired in Inspector ---
        if (txtTokenCount == null)
        {
            GameObject tokenGO = new GameObject("TokenCounter");
            tokenGO.transform.SetParent(transform, false);
            
            RectTransform rt = tokenGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-20, -20);
            rt.sizeDelta = new Vector2(200, 50);
            
            txtTokenCount = tokenGO.AddComponent<TMPro.TextMeshProUGUI>();
            txtTokenCount.fontSize = 28;
            txtTokenCount.alignment = TMPro.TextAlignmentOptions.Right;
            txtTokenCount.color = Color.white;
            string tokenFmt = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("shop_token_count") : "Tokens: {0}";
            txtTokenCount.text = string.Format(tokenFmt, 0);
            Debug.Log("[ShopUI] Auto-created TokenCounter text (top-right).");
        }

        int currentWandTier = Pizzard.Progression.SaveManager.Instance != null ? Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier : 0;

        // --- WAVE 2: ALWAYS show element selection UI, reactive to wandTier ---
        if (elementSelectionUI != null)
        {
            elementSelectionUI.gameObject.SetActive(true);
            if (playerEquip != null && playerEquip.equipedObject != null)
            {
                elementSelectionUI.OpenSelection(playerEquip);
            }
            else if (equipSelectorUI != null && equipSelectorUI.availableEquipables != null)
            {
                // Find the wand matching the current tier, or the first wand as fallback
                EquipableObject targetWand = equipSelectorUI.availableEquipables.Find(e => e.tier == currentWandTier);
                if (targetWand == null && equipSelectorUI.availableEquipables.Count > 0)
                    targetWand = equipSelectorUI.availableEquipables[0];
                
                if (targetWand != null)
                {
                    elementSelectionUI.OpenSelectionWithoutPlayer(targetWand, currentWandTier);
                }
            }
        }

        if (equipSelectorUI != null)
            equipSelectorUI.gameObject.SetActive(true);
            
        UpdateWandButtonUI();
        RefreshTokens();
        RefreshExitButton();
    }

    public void RefreshTokens()
    {
        ITokenSource src = _tokenSource ?? Pizzard.Progression.ProgressionManager.Instance as ITokenSource;
        if (txtTokenCount != null && src != null)
        {
            int tokens = src.GetTokens();
            string tokenFmt = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("shop_token_count") : "Tokens: {0}";
            txtTokenCount.text = string.Format(tokenFmt, tokens);
            txtTokenCount.color = tokens > 0 ? Color.white : new Color(1f, 0.4f, 0.4f); // Red if zero tokens

            // Visual feedback on the buttons based on token availability
            if (btnUpgradeMaxPotion != null) btnUpgradeMaxPotion.interactable = (tokens > 0);
            if (btnUpgradeMana != null) btnUpgradeMana.interactable = (tokens > 0);

            UpdateWandButtonUI();
            RefreshExitButton();
        }
    }
    
    public void RefreshExitButton()
    {
        if (btnShopExit != null && Pizzard.Progression.SaveManager.Instance != null)
        {
            bool isShop1Locked = Pizzard.Progression.SaveManager.Instance.CurrentSave.bossIndex == 1 && 
                                 Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier == 0;
            
            btnShopExit.interactable = !isShop1Locked;
        }
    }
    
    /// <summary>
    /// Oculta la tienda y sus componentes.
    /// </summary>
    /// <param name="suppressSave">Pass true from Playground context to skip SaveManager.SaveGame().</param>
    public void Hide(bool suppressSave = false)
    {
        // --- WAVE 3: AUTO-SAVE on shop close ---
        if (!suppressSave && Pizzard.Progression.SaveManager.Instance != null)
        {
            Pizzard.Progression.SaveManager.Instance.SaveGame();
            Debug.Log("[ShopUI] Auto-saved on shop close.");
        }

        gameObject.SetActive(false);
        if (elementSelectionUI != null)
            elementSelectionUI.gameObject.SetActive(false);
        if (equipSelectorUI != null)
            equipSelectorUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshLocalized;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshLocalized;
    }

    private void RefreshLocalized()
    {
        RefreshTokens();
        UpdateWandButtonUI();
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        ITokenSource dbgSrc = _tokenSource ?? Pizzard.Progression.ProgressionManager.Instance as ITokenSource;
        int tokens = dbgSrc != null ? dbgSrc.GetTokens() : 0;
        GUILayout.BeginArea(new Rect(Screen.width - 170f, 10f, 160f, 80f));
        GUI.Box(new Rect(0, 0, 160f, 80f), "");
        GUILayout.Label($"Tokens: {tokens}");
        if (GUILayout.Button("+1 Token"))
            Pizzard.Progression.ProgressionManager.Instance?.AddCurrency(1);
        if (GUILayout.Button("+50 Tokens"))
            Pizzard.Progression.ProgressionManager.Instance?.AddCurrency(50);
        GUILayout.EndArea();
    }
#endif
}
