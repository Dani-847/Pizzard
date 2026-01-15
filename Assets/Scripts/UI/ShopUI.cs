using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    }

    /// <summary>
    /// Mejora la capacidad máxima de pociones.
    /// </summary>
    public void OnBtnUpgradeMaxPotion()
    {
        if (healthPotionSystem != null)
            healthPotionSystem.MejorarCapacidad();
    }

    /// <summary>
    /// Mejora la fatiga elemental (pendiente de implementar).
    /// </summary>
    public void OnBtnUpgradeElementalFatigue()
    {
        // TODO: Implementar mejora de fatiga elemental
    }

    /// <summary>
    /// Cierra la tienda y pasa al siguiente diálogo.
    /// </summary>
    public void OnBtnShopExit()
    {
        Hide();
        flowManager?.AvanzarFase();
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
