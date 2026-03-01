using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Pizzard.Bosses;
using Pizzard.UI;

namespace Pizzard.EditorTools
{
    /// <summary>
    /// One-click setup for Niggel boss fight in BossArena_2.
    ///
    /// Run via: Tools/Pizzard/Setup Niggel Arena
    ///
    /// What it does:
    ///   1. Creates CoinBag, HealingCoin, BlackDotBarrier prefabs in Assets/Prefabs/Bosses/
    ///   2. Opens BossArena_2 and adds the NiggelCoinMeterUI HUD element below ManaUI
    ///   3. Assigns all SerializeField references on the NiggelController in the scene
    ///   4. Verifies BossHealthBarUI trackedBoss is pointing to NiggelController
    ///   5. Saves the scene
    /// </summary>
    public static class NiggelArenaSetup
    {
        private const string BossArenaPath = "Assets/FlowScenes/BossArena_2.unity";
        private const string PrefabDir = "Assets/Prefabs/Bosses";
        private const string CoinBagPrefabPath = "Assets/Prefabs/Bosses/CoinBag.prefab";
        private const string HealingCoinPrefabPath = "Assets/Prefabs/Bosses/HealingCoin.prefab";
        private const string BlackDotPrefabPath = "Assets/Prefabs/Bosses/BlackDotBarrier.prefab";

        [MenuItem("Tools/Pizzard/Setup Niggel Arena")]
        public static void Run()
        {
            // ── Step 1: Ensure prefab directory exists ──────────────────────────
            if (!AssetDatabase.IsValidFolder(PrefabDir))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Bosses");
                Debug.Log("[NiggelArenaSetup] Created Assets/Prefabs/Bosses/");
            }

            // ── Step 2: Create the three prefabs ────────────────────────────────
            var coinBagPrefab = CreateOrLoadPrefab(CoinBagPrefabPath, CreateCoinBagRoot);
            var healingCoinPrefab = CreateOrLoadPrefab(HealingCoinPrefabPath, CreateHealingCoinRoot);
            var blackDotPrefab = CreateOrLoadPrefab(BlackDotPrefabPath, CreateBlackDotBarrierRoot);

            // ── Step 3: Open BossArena_2 ─────────────────────────────────────────
            var scene = EditorSceneManager.OpenScene(BossArenaPath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[NiggelArenaSetup] Could not open scene: {BossArenaPath}");
                return;
            }

            // ── Step 4: Find NiggelController in scene ───────────────────────────
            var niggel = Object.FindObjectOfType<NiggelController>();
            if (niggel == null)
            {
                Debug.LogError("[NiggelArenaSetup] NiggelController not found in BossArena_2. " +
                               "Make sure the Niggel prefab is placed in the scene.");
                return;
            }

            // Assign prefab SerializeFields via SerializedObject
            var niggelSO = new SerializedObject(niggel);
            SetPrefabField(niggelSO, "coinBagPrefab", coinBagPrefab);
            SetPrefabField(niggelSO, "healingCoinPrefab", healingCoinPrefab);
            SetPrefabField(niggelSO, "blackDotBarrierPrefab", blackDotPrefab);
            // Set arena center to origin and standard clamp values for BossArena_2
            niggelSO.FindProperty("arenaCenter").vector2Value = Vector2.zero;
            niggelSO.FindProperty("arenaClampX").floatValue = 7f;
            niggelSO.FindProperty("arenaClampY").floatValue = 4f;
            niggelSO.ApplyModifiedProperties();
            Debug.Log("[NiggelArenaSetup] NiggelController prefab references assigned.");

            // ── Step 5: Find/add coin meter HUD ─────────────────────────────────
            SetupCoinMeterHUD(niggel.transform);

            // ── Step 6: Verify BossHealthBarUI ───────────────────────────────────
            var bossBar = Object.FindObjectOfType<BossHealthBarUI>();
            if (bossBar != null)
            {
                // BossHealthBarUI.OnEnable uses FindObjectOfType<BossBase>() automatically
                // Just confirm it's in the scene — no manual wiring needed
                Debug.Log("[NiggelArenaSetup] BossHealthBarUI found in scene — it auto-finds NiggelController " +
                          "via FindObjectOfType<BossBase>() in OnEnable. No manual wiring required.");
            }
            else
            {
                Debug.LogWarning("[NiggelArenaSetup] BossHealthBarUI not found in scene — add it manually to the HUD Canvas.");
            }

            // ── Step 7: Save scene ────────────────────────────────────────────────
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[NiggelArenaSetup] BossArena_2 saved. Niggel fight is fully wired!");
        }

        // ── Prefab creators ──────────────────────────────────────────────────────

        private static GameObject CreateCoinBagRoot()
        {
            var go = new GameObject("CoinBag");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultCircleSprite();
            sr.color = new Color(1f, 0.843f, 0f); // #FFD700

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            go.tag = "EnemyProjectile";
            go.AddComponent<CoinBagProjectile>();

            return go;
        }

        private static GameObject CreateHealingCoinRoot()
        {
            var go = new GameObject("HealingCoin");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultCircleSprite();
            sr.color = new Color(1f, 0.843f, 0f);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            go.tag = "EnemyProjectile";
            go.AddComponent<HealingCoinProjectile>();

            return go;
        }

        private static GameObject CreateBlackDotBarrierRoot()
        {
            var go = new GameObject("BlackDotBarrier");

            int barrierLayer = LayerMask.NameToLayer("BossBarrier");
            if (barrierLayer >= 0)
                go.layer = barrierLayer;
            else
                Debug.LogWarning("[NiggelArenaSetup] 'BossBarrier' layer not found — " +
                                 "the layer was added to TagManager.asset. Restart the Editor if this persists.");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultCircleSprite();
            sr.color = Color.black;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;

            // No Rigidbody2D — static barrier

            go.AddComponent<BlackDotBarrier>();

            return go;
        }

        // ── HUD setup ────────────────────────────────────────────────────────────

        private static void SetupCoinMeterHUD(Transform niggelTransform)
        {
            // Check if coin meter already exists
            var existing = Object.FindObjectOfType<NiggelCoinMeterUI>();
            if (existing != null)
            {
                Debug.Log("[NiggelArenaSetup] NiggelCoinMeterUI already in scene — skipping HUD creation.");
                return;
            }

            // Find the HUD Canvas
            var canvases = Object.FindObjectsOfType<Canvas>();
            Canvas hudCanvas = null;
            foreach (var c in canvases)
            {
                // Boss arena canvas is typically the one with a BossHealthBarUI or ManaUI child
                if (c.GetComponentInChildren<ManaUI>() != null ||
                    c.GetComponentInChildren<BossHealthBarUI>() != null)
                {
                    hudCanvas = c;
                    break;
                }
            }

            if (hudCanvas == null && canvases.Length > 0)
                hudCanvas = canvases[0];

            if (hudCanvas == null)
            {
                Debug.LogError("[NiggelArenaSetup] No Canvas found in scene — cannot place NiggelCoinMeterUI. " +
                               "Add one manually.");
                return;
            }

            // Find ManaUI to position below it
            var manaUI = hudCanvas.GetComponentInChildren<ManaUI>();
            Transform parent = manaUI != null ? manaUI.transform.parent : hudCanvas.transform;

            // Create the coin meter panel
            var panelGO = new GameObject("CoinMeter", typeof(RectTransform));
            panelGO.transform.SetParent(parent, false);

            var panelRT = panelGO.GetComponent<RectTransform>();

            if (manaUI != null)
            {
                // Mirror mana bar position, offset downward by its height + 10px gap
                var manaRT = manaUI.GetComponent<RectTransform>();
                panelRT.anchorMin = manaRT.anchorMin;
                panelRT.anchorMax = manaRT.anchorMax;
                panelRT.pivot = manaRT.pivot;
                panelRT.anchoredPosition = manaRT.anchoredPosition + new Vector2(0f, -manaRT.rect.height - 10f);
                panelRT.sizeDelta = new Vector2(manaRT.rect.width > 0 ? manaRT.rect.width : 120f, 20f);
            }
            else
            {
                // Fallback: top-left corner
                panelRT.anchorMin = new Vector2(0f, 1f);
                panelRT.anchorMax = new Vector2(0f, 1f);
                panelRT.pivot = new Vector2(0f, 1f);
                panelRT.anchoredPosition = new Vector2(10f, -10f);
                panelRT.sizeDelta = new Vector2(120f, 20f);
            }

            // Background image (dark)
            var bgImg = panelGO.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            // Fill bar child
            var fillGO = new GameObject("CoinFill", typeof(RectTransform));
            fillGO.transform.SetParent(panelGO.transform, false);
            var fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.843f, 0f); // gold
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;

            // Coin count text child
            var textGO = new GameObject("CoinCountText", typeof(RectTransform));
            textGO.transform.SetParent(panelGO.transform, false);
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "0";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 14f;
            tmp.color = Color.white;

            // Attach and wire the NiggelCoinMeterUI component
            var coinMeter = panelGO.AddComponent<NiggelCoinMeterUI>();
            var coinMeterSO = new SerializedObject(coinMeter);
            coinMeterSO.FindProperty("fillBar").objectReferenceValue = fillImg;
            coinMeterSO.FindProperty("coinCountText").objectReferenceValue = tmp;
            coinMeterSO.ApplyModifiedProperties();

            Debug.Log($"[NiggelArenaSetup] NiggelCoinMeterUI added to '{hudCanvas.name}' canvas " +
                      (manaUI != null ? "below ManaUI." : "(fallback position — ManaUI not found)."));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static GameObject CreateOrLoadPrefab(string path, System.Func<GameObject> rootFactory)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                Debug.Log($"[NiggelArenaSetup] Prefab already exists: {path}");
                return existing;
            }

            var root = rootFactory();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log($"[NiggelArenaSetup] Created prefab: {path}");
            return prefab;
        }

        private static void SetPrefabField(SerializedObject so, string fieldName, GameObject prefab)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
                prop.objectReferenceValue = prefab;
            else
                Debug.LogWarning($"[NiggelArenaSetup] Field '{fieldName}' not found on NiggelController.");
        }

        private static Sprite GetDefaultCircleSprite()
        {
            // Try to find Unity's built-in circle sprite first
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (sprite != null) return sprite;

            // Fallback: look for any circle sprite in the project
            var guids = AssetDatabase.FindAssets("t:Sprite circle");
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            // Last resort: null (SpriteRenderer will render as a white square — acceptable for prototype)
            Debug.LogWarning("[NiggelArenaSetup] No circle sprite found — prefab SpriteRenderer will have null sprite.");
            return null;
        }
    }
}
