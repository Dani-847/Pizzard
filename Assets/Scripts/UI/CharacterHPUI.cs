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
    public GameObject corazonLleno;
    // Imagen medio corazón
    public GameObject medioCorazon;
    // Imagen vacío
    public GameObject corazonVacio;

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

            if (valor >= 2) {
                corazones[i].GetComponent<Image>().color = corazonLleno.GetComponent<SpriteRenderer>().color;
            } else if (valor == 1) {
                corazones[i].GetComponent<Image>().color = medioCorazon.GetComponent<SpriteRenderer>().color;
            } else {
                corazones[i].GetComponent<Image>().color = corazonVacio.GetComponent<SpriteRenderer>().color;
            }

        }
    }
}
