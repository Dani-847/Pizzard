using TMPro;
using UnityEngine;

public class PotionUI : MonoBehaviour
{
    [SerializeField] private HealthPotionSystem healthPotionSystem;
    [SerializeField] private TextMeshProUGUI potionText;
    [SerializeField] private Color colorConPociones = Color.red;
    [SerializeField] private Color colorSinPociones = Color.white;

    private void Awake()
    {
        if (healthPotionSystem == null)
        {
            healthPotionSystem = FindObjectOfType<HealthPotionSystem>();
        }
    }

    private void Update()
    {
        if (healthPotionSystem == null || potionText == null)
            return;

        int currentPotions = healthPotionSystem.GetPocionesActuales();
        potionText.text = currentPotions.ToString();
        potionText.color = currentPotions > 0 ? colorConPociones : colorSinPociones;
    }
}