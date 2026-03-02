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
    [Header("Sprites de Vida")]
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    public void ActualizarUI(int vidaActual)
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            int valor = (vidaActual - (i * 2));
            Image heartImage = corazones[i].GetComponent<Image>();

            if (heartImage == null) continue;

            // Asegurar que el color sea blanco puro para que el sprite se vea tal cual
            heartImage.color = Color.white;

            if (valor >= 2) {
                if (fullHeart != null) heartImage.sprite = fullHeart;
            } else if (valor == 1) {
                if (halfHeart != null) heartImage.sprite = halfHeart;
            } else {
                if (emptyHeart != null) heartImage.sprite = emptyHeart;
            }
        }
    }
}
