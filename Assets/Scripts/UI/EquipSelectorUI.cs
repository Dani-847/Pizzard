using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipSelectorUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    private PlayerEquip playerEquip;

    public List<EquipableObject> availableEquipables;

    void OnEnable()
    {
        playerEquip = FindObjectOfType<PlayerEquip>(true);
        GenerateButtons();
    }

    public void GenerateButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        int currentTier = Pizzard.Progression.SaveManager.Instance != null 
            ? Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier 
            : 0;
        
        int tokens = Pizzard.Progression.ProgressionManager.Instance != null 
            ? Pizzard.Progression.ProgressionManager.Instance.BossCurrency 
            : 0;

        foreach (var equip in availableEquipables)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            
            bool isOwned = equip.tier <= currentTier;
            bool isNextTier = equip.tier == currentTier + 1;
            bool isLocked = equip.tier > currentTier + 1;
            
            Button btn = buttonGO.GetComponent<Button>();
            
            string labelText = equip.displayName;
            if (isOwned)
            {
                labelText += " (Owned)";
                btn.interactable = false;
            }
            else if (isNextTier)
            {
                // This is the wand the player can buy next
                labelText += tokens > 0 ? " (Buy - 1 Token)" : " (Need Token)";
                btn.interactable = tokens > 0;
            }
            else
            {
                // Higher tier - locked
                labelText += " (Locked)";
                btn.interactable = false;
            }
            
            buttonGO.GetComponentInChildren<Text>().text = labelText;

            // Capture for lambda
            var capturedEquip = equip;
            btn.onClick.AddListener(() =>
            {
                OnWandPurchased(capturedEquip);
            });
        }
    }

    private void OnWandPurchased(EquipableObject equip)
    {
        // Spend 1 token
        if (Pizzard.Progression.ProgressionManager.Instance == null || 
            !Pizzard.Progression.ProgressionManager.Instance.SpendCurrency(1))
        {
            Debug.LogWarning("[EquipSelector] Cannot afford wand!");
            return;
        }

        // Update wandTier in SaveData
        if (Pizzard.Progression.SaveManager.Instance != null)
        {
            Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier = equip.tier;
            Pizzard.Progression.SaveManager.Instance.SaveGame();
        }

        // Equip on the player if available
        if (playerEquip != null)
        {
            playerEquip.EquipObject(equip);
        }

        Debug.Log($"[EquipSelector] Purchased & equipped: {equip.displayName} (Tier {equip.tier})");

        // Refresh the ShopUI (tokens, exit button, element selector)
        var shopUI = FindObjectOfType<ShopUI>(true);
        if (shopUI != null)
        {
            shopUI.hasPurchasedInRun1 = true;
            shopUI.RefreshTokens();
            shopUI.RefreshExitButton();
            
            // Immediately show element selector for the new tier
            var elementUI = shopUI.elementSelectionUI;
            if (elementUI != null)
            {
                elementUI.gameObject.SetActive(true);
                elementUI.SwitchToWand(equip, equip.tier);
            }
        }

        // Regenerate buttons to reflect new state
        GenerateButtons();
    }
}