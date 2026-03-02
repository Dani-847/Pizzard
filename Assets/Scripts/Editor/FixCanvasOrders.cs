using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class FixCanvasOrders
{
    [MenuItem("Tools/Fix UI Canvas Layers")]
    public static void Execute()
    {
        // 1. Fix DialogUI fading / overlapping
        GameObject dialogUI = GameObject.Find("DialogUI");
        if (dialogUI != null)
        {
            // Set it to bottom of hierarchy to naturally render last
            dialogUI.transform.SetAsLastSibling();
            
            // Give it an override Canvas so it's guaranteed on top
            Canvas dCanvas = dialogUI.GetComponent<Canvas>();
            if (dCanvas == null) dCanvas = dialogUI.AddComponent<Canvas>();
            dCanvas.overrideSorting = true;
            dCanvas.sortingOrder = 100; // High order

            GraphicRaycaster dRaycaster = dialogUI.GetComponent<GraphicRaycaster>();
            if (dRaycaster == null) dialogUI.AddComponent<GraphicRaycaster>();

            // Fix the color fading (the background shadow)
            Image shadow = dialogUI.GetComponent<Image>();
            if (shadow != null)
            {
                // Most likely a dark overlay is applied, let's dial down alpha or disable
                var c = shadow.color;
                c.a = 0.2f; // make it less faded
                shadow.color = c;
            }

            // Ensure DialogPanel has no weird tint
            Transform dPanel = dialogUI.transform.Find("DialogPanel");
            if (dPanel != null)
            {
                Image panelImg = dPanel.GetComponent<Image>();
                if (panelImg != null)
                {
                    panelImg.color = Color.white; // Full bright sprite
                }
            }
        }

        // 2. Fix VictoryUI not showing fully
        GameObject victoryUI = GameObject.Find("VictoryUI");
        if (victoryUI != null)
        {
            // Set it to bottom of hierarchy
            victoryUI.transform.SetAsLastSibling();

            // Give it an override Canvas
            Canvas vCanvas = victoryUI.GetComponent<Canvas>();
            if (vCanvas == null) vCanvas = victoryUI.AddComponent<Canvas>();
            vCanvas.overrideSorting = true;
            vCanvas.sortingOrder = 105; // Even higher than dialog

            GraphicRaycaster vRaycaster = victoryUI.GetComponent<GraphicRaycaster>();
            if (vRaycaster == null) victoryUI.AddComponent<GraphicRaycaster>();

            // Ensure its children are active when VictoryUI is requested
            Transform contentPanel = victoryUI.transform.Find("DialogPanel");
            if (contentPanel != null)
            {
                contentPanel.gameObject.SetActive(true); // Must remain active in structure since UI script toggles it
            }

            // Also check for standard layout issues
            Transform textTr = victoryUI.transform.Find("DialogText");
            if (textTr != null) textTr.gameObject.SetActive(true);

            Transform logoTr = victoryUI.transform.Find("Logo");
            if (logoTr != null) logoTr.gameObject.SetActive(true);
        }

        Debug.Log("UI Canvas Sorting Fixed!");
    }
}
