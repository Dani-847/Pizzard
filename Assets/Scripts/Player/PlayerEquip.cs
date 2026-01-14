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
        
        FindObjectOfType<ElementsCombiner>()?.ClearSelectedElements();
    }
}
