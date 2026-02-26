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
                Pizzard.Progression.SaveManager.Instance.CurrentSave.currentWandTier = CurrentWandTier;
            }
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
        CurrentWandTier = Mathf.Clamp(savedTier, 1, 3);
        Debug.Log($"[PlayerEquip] Wand loaded at Tier {CurrentWandTier}");
    }
}
