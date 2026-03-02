using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class FixButtonTexts : MonoBehaviour
{
    private void Awake()
    {
        // 1. Fix Main Menu "Jugar" / "Continuar" Texts
        GameObject menuUI = GameObject.Find("MenuUI");
        if (menuUI != null)
        {
            Transform panelMenu = menuUI.transform.Find("PanelMenu");
            if (panelMenu != null)
            {
                Transform btnJugar = panelMenu.Find("ButtonJugar");
                if (btnJugar != null)
                {
                    TextMeshProUGUI txt = btnJugar.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null) txt.text = "Jugar / Play";
                }

                Transform btnContinuar = panelMenu.Find("ButtonContinuar");
                if (btnContinuar != null)
                {
                    TextMeshProUGUI txt = btnContinuar.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null) txt.text = "Continuar / Continue";
                }
            }
        }

        // 2. Fix Victory UI Buttons
        GameObject victoryUI = GameObject.Find("VictoryUI");
        if (victoryUI != null)
        {
            Transform textTr = victoryUI.transform.Find("DialogText");
            if (textTr != null)
            {
                TextMeshProUGUI txt = textTr.GetComponent<TextMeshProUGUI>();
                if (txt != null) txt.text = "Enhorabuena has ganado!!!";
            }

            Transform btnMenu = victoryUI.transform.Find("ButtonMenu");
            if (btnMenu != null)
            {
                TextMeshProUGUI txt = btnMenu.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null) txt.text = "Menu";
            }

            Transform btnExit = victoryUI.transform.Find("ButtonExit");
            if (btnExit != null)
            {
                TextMeshProUGUI txt = btnExit.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null) txt.text = "Exit";
            }
        }

        Debug.Log("Button texts forced successfully! Destroying script...");
        DestroyImmediate(this);
    }
}
