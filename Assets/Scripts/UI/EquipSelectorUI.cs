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

        int currentTier;
        int tokens;
        if (PlaygroundManager.IsPlaygroundSession)
        {
            currentTier = PlaygroundManager.PlaygroundWandTier;
            tokens = PlaygroundManager.GetCachedTokens();
        }
        else
        {
            currentTier = Pizzard.Progression.SaveManager.Instance != null
                ? Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier
                : 0;
            tokens = Pizzard.Progression.ProgressionManager.Instance != null
                ? Pizzard.Progression.ProgressionManager.Instance.BossCurrency
                : 0;
        }

        foreach (var equip in availableEquipables)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            
            bool isOwned = equip.tier <= currentTier;
            bool isNextTier = equip.tier == currentTier + 1;
            bool isLocked = equip.tier > currentTier + 1;
            
            Button btn = buttonGO.GetComponent<Button>();
            
            var loc = LocalizationManager.Instance;
            string labelText = equip.displayName;
            if (isOwned)
            {
                labelText += " (" + (loc != null ? loc.GetText("shop_equip_owned") : "Owned") + ")";
                btn.interactable = false;
            }
            else if (isNextTier)
            {
                string status = tokens > 0
                    ? (loc != null ? loc.GetText("shop_equip_buy") : "Buy - 1 Token")
                    : (loc != null ? loc.GetText("shop_equip_need_token") : "Need Token");
                labelText += " (" + status + ")";
                btn.interactable = tokens > 0;
            }
            else
            {
                labelText += " (" + (loc != null ? loc.GetText("shop_equip_locked") : "Locked") + ")";
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
        if (PlaygroundManager.IsPlaygroundSession)
        {
            if (!PlaygroundManager.SpendCachedTokens(1))
            {
                Debug.LogWarning("[EquipSelector] Cannot afford wand (playground)!");
                return;
            }
            PlaygroundManager.PlaygroundWandTier = equip.tier;
        }
        else
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