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

        foreach (var equip in availableEquipables)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            
            int currentTier = playerEquip != null ? playerEquip.CurrentWandTier 
                : (Pizzard.Progression.SaveManager.Instance != null ? Pizzard.Progression.SaveManager.Instance.CurrentSave.currentWandTier : 1);

            bool isUnlocked = equip.tier <= currentTier;
            Button btn = buttonGO.GetComponent<Button>();
            btn.interactable = isUnlocked;

            string labelText = equip.displayName;
            if (!isUnlocked) labelText += " (Locked)";
            buttonGO.GetComponentInChildren<Text>().text = labelText;

            btn.onClick.AddListener(() =>
            {
                if (Pizzard.Progression.SaveManager.Instance != null)
                {
                    Pizzard.Progression.SaveManager.Instance.CurrentSave.selectedWandTierEquipped = equip.tier;
                }

                if (playerEquip != null)
                {
                    playerEquip.EquipObject(equip);
                }
                else
                {
                    Debug.Log("[EquipSelector] Wand saved to SaveManager. PlayerEquip not in scene yet to visually hold it.");
                }
                
                var shopUI = FindObjectOfType<ShopUI>(true);
                if (shopUI != null)
                {
                    shopUI.hasPurchasedInRun1 = true;
                }
                
                gameObject.SetActive(false);
            });
        }
    }
}