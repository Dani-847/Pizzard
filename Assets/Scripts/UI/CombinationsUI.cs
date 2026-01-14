using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombinationsUI : MonoBehaviour
{
    [Header("Panel principal")]
    public GameObject panelCombinations;

    [Header("Lista de combinaciones")]
    public Transform listContainer;         // Content del ScrollView
    public GameObject listItemPrefab;       // Prefab de un rectángulo completo

    [Header("Placeholder")]
    public Sprite placeholderSprite;        // Sprite de "???"

    [Header("Datos")]
    public CombinationDatabase database;    // ScriptableObject con todas las combinaciones

    [Header("Botón Volver")]
    public Button backButton;               // ⬅️ NUEVO: Botón para volver a opciones

    void Start()
    {
        // Configurar botón de volver
        if (backButton != null)
        {
            backButton.onClick.AddListener(VolverAOptions);
        }
    }

    // Mostrar el panel y refrescar su contenido
    public void MostrarPanel()
    {
        panelCombinations.SetActive(true);
        RefrescarLista();
    }

    // Ocultar panel
    public void OcultarPanel()
    {
        panelCombinations.SetActive(false);
    }

    // ⬅️ NUEVO: Método para volver a opciones
    public void VolverAOptions()
    {
        OcultarPanel();
        // Notificar a OptionsUI que se debe mostrar nuevamente
        FindObjectOfType<OptionsUI>()?.Show();
    }

    // Crear toda la lista de rectángulos de combinaciones
    public void RefrescarLista()
    {
        // Limpiar contenido previo
        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        // Verificar que tenemos el prefab
        if (listItemPrefab == null)
        {
            Debug.LogError("❌ listItemPrefab no está asignado en el Inspector");
            return;
        }

        // Verificar placeholder
        if (placeholderSprite == null)
        {
            Debug.LogError("❌ placeholderSprite no está asignado en el Inspector");
            return;
        }

        // Crear cada ítem basado en database
        foreach (var entry in database.GetAll())
        {
            GameObject item = Instantiate(listItemPrefab, listContainer);

            // Buscar referencias internas del prefab con verificación de errores
            Transform resultadoTransform = item.transform.Find("Resultado");
            Transform base1Transform = item.transform.Find("Base1");
            Transform base2Transform = item.transform.Find("Base2");
            Transform base3Transform = item.transform.Find("Base3");
            Transform nombreTransform = item.transform.Find("Nombre");
            Transform descripcionTransform = item.transform.Find("Descripcion");

            // Verificar que encontramos todos los elementos
            if (resultadoTransform == null) Debug.LogError("❌ No se encontró 'Resultado' en el prefab");
            if (base1Transform == null) Debug.LogError("❌ No se encontró 'Base1' en el prefab");
            if (base2Transform == null) Debug.LogError("❌ No se encontró 'Base2' en el prefab");
            if (base3Transform == null) Debug.LogError("❌ No se encontró 'Base3' en el prefab");
            if (nombreTransform == null) Debug.LogError("❌ No se encontró 'Nombre' en el prefab");
            if (descripcionTransform == null) Debug.LogError("❌ No se encontró 'Descripcion' en el prefab");

            Image resultadoImage = resultadoTransform?.GetComponent<Image>();
            Image base1 = base1Transform?.GetComponent<Image>();
            Image base2 = base2Transform?.GetComponent<Image>();
            Image base3 = base3Transform?.GetComponent<Image>();

            TMP_Text nombreText = nombreTransform?.GetComponent<TMP_Text>();
            TMP_Text descripcionText = descripcionTransform?.GetComponent<TMP_Text>();

            if (entry.isUnlocked)
            {
                // Verificar que la entrada tiene los sprites necesarios
                if (entry.resultSprite == null)
                    Debug.LogWarning($"⚠️ entry.resultSprite es null para: {entry.combinationKey}");

                // Mostrar sprite resultado
                if (resultadoImage != null)
                    resultadoImage.sprite = entry.resultSprite ?? placeholderSprite;

                // Elementos base
                if (base1 != null)
                    base1.sprite = (entry.baseElementSprites.Length > 0 && entry.baseElementSprites[0] != null) 
                        ? entry.baseElementSprites[0] : placeholderSprite;

                if (base2 != null)
                    base2.sprite = (entry.baseElementSprites.Length > 1 && entry.baseElementSprites[1] != null) 
                        ? entry.baseElementSprites[1] : placeholderSprite;

                if (base3 != null)
                    base3.sprite = (entry.baseElementSprites.Length > 2 && entry.baseElementSprites[2] != null) 
                        ? entry.baseElementSprites[2] : placeholderSprite;

                // Textos
                if (nombreText != null)
                    nombreText.text = entry.combinationName;

                if (descripcionText != null)
                    descripcionText.text = entry.description;
            }
            else
            {
                // Tapa todo con "???"
                if (resultadoImage != null)
                    resultadoImage.sprite = placeholderSprite;

                if (base1 != null)
                    base1.sprite = placeholderSprite;

                if (base2 != null)
                    base2.sprite = placeholderSprite;

                if (base3 != null)
                    base3.sprite = placeholderSprite;

                if (nombreText != null)
                    nombreText.text = "???";

                if (descripcionText != null)
                    descripcionText.text = "???";
            }
        }
    }

    // Llamado cuando se desbloquea una combinación nueva
    public void MarcarComoDescubierta(string combinationKey)
    {
        var entry = database.GetByKey(combinationKey);

        if (entry == null)
        {
            Debug.LogError("❌ Combination no encontrada al intentar desbloquear: " + combinationKey);
            return;
        }

        entry.isUnlocked = true;

        // Refrescar lista si el panel está visible
        if (panelCombinations.activeInHierarchy)
        {
            RefrescarLista();
        }
    }
}