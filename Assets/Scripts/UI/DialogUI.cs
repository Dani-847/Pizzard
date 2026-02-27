using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pizzard.Core;

/// <summary>
/// Gestiona la interfaz de diálogos durante el flujo del juego.
/// Muestra textos narrativos en distintos momentos (intro, pre-boss, post-boss, etc.)
/// y notifica al GameFlowManager cuando el jugador avanza.
/// Soporta localización a través de LocalizationManager.
/// </summary>
public class DialogUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Panel contenedor del diálogo")]
    public GameObject dialogPanel;
    [Tooltip("Texto principal del diálogo")]
    public TMP_Text dialogText;
    [Tooltip("Botón para continuar/avanzar el diálogo")]
    public Button continueButton;
    
    [Header("Configuración de Diálogos (Fallback)")]
    [Tooltip("Líneas del diálogo introductorio (usado si no hay localización)")]
    [TextArea(2, 4)]
    public string[] introDialogLines = { "Welcome to the world of Pizzard!", "Prepare to face your first enemy..." };
    [Tooltip("Líneas del diálogo antes del boss (usado si no hay localización)")]
    [TextArea(2, 4)]
    public string[] preBossDialogLines = { "The boss awaits...", "Good luck!" };
    [Tooltip("Líneas del diálogo después de derrotar al boss (usado si no hay localización)")]
    [TextArea(2, 4)]
    public string[] postBossDialogLines = { "Congratulations! You defeated the boss.", "Continue your adventure..." };
    [Tooltip("Líneas del diálogo antes del siguiente boss (usado si no hay localización)")]
    [TextArea(2, 4)]
    public string[] preNextBossDialogLines = { "A new challenge awaits...", "Forward!" };

    [Header("Claves de Localización")]
    [Tooltip("Prefijo para las claves de diálogo intro (ej: dialog_intro_1, dialog_intro_2)")]
    public string introDialogKeyPrefix = "dialog_intro_";
    [Tooltip("Prefijo para las claves de diálogo pre-boss")]
    public string preBossDialogKeyPrefix = "dialog_preboss"; // Append boss ID like dialog_preboss1_
    [Tooltip("Prefijo para las claves de diálogo post-boss")]
    public string postBossDialogKeyPrefix = "dialog_postboss_";

    private GameFlowManager flowManager;
    private string[] currentLines;
    private int currentLineIndex;
    private bool advancePhaseOnEnd = true;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    /// <summary>
    /// Muestra el panel de diálogos.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel de diálogos.
    /// </summary>
    public void Hide()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Inicia el diálogo introductorio.
    /// Intenta usar localización, si no hay usa los textos por defecto.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowIntroDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = true;
        StartDialogWithLocalization(introDialogKeyPrefix, introDialogLines);
    }

    /// <summary>
    /// Inicia el diálogo previo al combate con el boss.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowPreBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = true;
        // Dynamically get boss dialog string: dialog_preboss1_, dialog_preboss2_, etc
        string dynamicKey = preBossDialogKeyPrefix + manager.currentBossIndex + "_";
        StartDialogWithLocalization(dynamicKey, preBossDialogLines);
    }

    /// <summary>
    /// Inicia el diálogo posterior a derrotar al boss.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowPostBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = true;
        StartDialogWithLocalization(postBossDialogKeyPrefix, postBossDialogLines);
    }

    [Header("Diálogo de Muerte (Tienda)")]
    public string[] deathShopDialogLines = { "Ah, you perished. I scraped you off the floor.", "Take a moment to prepare before you try again." };
    public string deathShopDialogKeyPrefix = "dialog_deathshop_";

    public void ShowDeathShopDialog(GameFlowManager manager)
    {
        flowManager = manager;
        advancePhaseOnEnd = false;
        StartDialogWithLocalization(deathShopDialogKeyPrefix, deathShopDialogLines);
    }

    /// <summary>
    /// Muestra únicamente el texto de advertencia (al intentar salir de tienda pronto).
    /// </summary>
    public void ShowShopWarningDialog()
    {
        if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(LocalizationManager.Instance.GetText("shop_warning_exit")))
        {
            string loc = LocalizationManager.Instance.GetText("shop_warning_exit");
            // Only show it if it exists and isn't a bracketed fallback
            if(!loc.StartsWith("["))
            {
                dialogText.text = loc;
                Show();
                StartCoroutine(HideAfterWarning());
            }
        }
    }

    private IEnumerator HideAfterWarning()
    {
        yield return new WaitForSeconds(3.5f);
        Hide();
    }

    /// <summary>
    /// Inicia un diálogo intentando usar localización primero.
    /// </summary>
    /// <param name="keyPrefix">Prefijo de la clave de localización.</param>
    /// <param name="fallbackLines">Líneas por defecto si no hay localización.</param>
    private void StartDialogWithLocalization(string keyPrefix, string[] fallbackLines)
    {
        string[] lines = GetLocalizedLines(keyPrefix, fallbackLines);
        StartDialog(lines);
    }

    /// <summary>
    /// Obtiene las líneas de diálogo localizadas o las de fallback.
    /// </summary>
    private string[] GetLocalizedLines(string keyPrefix, string[] fallbackLines)
    {
        if (LocalizationManager.Instance == null)
        {
            return fallbackLines;
        }

        List<string> localizedLines = new List<string>();
        for (int i = 0; i < fallbackLines.Length; i++)
        {
            string key = keyPrefix + (i + 1);
            string localized = LocalizationManager.Instance.GetText(key);
            
            // Si la clave no existe, GetText devuelve [key]
            if (localized.StartsWith("[") && localized.EndsWith("]"))
            {
                // Usar el fallback para esta línea
                localizedLines.Add(fallbackLines[i]);
            }
            else
            {
                localizedLines.Add(localized);
            }
        }
        
        return localizedLines.ToArray();
    }

    /// <summary>
    /// Inicia un diálogo con las líneas especificadas.
    /// </summary>
    private void StartDialog(string[] lines)
    {
        currentLines = lines;
        currentLineIndex = 0;
        Show();
        ShowCurrentLine();
    }

    /// <summary>
    /// Muestra la línea actual del diálogo.
    /// </summary>
    private void ShowCurrentLine()
    {
        if (currentLines != null && currentLineIndex < currentLines.Length)
        {
            if (dialogText != null)
                dialogText.text = currentLines[currentLineIndex];
        }
    }

    /// <summary>
    /// Callback del botón "Continuar".
    /// Avanza al siguiente texto o cierra el diálogo y notifica al GameFlowManager.
    /// </summary>
    private void OnContinueClicked()
    {
        currentLineIndex++;
        
        if (currentLines != null && currentLineIndex < currentLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            // Diálogo terminado
            Hide();
            if (advancePhaseOnEnd)
            {
                flowManager?.AvanzarFase();
            }
        }
    }
}
