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
    public Button btnUpgradeElementalFatigue;
    public Button btnShopExit;
    public Button btnUpgradeWand;
    public TMPro.TextMeshProUGUI txtUpgradeWand;
    
    [Header("Referencias auxiliares")]
    public HealthPotionSystem healthPotionSystem;
    public ElementSelectionUI elementSelectionUI;
    public EquipSelectorUI equipSelectorUI;
    public PlayerEquip playerEquip;

    private GameFlowManager flowManager;

    void Start()
    {
        if (btnShopExit != null)
            btnShopExit.onClick.AddListener(OnBtnShopExit);
        if (btnUpgradeMaxPotion != null)
            btnUpgradeMaxPotion.onClick.AddListener(OnBtnUpgradeMaxPotion);
        if (btnUpgradeElementalFatigue != null)
            btnUpgradeElementalFatigue.onClick.AddListener(OnBtnUpgradeElementalFatigue);
        if (btnUpgradeWand != null)
            btnUpgradeWand.onClick.AddListener(OnBtnUpgradeWand);
    }

    /// <summary>
    /// Mejora la capacidad máxima de pociones. Consumes 1 Token.
    /// </summary>
    public void OnBtnUpgradeMaxPotion()
    {
        if (Pizzard.Progression.ProgressionManager.Instance != null && Pizzard.Progression.ProgressionManager.Instance.SpendCurrency(1))
        {
            hasPurchasedInRun1 = true;
            if (healthPotionSystem != null)
                healthPotionSystem.MejorarCapacidad();
        }
        else
        {
            Debug.LogWarning("[ShopUI] No tokens to upgrade potion!");
        }
    }

    /// <summary>
    /// Mejora la fatiga elemental. Consumes 1 Token.
    /// </summary>
    public void OnBtnUpgradeElementalFatigue()
    {
        if (Pizzard.Progression.ProgressionManager.Instance != null && Pizzard.Progression.ProgressionManager.Instance.SpendCurrency(1))
        {
            hasPurchasedInRun1 = true;
            Debug.Log("[ShopUI] Improved Elemental Fatigue!");
            if (FatigueSystem.Instance != null)
            {
                FatigueSystem.Instance.UpgradeMaxFatigue(20);
            }
        }
        else
        {
            Debug.LogWarning("[ShopUI] No tokens to upgrade fatigue!");
        }
    }

    /// <summary>
    /// Mejora el nivel de la varita (Tier). Consumes 1 Token.
    /// </summary>
    public void OnBtnUpgradeWand()
    {
        if (Pizzard.Progression.ProgressionManager.Instance != null && Pizzard.Progression.ProgressionManager.Instance.SpendCurrency(1))
        {
            hasPurchasedInRun1 = true;
            
            if (Pizzard.Progression.SaveManager.Instance != null)
            {
                int savedTier = Pizzard.Progression.SaveManager.Instance.CurrentSave.currentWandTier;
                if (savedTier < 3)
                {
                    Pizzard.Progression.SaveManager.Instance.CurrentSave.currentWandTier = savedTier + 1;
                }
            }

            if (playerEquip != null)
                playerEquip.UpgradeWandTier();
                
            UpdateWandButtonUI();
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
            : (Pizzard.Progression.SaveManager.Instance != null ? Pizzard.Progression.SaveManager.Instance.CurrentSave.currentWandTier : 1);
        
        if (currentTier >= 3)
        {
            btnUpgradeWand.interactable = false;
            if (txtUpgradeWand != null) txtUpgradeWand.text = "Max Wand Level";
        }
        else
        {
            btnUpgradeWand.interactable = true;
            string nextWandName = currentTier == 1 ? "Tier 2 Wand" : "Tier 3 Wand";
            if (txtUpgradeWand != null) txtUpgradeWand.text = $"Buy next wand:\n\"{nextWandName}\"";
        }
    }

    private int exitClicks = 0;
    public bool hasPurchasedInRun1 = false;

    /// <summary>
    /// Cierra la tienda y pasa al siguiente diálogo.
    /// Run 1: Blocks until hasPurchasedInRun1 is true.
    /// Subsequent: Requires 2 clicks.
    /// </summary>
    public void OnBtnShopExit()
    {
        if (Pizzard.Progression.ProgressionManager.Instance != null && Pizzard.Progression.SaveManager.Instance != null)
        {
            if (Pizzard.Progression.SaveManager.Instance.CurrentSave.currentBossIndex == 1 && !hasPurchasedInRun1)
            {
                Debug.LogWarning("[ShopUI] Cannot exit! You must buy at least 1 upgrade (Wand) on your first run.");
                return;
            }
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
        if (elementSelectionUI != null)
        {
            elementSelectionUI.gameObject.SetActive(true);
            if (playerEquip != null)
                elementSelectionUI.OpenSelection(playerEquip);
        }
        if (equipSelectorUI != null)
            equipSelectorUI.gameObject.SetActive(true);
            
        UpdateWandButtonUI();
    }
    
    /// <summary>
    /// Oculta la tienda y sus componentes.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        if (elementSelectionUI != null)
            elementSelectionUI.gameObject.SetActive(false);
        if (equipSelectorUI != null)
            equipSelectorUI.gameObject.SetActive(false);
    } 
}
