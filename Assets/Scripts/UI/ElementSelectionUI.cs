using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementSelectionUI : MonoBehaviour
{
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    private PlayerEquip currentEquip;

    void Start()
    {
        
    }

    public void OpenSelection(PlayerEquip equip)
    {
        currentEquip = equip;
        GenerateButtons();
        gameObject.SetActive(true);
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
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            buttonGO.GetComponentInChildren<Text>().text = element.ToString();

            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                TryAddElementToWeapon(element);
            });
        }

        GameObject resetButtonGO = Instantiate(buttonPrefab, buttonContainer);
        resetButtonGO.GetComponentInChildren<Text>().text = "Reset";
        resetButtonGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            ResetWeaponElements();
        });
    }

    void ResetWeaponElements()
    {
        if (currentEquip == null || currentEquip.equipedObject == null) return;

        currentEquip.equipedObject.elements.Clear();
        Debug.Log("Lista de elementos reseteada.");
    }

    void TryAddElementToWeapon(ElementType element)
    {
        if (currentEquip == null || currentEquip.equipedObject == null) return;

        var weapon = currentEquip.equipedObject;

        if (weapon.elements.Contains(element))
        {
            Debug.Log("Este elemento ya ha sido seleccionado.");
            return;
        }

        if (weapon.elements.Count >= weapon.MaxElements)
        {
            Debug.Log("Has alcanzado el límite de elementos para este arma.");
            return;
        }

        weapon.elements.Add(element);
        Debug.Log("Elemento añadido: " + element);

        if (weapon.elements.Count >= weapon.MaxElements)
        {
            Debug.Log("Selección completada.");
            CloseSelection();
        }
    }

    void ClearButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);
    }
}