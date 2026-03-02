using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class FixUIVisuals
{
    [MenuItem("Tools/Fix UI Visuals")]
    public static void Execute()
    {
        // --- 1. Fix DialogUI --- 
        GameObject dialogUIObj = GameObject.Find("DialogUI");
        if (dialogUIObj != null)
        {
            Transform dialogPanelTr = dialogUIObj.transform.Find("DialogPanel");
            if (dialogPanelTr != null)
            {
                // Unstretch it so the Speech Bubble sprite is actually visible in its native/scaled size
                RectTransform rt = dialogPanelTr.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    rt.anchoredPosition = new Vector2(0, 50); // Bottom center
                    rt.sizeDelta = new Vector2(800, 300); // Fixed size for the bubble
                }

                // Remove the semi-transparent dark tint that's overlapping the actual dialogue bubble
                Image img = dialogPanelTr.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white; // Solid white allows the true Base sprite colors to show
                    
                    // We must NOT have a source image if the Sync script is fighting it, but if it needs an active image, we just leave it white
                }

                var sync = dialogPanelTr.GetComponent<SyncSpriteToImage>();
                if (sync == null) dialogPanelTr.gameObject.AddComponent<SyncSpriteToImage>();
            }
        }

        // --- 2. Fix VictoryUI ---
        GameObject victoryUIObj = GameObject.Find("VictoryUI");
        if (victoryUIObj != null)
        {
            Transform dialogPanelTr = victoryUIObj.transform.Find("DialogPanel");
            if (dialogPanelTr != null)
            {
                // For Victory, we actually WANT the center stretched panel, but maybe fully transparent or just a dark tint, not the speech bubble
                Image img = dialogPanelTr.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0, 0, 0, 0.8f); // Dark background for the victory screen instead of a speech bubble
                    img.sprite = null; // Remove any leftover speech bubble sprite
                }
                
                var sync = dialogPanelTr.GetComponent<SyncSpriteToImage>();
                if (sync != null) Object.DestroyImmediate(sync);
                var anim = dialogPanelTr.GetComponent<Animator>();
                if (anim != null) Object.DestroyImmediate(anim);

                // Fix Button Texts explicitly (sometimes GetComponentsInChildren misses disabled objects in Awake)
                Transform menuBtnTr = dialogPanelTr.Find("ButtonMenu");
                if (menuBtnTr != null)
                {
                    RectTransform rtMenu = menuBtnTr.GetComponent<RectTransform>();
                    rtMenu.anchoredPosition = new Vector2(-200, -100);
                    rtMenu.sizeDelta = new Vector2(250, 60);

                    TextMeshProUGUI txtMenu = menuBtnTr.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txtMenu != null) txtMenu.text = "Menu";
                }

                Transform exitBtnTr = dialogPanelTr.Find("ButtonExit");
                if (exitBtnTr != null)
                {
                    RectTransform rtExit = exitBtnTr.GetComponent<RectTransform>();
                    rtExit.anchoredPosition = new Vector2(200, -100);
                    rtExit.sizeDelta = new Vector2(250, 60);

                    TextMeshProUGUI txtExit = exitBtnTr.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txtExit != null) txtExit.text = "Exit";
                }

                // Ensure the logo is properly anchored big and center
                Transform logoTr = dialogPanelTr.Find("Logo");
                if (logoTr != null)
                {
                    RectTransform logoRt = logoTr.GetComponent<RectTransform>();
                    logoRt.anchorMin = new Vector2(0.5f, 1f);
                    logoRt.anchorMax = new Vector2(0.5f, 1f);
                    logoRt.pivot = new Vector2(0.5f, 1f);
                    logoRt.anchoredPosition = new Vector2(0, -100);
                    logoRt.sizeDelta = new Vector2(600, 300); // Make it big
                }

                Transform textTr = dialogPanelTr.Find("DialogText");
                if (textTr != null)
                {
                    TextMeshProUGUI victoryText = textTr.GetComponent<TextMeshProUGUI>();
                    if (victoryText != null)
                    {
                        victoryText.text = "Enhorabuena has ganado!!!";
                        victoryText.alignment = TextAlignmentOptions.Center;
                        victoryText.fontSize = 50;
                    }
                    
                    RectTransform textRt = textTr.GetComponent<RectTransform>();
                    textRt.anchorMin = new Vector2(0.5f, 0.5f);
                    textRt.anchorMax = new Vector2(0.5f, 0.5f);
                    textRt.pivot = new Vector2(0.5f, 0.5f);
                    textRt.anchoredPosition = new Vector2(0, 0); // Center
                    textRt.sizeDelta = new Vector2(800, 200);
                }
            }

            // Bind the internal VictoryUI fields again in case they broke
            VictoryUI vicScript = victoryUIObj.GetComponent<VictoryUI>();
            if (vicScript != null && dialogPanelTr != null)
            {
                vicScript.panelVictoria = dialogPanelTr.gameObject;
                vicScript.victoryText = dialogPanelTr.Find("DialogText")?.GetComponent<TextMeshProUGUI>();
                vicScript.victoryImage = dialogPanelTr.Find("Logo")?.GetComponent<Image>();
                
                var btnM = dialogPanelTr.Find("ButtonMenu");
                if (btnM != null)
                {
                    vicScript.botonMenu = btnM.GetComponent<Button>();
                }
                
                var btnE = dialogPanelTr.Find("ButtonExit");
                if (btnE != null) vicScript.botonSalir = btnE.GetComponent<Button>();
                
                EditorUtility.SetDirty(victoryUIObj);
            }
        }
        
        Debug.Log("Visuals fixed!");
    }
}
