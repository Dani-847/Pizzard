using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-contained pause during boss fight.
/// Creates its own simple pause panel with Resume + Quit buttons.
/// Does NOT touch MenuUI at all to avoid corrupting the main menu.
/// </summary>
public class PauseBossArena : MonoBehaviour
{
    private bool isPaused = false;
    private GameObject pausePanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            if (pausePanel == null) BuildPausePanel();
            pausePanel.SetActive(true);
        }
        else
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }

    private void BuildPausePanel()
    {
        // Find the scene's main Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Overlay panel
        pausePanel = new GameObject("BossPausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);
        var panelRT = pausePanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Semi-transparent background
        var bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        // Title
        var titleGO = new GameObject("PauseTitle");
        titleGO.transform.SetParent(pausePanel.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0, 60);
        titleRT.sizeDelta = new Vector2(300, 50);
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "PAUSA";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 36;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // Resume button
        CreateButton(pausePanel.transform, "Reanudar", new Vector2(0, -10), () =>
        {
            TogglePause();
        });

        // Quit to menu button
        CreateButton(pausePanel.transform, "Salir al Menú", new Vector2(0, -60), () =>
        {
            Time.timeScale = 1f;
            isPaused = false;
            if (Pizzard.Core.GameFlowManager.Instance != null)
                Pizzard.Core.GameFlowManager.Instance.VolverAlMenu();
        });
    }

    private void CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var btnGO = new GameObject("Btn_" + label);
        btnGO.transform.SetParent(parent, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 40);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        // Label
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) Destroy(pausePanel);
    }
}
