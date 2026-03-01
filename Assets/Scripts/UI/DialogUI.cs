using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pizzard.Core;

/// <summary>
/// Overlay-based dialogue UI. Lives on the persistent UIManager canvas.
/// Features: typewriter text reveal, click-anywhere to advance, 
/// character portrait placeholders, localization support.
/// Phase 14 — replaces scene-based dialogue system.
/// </summary>
public class DialogUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root panel containing all dialogue elements")]
    public GameObject dialogPanel;
    [Tooltip("Main dialogue text (centered)")]
    public TMP_Text dialogText;
    [Tooltip("Speaker name label above dialogue text")]
    public TMP_Text speakerNameText;
    [Tooltip("Left portrait image (player — Bob)")]
    public Image leftPortrait;
    [Tooltip("Right portrait image (NPC — Raberto)")]
    public Image rightPortrait;

    [Header("Typewriter Settings")]
    [Tooltip("Seconds between each character reveal")]
    public float typewriterSpeed = 0.03f;

    [Header("Portrait Colors (Placeholders)")]
    public Color bobColor = new Color(0f, 0.85f, 0.85f, 1f);      // Cyan/teal
    public Color rabertoColor = new Color(1f, 0.55f, 0f, 1f);      // Orange

    [Header("Dialogue Config (Fallback)")]
    [TextArea(2, 4)]
    public string[] introDialogLines = {
        "Welcome, young mage, to the Pizzard's Challenge!",
        "I am Raberto, the guardian of this trial.",
        "You must prove your worth by defeating four elemental guardians.",
        "Each victory will grant you a token.",
        "Spend them wisely in my shop to empower your wand.",
        "Your trial begins now. Good luck."
    };
    [TextArea(2, 4)]
    public string[] preBossDialogLines = {
        "Ready for your next opponent?",
        "Let's see how you handle this!"
    };
    [TextArea(2, 4)]
    public string[] postBossDialogLines = {
        "Impressive! One step closer to becoming a true Pizzard.",
        "Take this token as a reward for your victory.",
        "Return to my shop and prepare for the next challenge."
    };
    [TextArea(2, 4)]
    public string[] deathShopDialogLines = {
        "You have fallen... but do not despair.",
        "Use what you have learned and try again."
    };

    [Header("Localization Key Prefixes")]
    public string introDialogKeyPrefix = "dialog_intro_";
    public string preBossDialogKeyPrefix = "dialog_preboss";
    public string postBossDialogKeyPrefix = "dialog_postboss";
    public string deathShopDialogKeyPrefix = "dialog_deathshop_";

    // Internal state
    private GameFlowManager flowManager;
    private string[] currentLines;
    private int currentLineIndex;
    private bool advancePhaseOnEnd = true;
    private bool isTyping = false;
    private bool dialogActive = false;
    private Coroutine typewriterCoroutine;

    void Update()
    {
        if (!dialogActive) return;

        // Click-anywhere to advance (mouse click, touch, or Enter/Space key)
        bool clicked = false;
        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
            clicked = true;

        if (clicked)
        {
            OnAdvanceClicked();
        }
    }

    // ─────────────────────────────────────────────
    //  PUBLIC API — called by GameFlowManager
    // ─────────────────────────────────────────────

    public void Show()
    {
        gameObject.SetActive(true);
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
    }

    public void Hide()
    {
        dialogActive = false;
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        isTyping = false;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);
            
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the intro dialogue (after pressing Play, before first shop).
    /// </summary>
    public void ShowIntroDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = true;
        SetPortraits("Raberto", isPlayerSpeaking: false);
        StartDialogWithLocalization(introDialogKeyPrefix, introDialogLines);
    }

    /// <summary>
    /// Shows the pre-boss dialogue (after shop, before combat).
    /// Uses boss-specific localization keys: dialog_prebossN_1, dialog_prebossN_2...
    /// </summary>
    public void ShowPreBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = true;
        SetPortraits("Raberto", isPlayerSpeaking: false);
        string dynamicKey = preBossDialogKeyPrefix + manager.currentBossIndex + "_";
        StartDialogWithLocalization(dynamicKey, preBossDialogLines);
    }

    /// <summary>
    /// Shows the post-boss dialogue (after boss defeated, before next shop or credits).
    /// </summary>
    public void ShowPostBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = true;
        SetPortraits("Raberto", isPlayerSpeaking: false);
        string dynamicKey = postBossDialogKeyPrefix + manager.currentBossIndex + "_";
        StartDialogWithLocalization(dynamicKey, postBossDialogLines);
    }

    /// <summary>
    /// Shows the death/shop return dialogue. Does NOT advance phase on end.
    /// </summary>
    public void ShowDeathShopDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = false;
        SetPortraits("Raberto", isPlayerSpeaking: false);
        StartDialogWithLocalization(deathShopDialogKeyPrefix, deathShopDialogLines);
    }

    /// <summary>
    /// Shows the shop warning overlay (when trying to leave shop too early).
    /// Auto-hides after a few seconds. Does not trigger dialogue engine.
    /// </summary>
    public void ShowShopWarningDialog()
    {
        if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(LocalizationManager.Instance.GetText("shop_warning_exit")))
        {
            string loc = LocalizationManager.Instance.GetText("shop_warning_exit");
            if (!loc.StartsWith("["))
            {
                if (dialogText != null) dialogText.text = loc;
                if (speakerNameText != null) speakerNameText.text = "Raberto";
                Show();
                StartCoroutine(HideAfterWarning());
            }
        }
    }

    private System.Collections.IEnumerator HideAfterWarning()
    {
        yield return new WaitForSeconds(3.5f);
        Hide();
    }

    // ─────────────────────────────────────────────
    //  PORTRAITS
    // ─────────────────────────────────────────────

    private void SetPortraits(string speakerName, bool isPlayerSpeaking)
    {
        if (speakerNameText != null)
            speakerNameText.text = speakerName;

        // Show both portrait slots with placeholder colors
        if (leftPortrait != null)
        {
            leftPortrait.color = bobColor;
            leftPortrait.gameObject.SetActive(true);
        }
        if (rightPortrait != null)
        {
            rightPortrait.color = rabertoColor;
            rightPortrait.gameObject.SetActive(true);
        }
    }

    // ─────────────────────────────────────────────
    //  LOCALIZATION
    // ─────────────────────────────────────────────

    private void StartDialogWithLocalization(string keyPrefix, string[] fallbackLines)
    {
        string[] lines = GetLocalizedLines(keyPrefix, fallbackLines);
        StartDialog(lines);
    }

    private string[] GetLocalizedLines(string keyPrefix, string[] fallbackLines)
    {
        if (LocalizationManager.Instance == null)
            return fallbackLines;

        List<string> localizedLines = new List<string>();
        for (int i = 0; i < fallbackLines.Length; i++)
        {
            string key = keyPrefix + (i + 1);
            string localized = LocalizationManager.Instance.GetText(key);

            // If key doesn't exist, GetText returns [key]
            if (localized.StartsWith("[") && localized.EndsWith("]"))
                localizedLines.Add(fallbackLines[i]);
            else
                localizedLines.Add(localized);
        }

        return localizedLines.ToArray();
    }

    // ─────────────────────────────────────────────
    //  DIALOGUE ENGINE
    // ─────────────────────────────────────────────

    private void StartDialog(string[] lines)
    {
        currentLines = lines;
        currentLineIndex = 0;
        dialogActive = true;
        Show();
        ShowCurrentLineWithTypewriter();
    }

    private void ShowCurrentLineWithTypewriter()
    {
        if (currentLines == null || currentLineIndex >= currentLines.Length)
            return;

        string fullText = currentLines[currentLineIndex];

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterReveal(fullText));
    }

    private IEnumerator TypewriterReveal(string fullText)
    {
        isTyping = true;
        if (dialogText != null)
            dialogText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            if (!isTyping) break; // interrupted by click

            if (dialogText != null)
                dialogText.text = fullText.Substring(0, i + 1);

            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        // Ensure full text is shown
        if (dialogText != null)
            dialogText.text = fullText;

        isTyping = false;
        typewriterCoroutine = null;
    }

    /// <summary>
    /// Called when player clicks anywhere on screen during dialogue.
    /// If typing: instantly reveal full text.
    /// If fully revealed: advance to next line or end dialogue.
    /// </summary>
    private void OnAdvanceClicked()
    {
        if (isTyping)
        {
            // Instantly show the full current line
            isTyping = false;
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            if (currentLines != null && currentLineIndex < currentLines.Length && dialogText != null)
            {
                dialogText.text = currentLines[currentLineIndex];
            }
            return;
        }

        // Advance to next line
        currentLineIndex++;

        if (currentLines != null && currentLineIndex < currentLines.Length)
        {
            ShowCurrentLineWithTypewriter();
        }
        else
        {
            // Dialogue finished
            Hide();
            if (advancePhaseOnEnd)
            {
                flowManager?.AvanzarFase();
            }
        }
    }
}
