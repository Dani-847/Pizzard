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
            playerEquip = FindObjectOfType<PlayerEquip>();

        if (elementsUI == null)
        {
            elementsUI = FindObjectOfType<ElementsUI>();
            if (elementsUI == null)
                Debug.LogWarning("[ElementsCombiner] ElementsUI not found at Start (may be inactive — will retry).");
        }

        if (elementsRegister == null)
            elementsRegister = FindObjectOfType<ElementsRegister>();

        // Si no asignaste database directamente, intenta usar la que tenga elementsUI (si existe)
        if (database == null && elementsUI != null)
            database = elementsUI.database;
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

    private string lastCombinationCast = "";
    private int sameCastCount = 0;
    public float spamCostMultiplier = 1.0f;
    private float spamResetTimer = 0f;
    public float spamResetDelay = 3.0f; // Seconds before combo counter resets
    private float elementsUIRetryTimer = 0f; // Retry finding ElementsUI every 0.5s if null

    private void Update()
    {
        // Retry finding ElementsUI if it was inactive at Start (activated later by GameFlowManager)
        if (elementsUI == null)
        {
            elementsUIRetryTimer -= Time.unscaledDeltaTime;
            if (elementsUIRetryTimer <= 0f)
            {
                elementsUIRetryTimer = 0.5f;
                elementsUI = FindObjectOfType<ElementsUI>();
                if (elementsUI != null)
                {
                    if (database == null) database = elementsUI.database;
                    Debug.Log("[ElementsCombiner] ElementsUI found on retry.");
                }
            }
        }

        if (spamResetTimer > 0)
        {
            spamResetTimer -= Time.deltaTime;
            if (spamResetTimer <= 0)
            {
                sameCastCount = 0;
                spamCostMultiplier = 1.0f;
                lastCombinationCast = "";
            }
        }
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

        // Apply Wand Tier Limit
        // Tier 1 -> Max 1 element
        // Tier 2 -> Max 2 elements
        // Tier 3+ -> 3+ elements
        int maxAllowed = playerEquip.CurrentWandTier;
        if (maxAllowed > 3) maxAllowed = 3; // Hardcap at 3 for safety based on current 3-slot UI

        if (selectedElements.Count >= maxAllowed || selectedElements.Count >= playerEquip.equipedObject.MaxElements) {
            Debug.Log($"Wand Tier {playerEquip.CurrentWandTier}: Limits reached.");
            return;
        }

        ElementType elementToAdd = playerEquip.equipedObject.elements[index];
        selectedElements.Add(elementToAdd);
        if (elementsUI != null) elementsUI.UpdateUI(selectedElements);

        // Registrar combinación para 1, 2 o 3 elementos
        CheckAndRegisterCombination();
    }

    /// <summary>
    /// Call this when the spell is actually fired/cast to increment spam counters.
    /// </summary>
    public void RegisterCast(string combinationKey)
    {
        if (combinationKey == lastCombinationCast)
        {
            sameCastCount++;
            spamCostMultiplier = 1.0f + (sameCastCount * 0.5f); // +50% cost per consecutive cast
        }
        else
        {
            sameCastCount = 0;
            spamCostMultiplier = 1.0f;
            lastCombinationCast = combinationKey;
        }
        spamResetTimer = spamResetDelay;
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
            else if (entry == null && elementCount <= 2) // Solo log para combinaciones pequenas
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
        if (elementsUI != null) elementsUI.ClearUI();
    }
}
