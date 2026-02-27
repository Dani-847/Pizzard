using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pizzard.Core;

public class ElementSelectionUI : MonoBehaviour
{
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    private PlayerEquip currentEquip;
    private Dictionary<ElementType, Button> elementButtons = new Dictionary<ElementType, Button>();

    public void OpenSelection(PlayerEquip equip)
    {
        currentEquip = equip;
        GenerateButtons();
        gameObject.SetActive(true);
        RefreshVisuals();
    }

    public void CloseSelection()
    {
        ClearButtons();
        currentEquip = null;
        gameObject.SetActive(false);
    }

    void GenerateButtons()
    {
        ClearButtons();

        foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
        {
            if (element == ElementType.None) continue; // Skip None

            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            buttonGO.GetComponentInChildren<Text>().text = element.ToString();
            
            Button btn = buttonGO.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                TryAddElementToWeapon(element);
            });

            elementButtons[element] = btn;
        }

        // Separator logic or simply the Reset button
        GameObject resetButtonGO = Instantiate(buttonPrefab, buttonContainer);
        resetButtonGO.GetComponentInChildren<Text>().text = "Reset Choices";
        resetButtonGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            ResetWeaponElements();
        });
    }

    void ResetWeaponElements()
    {
        if (currentEquip == null || currentEquip.equipedObject == null) return;

        currentEquip.equipedObject.elements.Clear();
        // Also clear PlayerEquip's duplicate list which dictates the Combiner
        currentEquip.elementsToShow.Clear(); 

        Debug.Log("[Shop] Element choices reset.");
        RefreshVisuals();
    }

    void TryAddElementToWeapon(ElementType element)
    {
        if (currentEquip == null || currentEquip.equipedObject == null) return;

        var weapon = currentEquip.equipedObject;
        int maxAllowed = currentEquip.CurrentWandTier; // Tier 1 = 1 element, Tier 2 = 2 elements...

        if (weapon.elements.Contains(element))
        {
            Debug.Log($"[Shop] Element {element} is already selected.");
            return;
        }

        if (weapon.elements.Count >= maxAllowed)
        {
            Debug.Log($"[Shop] Reached limit of {maxAllowed} elements for your Tier {maxAllowed} wand!");
            return;
        }

        weapon.elements.Add(element);
        currentEquip.elementsToShow.Add(element); // Sync to Combiner
        Debug.Log($"[Shop] Added Element: {element}. ({weapon.elements.Count}/{maxAllowed})");
        
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        if (currentEquip == null || currentEquip.equipedObject == null) return;
        var selectedElements = currentEquip.equipedObject.elements;

        foreach (var kvp in elementButtons)
        {
            Button btn = kvp.Value;
            Image btnImage = btn.GetComponent<Image>();
            
            if (btnImage != null)
            {
                if (selectedElements.Contains(kvp.Key))
                {
                    btnImage.color = new Color(0.5f, 1f, 0.5f); // Green for selected
                }
                else
                {
                    btnImage.color = Color.white; // Default for unselected
                }
            }
        }
    }

    void ClearButtons()
    {
        elementButtons.Clear();
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }
}