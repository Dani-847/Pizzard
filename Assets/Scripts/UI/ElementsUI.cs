using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementsUI : MonoBehaviour
{
    [Header("UI Slots")]
    public Image primerElemento;
    public Image segundoElemento;
    public Image tercerElemento;
    public Image combinacionElemento;

    [Header("Sprites base por tipo")]
    public Sprite pepperoniSprite;
    public Sprite piñaSprite;
    public Sprite quesoSprite;

    [Header("Database de combinaciones")]
    public CombinationDatabase database;

    private Dictionary<ElementType, Sprite> baseSprites;

    void Awake()
    {
        baseSprites = new Dictionary<ElementType, Sprite>()
        {
            { ElementType.queso, quesoSprite },
            { ElementType.pepperoni, pepperoniSprite },
            { ElementType.piña, piñaSprite }
        };

        ClearUI();
    }

    public void UpdateUI(List<ElementType> elements)
    {
        ClearUI();

        int count = elements.Count;

        if (count >= 1)
        {
            primerElemento.sprite = baseSprites[elements[0]];
            primerElemento.gameObject.SetActive(true);
        }

        if (count >= 2)
        {
            segundoElemento.sprite = baseSprites[elements[1]];
            segundoElemento.gameObject.SetActive(true);
        }

        if (count >= 3)
        {
            tercerElemento.sprite = baseSprites[elements[2]];
            tercerElemento.gameObject.SetActive(true);
        }

        string combinationKey = BuildKey(elements);
        var entry = database.GetByKey(combinationKey);

        if (entry != null)
        {
            combinacionElemento.sprite = entry.resultSprite;
            combinacionElemento.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No existe combinación en database: " + combinationKey);
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

    public void ClearUI()
    {
        primerElemento.gameObject.SetActive(false);
        segundoElemento.gameObject.SetActive(false);
        tercerElemento.gameObject.SetActive(false);
        combinacionElemento.gameObject.SetActive(false);
    }
}