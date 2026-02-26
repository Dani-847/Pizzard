using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipSelectorUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    private PlayerEquip playerEquip;

    public List<EquipableObject> availableEquipables;

    void Start()
    {
        playerEquip = FindObjectOfType<PlayerEquip>();
        GenerateButtons();
    }

    public void GenerateButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var equip in availableEquipables)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            buttonGO.GetComponentInChildren<Text>().text = equip.displayName;

            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (playerEquip != null)
                {
                    playerEquip.EquipObject(equip);
                }
                else
                {
                    Debug.LogWarning("[EquipSelector] No PlayerEquip found! Wand selection bypassed for now.");
                }
                gameObject.SetActive(false);
            });
        }
    }
}