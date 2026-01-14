using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // Agrega los listeners a los botones
    void Start()
    {
        btnShopExit.onClick.AddListener(OnBtnShopExit);
        btnUpgradeMaxPotion.onClick.AddListener(OnBtnUpgradeMaxPotion);
        btnUpgradeElementalFatigue.onClick.AddListener(OnBtnUpgradeElementalFatigue);
        elementSelectionUI.gameObject.SetActive(true);
        equipSelectorUI.gameObject.SetActive(true);
        elementSelectionUI.OpenSelection(playerEquip);
    }

    public void OnBtnUpgradeMaxPotion()
    {
        healthPotionSystem.MejorarCapacidad();
    }

    public void OnBtnUpgradeElementalFatigue()
    {

    }

    // Cierra la tienda y pasa al siguiente diálogo
    public void OnBtnShopExit()
    {
        Hide();
        // Pasa al siguiente diálogo
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        elementSelectionUI.gameObject.SetActive(false);
        equipSelectorUI.gameObject.SetActive(false);
    } 
}
