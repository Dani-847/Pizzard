#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pizzard.UI;

public class FixBoss2UI
{
    [MenuItem("Tools/Pizzard/Fix Boss 2 UI")]
    static void Run()
    {
        string mainMenuPath = "Assets/FlowScenes/MainMenu.unity";
        var mainMenuScene = EditorSceneManager.OpenScene(mainMenuPath, OpenSceneMode.Single);

        string spriteBack  = "Assets/Sprites/HealthBar/vidadañada 1.png";
        string spriteFront = "Assets/Sprites/HealthBar/vida 1.png";

        UIManager uiMgr = Object.FindObjectOfType<UIManager>(true);
        if (uiMgr == null) { Debug.LogError("[FixBoss2UI] UIManager not found in MainMenu."); return; }
        Transform canvasRoot = uiMgr.transform;

        Transform pblobUI = canvasRoot.Find("PblobUI");
        RectTransform pblobRT = pblobUI != null ? pblobUI.GetComponent<RectTransform>() : null;

        // ── NiggelBossUI ──────────────────────────────────────
        Transform existingNiggel = canvasRoot.Find("NiggelBossUI");
        if (existingNiggel != null) Object.DestroyImmediate(existingNiggel.gameObject);

        GameObject niggelBossUI = new GameObject("NiggelBossUI");
        niggelBossUI.transform.SetParent(canvasRoot, false);
        RectTransform niggelRT = niggelBossUI.AddComponent<RectTransform>();
        niggelBossUI.AddComponent<CanvasRenderer>();

        if (pblobRT != null)
        {
            niggelRT.anchorMin        = pblobRT.anchorMin;
            niggelRT.anchorMax        = pblobRT.anchorMax;
            niggelRT.anchoredPosition = pblobRT.anchoredPosition;
            niggelRT.sizeDelta        = pblobRT.sizeDelta;
            niggelRT.pivot            = pblobRT.pivot;
        }
        else
        {
            niggelRT.anchorMin        = new Vector2(0.5f, 1f);
            niggelRT.anchorMax        = new Vector2(0.5f, 1f);
            niggelRT.anchoredPosition = new Vector2(0f, -50f);
            niggelRT.sizeDelta        = Vector2.zero;
            niggelRT.pivot            = new Vector2(0.5f, 1f);
        }

        var backBar  = CreateUIImage("BackHealthBar",  niggelBossUI.transform, spriteBack,
                           new Vector2(22.995f, 118.88f),  new Vector2(482.19f, 89.208f), false);
        var frontBar = CreateUIImage("FrontHealthBar", niggelBossUI.transform, spriteFront,
                           new Vector2(22.995f, 118.882f), new Vector2(482.19f, 89.21f),  true);

        GameObject nameTextGO = new GameObject("BossNameText");
        nameTextGO.transform.SetParent(niggelBossUI.transform, false);
        nameTextGO.AddComponent<CanvasRenderer>();
        RectTransform nameRT = nameTextGO.AddComponent<RectTransform>();
        nameRT.anchorMin        = new Vector2(0.5f, 0f);
        nameRT.anchorMax        = new Vector2(0.5f, 0f);
        nameRT.anchoredPosition = new Vector2(0f, 22f);
        nameRT.sizeDelta        = new Vector2(500f, 20f);
        nameRT.pivot            = new Vector2(0.5f, 0.5f);
        var tmp = nameTextGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "Niggel Worthington";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 36f;
        tmp.color     = Color.white;

        var bossHPBar         = niggelBossUI.AddComponent<BossHealthBarUI>();
        bossHPBar.healthFillBar = frontBar.GetComponent<Image>();
        bossHPBar.bossNameText  = tmp;

        // ── CoinMeterUI ───────────────────────────────────────
        Transform existingCoin = canvasRoot.Find("CoinMeterUI");
        if (existingCoin != null) Object.DestroyImmediate(existingCoin.gameObject);

        Transform manaUI  = canvasRoot.Find("ManaUI");
        RectTransform manaRT = manaUI != null ? manaUI.GetComponent<RectTransform>() : null;

        GameObject coinMeterUI = new GameObject("CoinMeterUI");
        coinMeterUI.transform.SetParent(canvasRoot, false);
        coinMeterUI.AddComponent<CanvasRenderer>();
        RectTransform coinRT = coinMeterUI.AddComponent<RectTransform>();
        coinRT.anchorMin        = new Vector2(1f, 0f);
        coinRT.anchorMax        = new Vector2(1f, 0f);
        coinRT.pivot            = new Vector2(1f, 0f);
        float coinY             = manaRT != null ? (manaRT.anchoredPosition.y + manaRT.sizeDelta.y + 10f) : 80f;
        coinRT.anchoredPosition = new Vector2(-10f, coinY);
        coinRT.sizeDelta        = new Vector2(200f, 20f);

        var coinBg   = new GameObject("CoinBackground");
        coinBg.transform.SetParent(coinMeterUI.transform, false);
        coinBg.AddComponent<CanvasRenderer>();
        var coinBgRT = coinBg.AddComponent<RectTransform>();
        coinBgRT.anchorMin = Vector2.zero; coinBgRT.anchorMax = Vector2.one;
        coinBgRT.offsetMin = coinBgRT.offsetMax = Vector2.zero;
        var coinBgImg       = coinBg.AddComponent<Image>();
        coinBgImg.color     = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        var coinFillGO  = new GameObject("CoinFillBar");
        coinFillGO.transform.SetParent(coinMeterUI.transform, false);
        coinFillGO.AddComponent<CanvasRenderer>();
        var coinFillRT  = coinFillGO.AddComponent<RectTransform>();
        coinFillRT.anchorMin = Vector2.zero; coinFillRT.anchorMax = Vector2.one;
        coinFillRT.offsetMin = coinFillRT.offsetMax = Vector2.zero;
        var coinFillImg         = coinFillGO.AddComponent<Image>();
        coinFillImg.color       = new Color(1f, 0.84f, 0f, 1f);
        coinFillImg.type        = Image.Type.Filled;
        coinFillImg.fillMethod  = Image.FillMethod.Horizontal;
        coinFillImg.fillAmount  = 0f;

        var coinTextGO  = new GameObject("CoinCountText");
        coinTextGO.transform.SetParent(coinMeterUI.transform, false);
        coinTextGO.AddComponent<CanvasRenderer>();
        var coinTextRT  = coinTextGO.AddComponent<RectTransform>();
        coinTextRT.anchorMin = Vector2.zero; coinTextRT.anchorMax = Vector2.one;
        coinTextRT.offsetMin = coinTextRT.offsetMax = Vector2.zero;
        var coinTMP         = coinTextGO.AddComponent<TextMeshProUGUI>();
        coinTMP.text        = "0";
        coinTMP.alignment   = TextAlignmentOptions.Center;
        coinTMP.fontSize    = 14f;
        coinTMP.color       = Color.white;

        var coinMeterScript = coinMeterUI.AddComponent<NiggelCoinMeterUI>();
        var so = new SerializedObject(coinMeterScript);
        so.FindProperty("fillBar").objectReferenceValue        = coinFillImg;
        so.FindProperty("coinCountText").objectReferenceValue  = coinTMP;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(mainMenuScene);
        EditorSceneManager.SaveScene(mainMenuScene);
        Debug.Log("[FixBoss2UI] MainMenu fixed and saved. Now reopening BossArena_2.");

        EditorSceneManager.OpenScene("Assets/FlowScenes/BossArena_2.unity", OpenSceneMode.Single);
        Debug.Log("[FixBoss2UI] Done! Open BossArena_2 for final verify.");
    }

    static GameObject CreateUIImage(string name, Transform parent, string spritePath,
        Vector2 anchoredPos, Vector2 size, bool filled)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        var rt             = go.AddComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0.5f, 0.5f);
        rt.anchorMax       = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = size;
        rt.pivot           = new Vector2(0.5f, 0.5f);
        var img    = go.AddComponent<Image>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null) img.sprite = sprite;
        if (filled)
        {
            img.type       = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
        }
        return go;
    }
}
#endif
