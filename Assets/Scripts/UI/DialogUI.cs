using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la interfaz de diálogos durante el flujo del juego.
/// Muestra textos narrativos en distintos momentos (intro, pre-boss, post-boss, etc.)
/// y notifica al GameFlowManager cuando el jugador avanza.
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
    
    [Header("Configuración de Diálogos")]
    [Tooltip("Líneas del diálogo introductorio")]
    [TextArea(2, 4)]
    public string[] introDialogLines = { "¡Bienvenido al mundo de Pizzard!", "Prepárate para enfrentar a tu primer enemigo..." };
    [Tooltip("Líneas del diálogo antes del boss")]
    [TextArea(2, 4)]
    public string[] preBossDialogLines = { "El boss te espera...", "¡Buena suerte!" };
    [Tooltip("Líneas del diálogo después de derrotar al boss")]
    [TextArea(2, 4)]
    public string[] postBossDialogLines = { "¡Felicidades! Has derrotado al boss.", "Continúa tu aventura..." };
    [Tooltip("Líneas del diálogo antes del siguiente boss")]
    [TextArea(2, 4)]
    public string[] preNextBossDialogLines = { "Un nuevo desafío te espera...", "¡Adelante!" };

    private GameFlowManager flowManager;
    private string[] currentLines;
    private int currentLineIndex;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        Hide();
    }

    /// <summary>
    /// Muestra el panel de diálogos.
    /// </summary>
    public void Show()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
        else
            gameObject.SetActive(true);
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
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowIntroDialog(GameFlowManager manager)
    {
        flowManager = manager;
        StartDialog(introDialogLines);
    }

    /// <summary>
    /// Inicia el diálogo previo al combate con el boss.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowPreBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        StartDialog(preBossDialogLines);
    }

    /// <summary>
    /// Inicia el diálogo posterior a derrotar al boss.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowPostBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        StartDialog(postBossDialogLines);
    }

    /// <summary>
    /// Inicia el diálogo antes de pasar al siguiente boss.
    /// </summary>
    /// <param name="manager">GameFlowManager que recibirá la notificación al terminar.</param>
    public void ShowPreNextBossDialog(GameFlowManager manager)
    {
        flowManager = manager;
        StartDialog(preNextBossDialogLines);
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
            flowManager?.AvanzarFase();
        }
    }
}
