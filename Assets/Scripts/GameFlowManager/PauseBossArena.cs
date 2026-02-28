using UnityEngine;

/// <summary>
/// Handles Esc-key pause toggling during the boss fight.
/// Calls MenuUI.ShowPauseMenu() which shows Resume + BackToMenu buttons
/// (NOT the main menu with Play/Settings/Quit).
/// </summary>
public class PauseBossArena : MonoBehaviour
{
    private bool isPaused = false;
    private MenuUI menuUI;

    private void Start()
    {
        menuUI = FindObjectOfType<MenuUI>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (menuUI == null)
            menuUI = FindObjectOfType<MenuUI>(true);

        if (menuUI != null)
        {
            if (isPaused)
            {
                // Show pause menu (Resume + BackToMenu), NOT main menu
                menuUI.ShowPauseMenu(Pizzard.Core.GameFlowManager.Instance);
            }
            else
            {
                menuUI.Hide();
            }
        }
    }

    private void OnDestroy()
    {
        // Safety: always restore time on scene unload
        Time.timeScale = 1f;
    }
}
