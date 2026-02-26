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
            
            bool isUnlocked = playerEquip != null && equip.tier <= playerEquip.CurrentWandTier;
            Button btn = buttonGO.GetComponent<Button>();
            btn.interactable = isUnlocked;

            string labelText = equip.displayName;
            if (!isUnlocked) labelText += " (Locked)";
            buttonGO.GetComponentInChildren<Text>().text = labelText;

            btn.onClick.AddListener(() =>
            {
                if (playerEquip != null)
                {
                    playerEquip.EquipObject(equip);
                }
                else
                {
                    Debug.LogWarning("[EquipSelector] No PlayerEquip found! Wand selection bypassed for now.");
                }
                gameObject.SetActive(false);
            });
        }
    }
}