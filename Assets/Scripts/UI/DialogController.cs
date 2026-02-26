using System;
using UnityEngine;
using TMPro; // Assuming TextMesh Pro based on architecture scan.

namespace Pizzard.UI
{
    /// <summary>
    /// Controls the display of dialog arrays, iterating through them on user input,
    /// and firing an Action callback when the dialog ends.
    /// </summary>
    public class DialogController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private GameObject dialogPanel;

        private string[] currentDialogues;
        private int currentIndex = 0;
        private Action onDialogFinished;

        private bool isDialogActive = false;

        private void Update()
        {
            if (!isDialogActive) return;

            // Advance dialogue on any key/mouse click
            if (Input.anyKeyDown)
            {
                NextLine();
            }
        }

        /// <summary>
        /// Starts parsing a new set of dialogue lines.
        /// </summary>
        public void StartDialog(string[] lines, Action onCompleteCallback)
        {
            if (lines == null || lines.Length == 0)
            {
                onCompleteCallback?.Invoke();
                return;
            }

            currentDialogues = lines;
            currentIndex = 0;
            onDialogFinished = onCompleteCallback;
            isDialogActive = true;

            if (dialogPanel != null) dialogPanel.SetActive(true);
            
            ShowCurrentLine();
        }

        private void NextLine()
        {
            currentIndex++;

            if (currentIndex < currentDialogues.Length)
            {
                ShowCurrentLine();
            }
            else
            {
                EndDialog();
            }
        }

        private void ShowCurrentLine()
        {
            if (dialogText != null)
            {
                dialogText.text = currentDialogues[currentIndex];
            }
        }

        private void EndDialog()
        {
            isDialogActive = false;
            if (dialogPanel != null) dialogPanel.SetActive(false);

            onDialogFinished?.Invoke();
        }
    }
}
