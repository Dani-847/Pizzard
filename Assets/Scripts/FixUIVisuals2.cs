using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class FixUIVisuals2 : MonoBehaviour
{
    private void Awake()
    {
        // --- 1. Fix DialogUI --- 
        GameObject dialogUIObj = GameObject.Find("DialogUI");
        if (dialogUIObj != null)
        {
            // Remove the dark background fading the screen
            Image dRootImage = dialogUIObj.GetComponent<Image>();
            if (dRootImage != null)
            {
                var c = dRootImage.color;
                c.a = 0f; // completely clear, no grey tint
                dRootImage.color = c;
            }

            Transform dialogPanelTr = dialogUIObj.transform.Find("DialogPanel");
            if (dialogPanelTr != null)
            {
                // Revert to original SetupSpeechBubble size that the user liked
                RectTransform rt = dialogPanelTr.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0, -200); // Back to bottom center-ish
                    rt.sizeDelta = new Vector2(800, 300);
                }

                Image img = dialogPanelTr.GetComponent<Image>();
                if (img != null) img.color = Color.white;
            }
        }

        // --- 2. Fix VictoryUI ---
        GameObject victoryUIObj = GameObject.Find("VictoryUI");
        if (victoryUIObj != null)
        {
            // Make root grey out the game slightly
            Image vRootImg = victoryUIObj.GetComponent<Image>();
            if (vRootImg != null)
            {
                var c = vRootImg.color;
                c.a = 0.5f;
                vRootImg.color = c;
            }

            Transform dialogPanelTr = victoryUIObj.transform.Find("DialogPanel");
            if (dialogPanelTr != null)
            {
                // Make the panel a large dark box serving as a background for the victory UI
                RectTransform rt = dialogPanelTr.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = new Vector2(1000, 700);
                }

                Image img = dialogPanelTr.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.1f, 0.1f, 0.2f, 0.9f); // Dark blueish tint
                    img.sprite = null;
                }
            }

            // Fix Logo (Make sure it's huge and at the top)
            Transform logoTr = victoryUIObj.transform.Find("Logo");
            if (logoTr != null)
            {
                RectTransform logoRt = logoTr.GetComponent<RectTransform>();
                logoRt.anchorMin = new Vector2(0.5f, 1f);
                logoRt.anchorMax = new Vector2(0.5f, 1f);
                logoRt.pivot = new Vector2(0.5f, 1f);
                logoRt.anchoredPosition = new Vector2(0, -200); 
                logoRt.sizeDelta = new Vector2(800, 400); 
            }

            // Fix Text (Center)
            Transform textTr = victoryUIObj.transform.Find("DialogText");
            if (textTr != null)
            {
                RectTransform textRt = textTr.GetComponent<RectTransform>();
                textRt.anchorMin = new Vector2(0.5f, 0.5f);
                textRt.anchorMax = new Vector2(0.5f, 0.5f);
                textRt.pivot = new Vector2(0.5f, 0.5f);
                textRt.anchoredPosition = new Vector2(0, 50);
                textRt.sizeDelta = new Vector2(800, 200);

                TextMeshProUGUI victoryText = textTr.GetComponent<TextMeshProUGUI>();
                if (victoryText != null)
                {
                    victoryText.text = "Enhorabuena has ganado!!!";
                    victoryText.alignment = TextAlignmentOptions.Center;
                    victoryText.fontSize = 60;
                    victoryText.color = Color.white;
                }
            }

            // Fix Buttons (Bottom row)
            Transform menuBtnTr = victoryUIObj.transform.Find("ButtonMenu");
            if (menuBtnTr != null)
            {
                RectTransform rtMenu = menuBtnTr.GetComponent<RectTransform>();
                rtMenu.anchorMin = new Vector2(0.5f, 0f);
                rtMenu.anchorMax = new Vector2(0.5f, 0f);
                rtMenu.pivot = new Vector2(0.5f, 0f);
                rtMenu.anchoredPosition = new Vector2(-250, 100);
                rtMenu.sizeDelta = new Vector2(300, 80);

                TextMeshProUGUI txtMenu = menuBtnTr.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txtMenu != null) 
                {
                    txtMenu.text = "Menu";
                    txtMenu.fontSize = 36;
                }
            }

            Transform exitBtnTr = victoryUIObj.transform.Find("ButtonExit");
            if (exitBtnTr != null)
            {
                RectTransform rtExit = exitBtnTr.GetComponent<RectTransform>();
                rtExit.anchorMin = new Vector2(0.5f, 0f);
                rtExit.anchorMax = new Vector2(0.5f, 0f);
                rtExit.pivot = new Vector2(0.5f, 0f);
                rtExit.anchoredPosition = new Vector2(250, 100);
                rtExit.sizeDelta = new Vector2(300, 80);

                TextMeshProUGUI txtExit = exitBtnTr.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txtExit != null)
                {
                    txtExit.text = "Exit";
                    txtExit.fontSize = 36;
                }
            }
        }
        
        Debug.Log("Visuals V2 Formatted! Destroying FixUIVisuals2...");
        
        // Destroy this exact script instance so it doesn't linger
        DestroyImmediate(this);
    }
}
