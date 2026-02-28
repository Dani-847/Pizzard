using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pizzard.Core;

public class ElementSelectionUI : MonoBehaviour
{
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    private PlayerEquip currentEquip;
    private EquipableObject currentWand;
    private int maxAllowedTier;

    private Dictionary<ElementType, Button> elementButtons = new Dictionary<ElementType, Button>();
    private GameObject resetButtonGO;

    public void OpenSelection(PlayerEquip equip)
    {
        currentEquip = equip;
        currentWand = equip.equipedObject;
        maxAllowedTier = equip.CurrentWandTier;

        GenerateButtons();
        gameObject.SetActive(true);
        RefreshVisuals();
    }

    public void OpenSelectionWithoutPlayer(EquipableObject wand, int maxTier)
    {
        currentEquip = null;
        currentWand = wand;
        maxAllowedTier = maxTier;

        GenerateButtons();
        gameObject.SetActive(true);
        RefreshVisuals();
    }

    /// <summary>
    /// Called by ShopUI when wandTier changes (e.g., after purchasing a wand upgrade).
    /// Updates the number of active element slots IMMEDIATELY.
    /// </summary>
    public void RefreshFromWandTier(int newTier)
    {
        maxAllowedTier = newTier;
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
            if (element == ElementType.None) continue;

            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            buttonGO.GetComponentInChildren<Text>().text = element.ToString();
            
            Button btn = buttonGO.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                TryAddElementToWeapon(element);
            });

            elementButtons[element] = btn;
        }

        // Deselect / Reset button
        resetButtonGO = Instantiate(buttonPrefab, buttonContainer);
        resetButtonGO.GetComponentInChildren<Text>().text = "Deselect All";
        resetButtonGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            ResetWeaponElements();
        });
    }

    void ResetWeaponElements()
    {
        if (currentWand == null) return;

        currentWand.elements.Clear();
        if (currentEquip != null)
        {
            currentEquip.elementsToShow.Clear(); 
        }

        Debug.Log("[Shop] Element choices reset.");
        RefreshVisuals();
    }

    void TryAddElementToWeapon(ElementType element)
    {
        if (currentWand == null) return;

        var weapon = currentWand;
        int maxAllowed = maxAllowedTier; // Tier 1 = 1 element, Tier 2 = 2, Tier 3 = 3

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
        if (currentEquip != null) currentEquip.elementsToShow.Add(element);
        Debug.Log($"[Shop] Added Element: {element}. ({weapon.elements.Count}/{maxAllowed})");
        
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        // --- WAVE 2: REACTIVE BINDING ---
        // wandTier 0 = ALL grayed/disabled
        // wandTier N = N slots active, rest disabled
        int currentSelectedCount = currentWand != null ? currentWand.elements.Count : 0;
        bool hasAnyTier = maxAllowedTier > 0;

        foreach (var kvp in elementButtons)
        {
            Button btn = kvp.Value;
            Image btnImage = btn.GetComponent<Image>();

            if (!hasAnyTier)
            {
                // Tier 0: Everything grayed out and non-interactable
                btn.interactable = false;
                if (btnImage != null) btnImage.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            }
            else
            {
                bool isSelected = currentWand != null && currentWand.elements.Contains(kvp.Key);
                bool canSelectMore = currentSelectedCount < maxAllowedTier;
                
                btn.interactable = isSelected || canSelectMore;
                
                if (btnImage != null)
                {
                    if (isSelected)
                        btnImage.color = new Color(0.5f, 1f, 0.5f); // Green = selected
                    else if (canSelectMore)
                        btnImage.color = Color.white; // White = available
                    else
                        btnImage.color = new Color(0.6f, 0.6f, 0.6f, 0.7f); // Gray = slots full
                }
            }
        }

        // Deselect button: hidden when wandTier == 0 or no elements selected
        if (resetButtonGO != null)
        {
            resetButtonGO.SetActive(hasAnyTier && currentSelectedCount > 0);
        }
    }

    void ClearButtons()
    {
        elementButtons.Clear();
        resetButtonGO = null;
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }
}