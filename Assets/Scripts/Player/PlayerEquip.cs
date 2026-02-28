using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
-Equiparse una varita(EquipableObject) de forma física en un slot (dentro de la jerarquia y escena)
-Saber qué varita tiene equipada
-Tener SU LISTA de elementos(List<ElementType>) base
-Al tener esta varita equipada no pueda acceder a elementos que no estén en su lista
*/
public class PlayerEquip : MonoBehaviour
{
    public EquipableObject equipedObject;
    public List<ElementType> elementsToShow = new List<ElementType>();

    public Transform equipSlot;

    public List<ElementType> elementTypes = new List<ElementType>();

    private GameObject currentVisual;

    public int CurrentWandTier { get; private set; } = 1;

    void Start()
    {
        if (Pizzard.Progression.SaveManager.Instance != null)
        {
            LoadTierFromSave(Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier);
        }

        if (equipedObject == null)
        {
            Debug.Log("[PlayerEquip] No wand equipped. Waiting for player to equip one.");
        }
    }

    public void EquipObject (EquipableObject newEquipableObject)
    {
        equipedObject = newEquipableObject;
        elementsToShow = newEquipableObject.elements;

        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }
        if (newEquipableObject.visualPrefab != null && equipSlot != null){
            currentVisual = Instantiate(newEquipableObject.visualPrefab, equipSlot);
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = Quaternion.identity;
        }
        
        // Ensure UI and limits are respected after equip
        FindObjectOfType<ElementsCombiner>()?.ClearSelectedElements();
    }

    /// <summary>
    /// Upgrades the wand tier (Max Tier 3).
    /// Tier 1: 1 Element combinations
    /// Tier 2: 2 Element combinations
    /// Tier 3: 3+ Element combinations
    /// </summary>
    public void UpgradeWandTier()
    {
        if (CurrentWandTier < 3)
        {
            CurrentWandTier++;
            Debug.Log($"[PlayerEquip] Wand upgraded to Tier {CurrentWandTier}!");
            
            if (Pizzard.Progression.SaveManager.Instance != null)
            {
                Pizzard.Progression.SaveManager.Instance.CurrentSave.wandTier = CurrentWandTier;
                Pizzard.Progression.SaveManager.Instance.SaveGame();
            }
            
            // Automatically equip the new visual properties based on tier
            LoadTierFromSave(CurrentWandTier);
        }
        else
        {
            Debug.Log("[PlayerEquip] Wand is already at maximum tier!");
        }
    }

    /// <summary>
    /// Intialize the correct state from the Save file.
    /// </summary>
    public void LoadTierFromSave(int savedTier)
    {
        CurrentWandTier = Mathf.Clamp(savedTier, 0, 3);
        Debug.Log($"[PlayerEquip] Wand loaded at Tier {CurrentWandTier}");

        if (CurrentWandTier > 0)
        {
            var selector = FindObjectOfType<EquipSelectorUI>(true);
            
            if (selector != null && selector.availableEquipables != null)
            {
                // Auto-equip the wand corresponding to the current highest tier achieved
                var equip = selector.availableEquipables.Find(e => e.tier == CurrentWandTier);
                if (equip != null)
                {
                    EquipObject(equip);
                    Debug.Log($"[PlayerEquip] Auto-equipped wand: {equip.displayName}");
                }
            }
        }
        else
        {
            // Tier 0 (No wand yet)
            equipedObject = null;
            elementsToShow.Clear();
            if (currentVisual != null) Destroy(currentVisual);
            FindObjectOfType<ElementsCombiner>()?.ClearSelectedElements();
            Debug.Log("[PlayerEquip] Player is currently Wand Tier 0 (No attack possible).");
        }
    }
}
