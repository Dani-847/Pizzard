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
    public Sprite corazonLleno;
    // Imagen medio corazón
    public Sprite medioCorazon;
    // Imagen vacío
    public Sprite corazonVacio;

    // Inicializa la UI según la vida maxima del jugador
    void Start()
    {
        //ActualizarUI(corazones.Length * 2); 
    }

    // Actualiza la imagen de cada corazón en función de la vida actual:
    // 0 = vacío, 1 = medio, 2 = lleno
    public void ActualizarUI(int vidaActual)
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            int valor = (vidaActual - (i * 2));
            Image heartImage = corazones[i].GetComponent<Image>();

            if (valor >= 2) {
                if (corazonLleno != null) heartImage.sprite = corazonLleno;
                heartImage.color = Color.white; // Normalize color
            } else if (valor == 1) {
                if (medioCorazon != null) heartImage.sprite = medioCorazon;
                heartImage.color = Color.white;
            } else {
                if (corazonVacio != null) heartImage.sprite = corazonVacio;
                heartImage.color = Color.white;
            }
        }
    }
}
