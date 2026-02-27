//CharacterHPUI.cs

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Representar gráficamente la vida del jugador en corazones, cada corazón tiene dos mitades.
public class CharacterHPUI : MonoBehaviour
{
    [Header("Configuración de UI")]
    // Array de imágenes de corazones (3 corazones)
    public GameObject[] corazones;
    // Imagen corazón completo
    public void ActualizarUI(int vidaActual)
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            int valor = (vidaActual - (i * 2));
            Image heartImage = corazones[i].GetComponent<Image>();

            if (heartImage == null) continue;

            if (valor >= 2) {
                heartImage.color = Color.red;
            } else if (valor == 1) {
                heartImage.color = Color.yellow;
            } else {
                heartImage.color = Color.gray;
            }
        }
    }
}
