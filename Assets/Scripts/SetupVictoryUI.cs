using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class SetupVictoryUI : MonoBehaviour
{
    private void Awake()
    {
        // 1. Encontrar el GameObject VictoryUI (por nombre)
        GameObject victoryObj = GameObject.Find("VictoryUI");
        if (victoryObj == null)
        {
            Debug.LogError("No se encontró 'VictoryUI' en la escena.");
            return;
        }

        // 2. Eliminar DialogUI viejo y agregar VictoryUI real
        DialogUI dialogRef = victoryObj.GetComponent<DialogUI>();
        if (dialogRef != null) DestroyImmediate(dialogRef);

        VictoryUI victoryScript = victoryObj.GetComponent<VictoryUI>();
        if (victoryScript == null) victoryScript = victoryObj.AddComponent<VictoryUI>();

        // 3. Obtener referencias de nodos hijos
        Transform dialogPanel = victoryObj.transform.Find("DialogPanel");
        if (dialogPanel == null) return;

        Transform dialogTextTr = victoryObj.transform.Find("DialogText");
        if (dialogTextTr == null) return;
        TextMeshProUGUI textComp = dialogTextTr.GetComponent<TextMeshProUGUI>();

        Transform logoTr = victoryObj.transform.Find("Logo");
        if (logoTr == null) return;
        Image logoImg = logoTr.GetComponent<Image>();

        // 4. Transformar los botones
        Transform buttonNext = victoryObj.transform.Find("ButtonNext");
        if (buttonNext == null) buttonNext = victoryObj.transform.Find("ButtonMenu");
        
        GameObject buttonMenuGO = null;
        GameObject buttonExitGO = null;
        
        if (buttonNext != null)
        {
            buttonNext.gameObject.name = "ButtonMenu";
            buttonMenuGO = buttonNext.gameObject;
            
            Transform exitCheck = victoryObj.transform.Find("ButtonExit");
            if (exitCheck == null)
            {
                buttonExitGO = Instantiate(buttonMenuGO, victoryObj.transform);
                buttonExitGO.name = "ButtonExit";
            }
            else
            {
                buttonExitGO = exitCheck.gameObject;
            }
        }
        else
        {
            return;
        }

        // 4.1. Configurar posiciones de botones
        RectTransform rtMenu = buttonMenuGO.GetComponent<RectTransform>();
        RectTransform rtExit = buttonExitGO.GetComponent<RectTransform>();
        rtMenu.anchoredPosition = new Vector2(-200, rtMenu.anchoredPosition.y);
        rtExit.anchoredPosition = new Vector2(200, rtMenu.anchoredPosition.y);
        
        buttonMenuGO.SetActive(true);
        buttonExitGO.SetActive(true);

        // 4.2. Cambiar textos de botones
        TextMeshProUGUI txtMenu = buttonMenuGO.GetComponentInChildren<TextMeshProUGUI>();
        if (txtMenu != null) txtMenu.text = "Menu";

        TextMeshProUGUI txtExit = buttonExitGO.GetComponentInChildren<TextMeshProUGUI>();
        if (txtExit != null) txtExit.text = "Exit";

        // 5. Asignar todo en el Inspector (script)
        victoryScript.panelVictoria = dialogPanel.gameObject;
        victoryScript.victoryImage = logoImg;
        victoryScript.victoryText = textComp;
        victoryScript.botonMenu = buttonMenuGO.GetComponent<Button>();
        victoryScript.botonSalir = buttonExitGO.GetComponent<Button>();

        // 6. Set main text
        textComp.text = "Enhorabuena has ganado!!!";

        // Remove the speech_bubble sprite stuff if it got copied over to dialogPanel
        var sync = dialogPanel.GetComponent<SyncSpriteToImage>();
        if (sync != null) DestroyImmediate(sync);
        var anim = dialogPanel.GetComponent<Animator>();
        if (anim != null) DestroyImmediate(anim);

        // Alinear Logo arriba
        RectTransform logoRt = logoTr.GetComponent<RectTransform>();
        if (logoRt != null)
        {
            logoRt.anchorMin = new Vector2(0.5f, 1f);
            logoRt.anchorMax = new Vector2(0.5f, 1f);
            logoRt.pivot = new Vector2(0.5f, 1f);
            logoRt.anchoredPosition = new Vector2(0, -50);
        }
        
        // Re-asignar en el UIManager
        UIManager uim = FindObjectOfType<UIManager>();
        if (uim != null)
        {
            uim.victoryUI = victoryScript;
        }

        Debug.Log("VictoryUI completado con éxito! Destruyendo SetupVictoryUI...");
        
        // Destroy this exact script instance
        DestroyImmediate(this);
    }
}
