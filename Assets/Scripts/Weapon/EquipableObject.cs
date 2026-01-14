using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
-Tiene el tier de la varita
-Tiene su List<ElementType>
-Obtiene el nombre de la varita a través de su tier y los elementos máximos que puede llevar
-Prefab visual
*/
public class EquipableObject : MonoBehaviour
{
    public int tier;

    public GameObject visualPrefab;

    public List<ElementType> elements = new List<ElementType>();

    public string displayName
    {
        get { 
            switch (tier)
            {
                case 1 : return "Adept Wand";
                case 2 : return "Master Wand";
                case 3 : return "Archmage Wand";
                default : return "Desconicido";
            }
        }
    }
    public int MaxElements => tier;

    public void SetElementList (List<ElementType> newElements)
    {
        elements = newElements;
    }
}
