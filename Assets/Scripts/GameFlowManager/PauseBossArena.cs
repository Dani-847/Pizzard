using UnityEngine;

/// <summary>
/// Handles Esc-key pause toggling during the boss fight.
/// Attach to any persistent GameObject in the boss arena scene.
/// Uses Time.timeScale for pause and shows/hides the MenuUI panel.
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

        if (menuUI != null)
            menuUI.gameObject.SetActive(isPaused);
    }

    private void OnDestroy()
    {
        // Safety: always restore time on scene unload
        Time.timeScale = 1f;
    }
}
