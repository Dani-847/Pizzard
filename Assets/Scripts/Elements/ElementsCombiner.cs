using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ElementsCombiner : MonoBehaviour
{
    public PlayerEquip playerEquip;
    public ElementsUI elementsUI;
    public ElementsRegister elementsRegister;

    // Opcional: referencia directa a la database si prefieres no pasar por elementsUI
    public CombinationDatabase database;

    public List<ElementType> selectedElements = new List<ElementType>();

    void Start()
    {
        if (playerEquip == null)
        {
            playerEquip = FindObjectOfType<PlayerEquip>();
        }

        if (elementsUI == null)
        {
            elementsUI = FindObjectOfType<ElementsUI>();
        }

        if (elementsRegister == null)
        {
            elementsRegister = FindObjectOfType<ElementsRegister>();
        }

        // Si no asignaste database directamente, intenta usar la que tenga elementsUI (si existe)
        if (database == null && elementsUI != null)
        {
            database = elementsUI.database;
        }
    }

    public void OnSelectElement1(InputAction.CallbackContext context)
    {
        if (context.performed)
            TrySelectElementAtIndex(0);
    }

    public void OnSelectElement2(InputAction.CallbackContext context)
    {
        if (context.performed)
            TrySelectElementAtIndex(1);
    }

    public void OnSelectElement3(InputAction.CallbackContext context)
    {
        if (context.performed)
            TrySelectElementAtIndex(2);
    }

    public void TrySelectElementAtIndex(int index)
    {
        if (playerEquip.equipedObject == null) {
            Debug.Log("El jugador no tiene una varita seleccionada");
            return;
        }
        if (index >= playerEquip.equipedObject.elements.Count) {
            Debug.Log("El arma no tiene un elemento en la posicion: " + index);
            return;
        }
        if (playerEquip.equipedObject.elements.Count <= selectedElements.Count) {
            Debug.Log("Ya has seleccionado todos los elementos posibles");
            return;
        }

        ElementType elementToAdd = playerEquip.equipedObject.elements[index];
        selectedElements.Add(elementToAdd);
        elementsUI.UpdateUI(selectedElements);

        // Registrar combinación para 1, 2 o 3 elementos
        CheckAndRegisterCombination();
    }

    private void CheckAndRegisterCombination()
    {
        // Probar combinaciones de 1, 2 y 3 elementos
        for (int elementCount = 1; elementCount <= selectedElements.Count; elementCount++)
        {
            // Tomar solo los primeros 'elementCount' elementos
            List<ElementType> currentCombination = selectedElements.GetRange(0, elementCount);
            string combinationKey = BuildKey(currentCombination);

            // Intentar obtener entry desde la database (primera opción: elementsUI.database, luego database)
            CombinationEntry entry = null;

            if (elementsUI != null && elementsUI.database != null)
            {
                entry = elementsUI.database.GetByKey(combinationKey);
            }

            if (entry == null && database != null)
            {
                entry = database.GetByKey(combinationKey);
            }

            if (entry != null && elementsRegister != null)
            {
                // Registrar la combinación si existe en la database
                elementsRegister.RegistrarCombinacion(combinationKey);
            }
            else if (entry == null && elementCount <= 2) // Solo log para combinaciones pequeñas
            {
                Debug.LogWarning("No se encontró la combinación en la base de datos: " + combinationKey);
            }
        }
    }

    // Construcción de clave unificada
    private string BuildKey(List<ElementType> elements)
    {
        List<string> names = new List<string>();
        foreach (var e in elements)
            names.Add(e.ToString().ToLower());

        return string.Join("|", names);
    }

    // Método público que devuelve la key actual construida (útil para PlayerAimAndCast)
    public string GetCurrentKey()
    {
        return BuildKey(selectedElements);
    }

    public List<ElementType> GetSelectedElements()
    {
        return selectedElements;
    }

    public void ClearSelectedElements()
    {
        selectedElements.Clear();
        elementsUI.ClearUI();
    }
}
