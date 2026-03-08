// Assets/Scripts/Editor/PlaygroundSceneSetup.cs
//
// Creates Assets/FlowScenes/PlaygroundScene.unity with:
//   - PlaygroundManager GameObject
//   - TrainingDummy with DummyDPSTracker (tagged "Boss") + world-space DPS canvas
//   - PlayerSpawnPoint marker
//   - HUD Canvas (Screen Space Overlay) with PlaygroundHUDController
//     (shopUI reference is wired to the ShopUI child panel)
//
// Run via: Tools/Pizzard/Setup Playground Scene

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pizzard.EditorTools
{
    public static class PlaygroundSceneSetup
    {
        private const string ScenePath = "Assets/FlowScenes/PlaygroundScene.unity";

        [MenuItem("Tools/Pizzard/Setup Playground Scene")]
        public static void Run()
        {
            // ── Create a new empty scene ──────────────────────────────────────
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ── PlaygroundManager ─────────────────────────────────────────────
            var managerGO = new GameObject("PlaygroundManager");
            managerGO.AddComponent<PlaygroundManager>();

            // ── TrainingDummy ─────────────────────────────────────────────────
            var dummyGO = new GameObject("TrainingDummy");
            dummyGO.transform.position = new Vector3(4f, 0f, 0f);

            // Tag must be "Boss" so CharacterProjectile collision routing reaches DummyDPSTracker
            if (!string.IsNullOrEmpty(dummyGO.tag))
            {
                try { dummyGO.tag = "Boss"; }
                catch { Debug.LogWarning("[PlaygroundSceneSetup] 'Boss' tag not found — add it in Tags & Layers."); }
            }

            var spriteRenderer = dummyGO.AddComponent<SpriteRenderer>();
            // Placeholder: use a white square; replace with art in Unity Inspector
            spriteRenderer.color = Color.gray;

            var col = dummyGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 2f);

            var tracker = dummyGO.AddComponent<DummyDPSTracker>();

            // ── World-Space DPS Canvas ────────────────────────────────────────
            var dpsCanvasGO = new GameObject("DPSCanvas");
            dpsCanvasGO.transform.SetParent(dummyGO.transform, false);
            dpsCanvasGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var dpsCanvas = dpsCanvasGO.AddComponent<Canvas>();
            dpsCanvas.renderMode = RenderMode.WorldSpace;

            var dpsCanvasRT = dpsCanvasGO.GetComponent<RectTransform>();
            dpsCanvasRT.sizeDelta = new Vector2(2f, 0.5f);
            dpsCanvasRT.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            var dpsTextGO = new GameObject("DPSText");
            dpsTextGO.transform.SetParent(dpsCanvasGO.transform, false);

            var dpsText = dpsTextGO.AddComponent<TextMeshPro>();
            dpsText.text = "0.0";
            dpsText.fontSize = 36;
            dpsText.alignment = TextAlignmentOptions.Center;
            dpsText.color = Color.yellow;

            var dpsTextRT = dpsTextGO.GetComponent<RectTransform>();
            dpsTextRT.sizeDelta = new Vector2(200f, 50f);
            dpsTextRT.localPosition = Vector3.zero;

            // Wire dpsText reference into tracker via SerializedObject
            var trackerSO = new SerializedObject(tracker);
            trackerSO.FindProperty("dpsText").objectReferenceValue = dpsText;
            trackerSO.ApplyModifiedPropertiesWithoutUndo();

            // ── PlayerSpawnPoint ──────────────────────────────────────────────
            var spawnGO = new GameObject("PlayerSpawnPoint");
            spawnGO.transform.position = new Vector3(-2f, 0f, 0f);

            // ── HUD Canvas (Screen Space Overlay) ─────────────────────────────
            var hudGO = new GameObject("HUD Canvas");
            var hudCanvas = hudGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 10;
            hudGO.AddComponent<CanvasScaler>();
            hudGO.AddComponent<GraphicRaycaster>();

            // Shop Button (bottom-left)
            var shopBtnGO = CreateButton(hudGO.transform, "ShopButton", "Shop", new Vector2(100f, 50f), new Vector2(-Screen.width / 2f + 100f, -Screen.height / 2f + 40f));

            // Back to Menu Button (top-right)
            var backBtnGO = CreateButton(hudGO.transform, "BackToMenuButton", "Back to Menu", new Vector2(160f, 50f), new Vector2(Screen.width / 2f - 100f, Screen.height / 2f - 40f));

            // Token Counter Text (near shop button)
            var tokenTextGO = new GameObject("TokenCounterText");
            tokenTextGO.transform.SetParent(hudGO.transform, false);
            var tokenText = tokenTextGO.AddComponent<TextMeshProUGUI>();
            tokenText.text = "10 / 10";
            tokenText.fontSize = 24;
            tokenText.color = Color.white;
            var tokenRT = tokenTextGO.GetComponent<RectTransform>();
            tokenRT.sizeDelta = new Vector2(140f, 40f);
            tokenRT.anchoredPosition = new Vector2(-Screen.width / 2f + 100f, -Screen.height / 2f + 90f);

            // ShopUI panel (hidden by default — stub; wire the real ShopUI prefab in Inspector)
            var shopPanelGO = new GameObject("ShopUI");
            shopPanelGO.transform.SetParent(hudGO.transform, false);
            var shopUI = shopPanelGO.AddComponent<ShopUI>();
            shopPanelGO.AddComponent<RectTransform>();
            shopPanelGO.SetActive(false);

            // PlaygroundHUDController on the HUD Canvas GameObject
            var hudController = hudGO.AddComponent<PlaygroundHUDController>();
            var hudSO = new SerializedObject(hudController);
            hudSO.FindProperty("shopUI").objectReferenceValue = shopUI;
            hudSO.FindProperty("tokenCounterText").objectReferenceValue = tokenText;
            hudSO.FindProperty("shopButton").objectReferenceValue = shopBtnGO.GetComponent<Button>();
            hudSO.FindProperty("backToMenuButton").objectReferenceValue = backBtnGO.GetComponent<Button>();
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Save scene ────────────────────────────────────────────────────
            System.IO.Directory.CreateDirectory("Assets/FlowScenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            // ── Add to Build Settings ─────────────────────────────────────────
            AddSceneToBuildSettings(ScenePath);

            Debug.Log("[PlaygroundSceneSetup] PlaygroundScene created and added to Build Settings.");
            Debug.Log("[PlaygroundSceneSetup] NEXT STEPS in Unity Inspector:");
            Debug.Log("  1. Select TrainingDummy — assign a visible sprite.");
            Debug.Log("  2. Wire the real ShopUI prefab to the HUD Canvas > ShopUI slot.");
            Debug.Log("  3. Place the Player prefab at PlayerSpawnPoint position.");
        }

        private static GameObject CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var btn = go.AddComponent<Button>();

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return go;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return; // already present
            }

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log($"[PlaygroundSceneSetup] Added '{scenePath}' to Build Settings.");
        }
    }
}
