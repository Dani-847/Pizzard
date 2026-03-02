using UnityEngine;
using UnityEditor;

public class AssignFinalSprites : EditorWindow
{
    [MenuItem("Tools/Assign Final v1 Sprites")]
    public static void AssignAll()
    {
        string root = "Assets/Sprites/Spritesforfinalv1/";

        // Helper to load sprite from .ase (gets the Sprite sub-asset)
        Sprite Spr(string name) {
            var path = root + name;
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var o in objs) if (o is Sprite s) return s;
            // Fallback: try direct load
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ── Characters ──────────────────────────────────────────
        SetPrefabSprite("Assets/Prefabs/Base/Player.prefab", Spr("player.ase"));
        SetPrefabSprite("Assets/Prefabs/Base/Pblob.prefab", Spr("pblob.ase"));
        SetPrefabSprite("Assets/Prefabs/Base/Niggel.prefab", Spr("niggel.ase"));

        // ── T1 Projectiles ───────────────────────────────────────
        SetPrefabSprite("Assets/Prefabs/pepperoniProyectile.prefab", Spr("basepeperoniproyectile.ase"));
        SetPrefabSprite("Assets/Prefabs/CheeseShield.prefab", Spr("quesoproyectile.ase"));
        SetPrefabSprite("Assets/Prefabs/StaticCheeseShield.prefab", Spr("quesoproyectile.ase"));

        // ── Boss Projectiles ─────────────────────────────────────
        SetPrefabSprite("Assets/Prefabs/Bosses/CoinBag.prefab", Spr("orangecoinproyectile.ase"));

        // ── T2 Projectiles ───────────────────────────────────────
        SetPrefabSprite("Assets/Prefabs/pepperoniPiñaAttack.prefab", Spr("piñapeperoniproyectile.ase"));
        SetPrefabSprite("Assets/Prefabs/pineappleChikitProjectile.prefab", Spr("piñapeperonifragmentationproyectile.ase"));
        SetPrefabSprite("Assets/Prefabs/piñaquesoPrefab.prefab", Spr("quesopiñaproyectile.ase"));

        // ── T2 Area / Wall objects ───────────────────────────────
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPepperoniArea.prefab", Spr("quesopeperoni.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPepperoniPepperoniArea.prefab", Spr("quesopeperoni.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPepperoniPinaArea.prefab", Spr("quesopeperoni.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPepperoniQuesoArea.prefab", Spr("quesopeperoni.ase"));

        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPinaPillar.prefab", Spr("quesopiñatransformedinwall.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPinaPinaPillar.prefab", Spr("quesopiñatransformedinwall.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPinaQuesoPillar.prefab", Spr("quesopiñatransformedinwall.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoPinaPepperoniPillar.prefab", Spr("quesopiñatransformedinwall.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/QuesoQuesoWall.prefab", Spr("quesopiñatransformedinwall.ase"));

        // ── Small sub-projectiles ────────────────────────────────
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/SmallExplodingProjectile.prefab", Spr("piñapeperonifragmentationproyectile.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/SmallBurningProjectile.prefab", Spr("piñapeperonifragmentationproyectile.ase"));
        SetPrefabSprite("Assets/Prefabs/ElementsAttack/SmallAbsorbingProjectile.prefab", Spr("piñapeperonifragmentationproyectile.ase"));

        // ── Add SpriteRenderer to piñapepperoniAttack (currently has none) ───
        AddSpriteRendererToPrefab("Assets/Prefabs/piñapepperoniAttack.prefab", Spr("piñapeperoni.ase"));

        // ── Element Icons (UI.prefab — ElementsUI script) ────────
        AssignElementIcons(
            Spr("PeperoniIconForElementUI.ase"),
            Spr("PiñaIconForElementUI.ase"),
            Spr("QuesoIconForElementUI.ase")
        );

        // ── Button sprites (UI.prefab) ───────────────────────────
        AssignButtonSprites(
            Spr("button.ase"),
            Spr("pressedbutton.ase")
        );

        // ── Shopkeeper scene override ──────────────────────────
        UpdateShopkeeperSceneOverride(Spr("shopkeeperrabblet.ase"));

        // ── Combination Database icons ───────────────────────────
        AssignCombinationIcons(
            Spr("quesopeperoniicon.ase"),
            Spr("quesopiñaicon.ase")
        );

        Debug.Log("[AssignFinalSprites] All sprites assigned successfully!");
    }

    static void SetPrefabSprite(string prefabPath, Sprite sprite)
    {
        if (sprite == null) { Debug.LogWarning($"[AssignFinalSprites] Sprite not found for {prefabPath}"); return; }
        var go = PrefabUtility.LoadPrefabContents(prefabPath);
        if (go == null) { Debug.LogWarning($"[AssignFinalSprites] Prefab not found: {prefabPath}"); return; }
        var sr = go.GetComponent<SpriteRenderer>() ?? go.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) { Debug.LogWarning($"[AssignFinalSprites] No SpriteRenderer on {prefabPath}"); PrefabUtility.UnloadPrefabContents(go); return; }
        sr.sprite = sprite;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Debug.Log($"[AssignFinalSprites] ✓ {prefabPath}");
    }

    static void AddSpriteRendererToPrefab(string prefabPath, Sprite sprite)
    {
        if (sprite == null) { Debug.LogWarning($"[AssignFinalSprites] Sprite not found for {prefabPath}"); return; }
        var go = PrefabUtility.LoadPrefabContents(prefabPath);
        if (go == null) { Debug.LogWarning($"[AssignFinalSprites] Prefab not found: {prefabPath}"); return; }
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Debug.Log($"[AssignFinalSprites] ✓ Added SpriteRenderer to {prefabPath}");
    }

    static void AssignElementIcons(Sprite pepperoni, Sprite pina, Sprite queso)
    {
        string prefabPath = "Assets/Prefabs/Base/UI.prefab";
        var go = PrefabUtility.LoadPrefabContents(prefabPath);
        if (go == null) { Debug.LogWarning($"[AssignFinalSprites] UI.prefab not found"); return; }

        var elemUI = go.GetComponentInChildren<ElementsUI>(true);
        if (elemUI == null) { Debug.LogWarning("[AssignFinalSprites] ElementsUI not found in UI.prefab"); PrefabUtility.UnloadPrefabContents(go); return; }

        if (pepperoni) { var f = typeof(ElementsUI).GetField("pepperoniSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (f != null) f.SetValue(elemUI, pepperoni); }
        if (pina)      { var f = typeof(ElementsUI).GetField("piñaSprite",      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (f != null) f.SetValue(elemUI, pina); }
        if (queso)     { var f = typeof(ElementsUI).GetField("quesoSprite",     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (f != null) f.SetValue(elemUI, queso); }

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Debug.Log("[AssignFinalSprites] ✓ Element icons assigned in UI.prefab");
    }

    static void AssignButtonSprites(Sprite normal, Sprite pressed)
    {
        string prefabPath = "Assets/Prefabs/Base/UI.prefab";
        var go = PrefabUtility.LoadPrefabContents(prefabPath);
        if (go == null) { Debug.LogWarning($"[AssignFinalSprites] UI.prefab not found"); return; }

        var buttons = go.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        int count = 0;
        foreach (var btn in buttons)
        {
            var img = btn.GetComponent<UnityEngine.UI.Image>();
            if (img != null && normal != null) img.sprite = normal;
            var state = btn.spriteState;
            if (pressed != null) state.pressedSprite = pressed;
            btn.spriteState = state;
            btn.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
            count++;
        }

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Debug.Log($"[AssignFinalSprites] ✓ Button sprites assigned ({count} buttons in UI.prefab)");
    }

    static void UpdateShopkeeperSceneOverride(Sprite sprite)
    {
        if (sprite == null) { Debug.LogWarning("[AssignFinalSprites] shopkeeperrabblet sprite not found"); return; }
        string prefabPath = "Assets/Prefabs/Base/UI.prefab";
        var go = PrefabUtility.LoadPrefabContents(prefabPath);
        if (go == null) { Debug.LogWarning("[AssignFinalSprites] UI.prefab not found"); return; }
        int count = 0;
        foreach (var img in go.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if (img.sprite != null && img.sprite.name == "shopkeeperrabblet") { img.sprite = sprite; count++; }
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
            if (sr.sprite != null && sr.sprite.name == "shopkeeperrabblet") { sr.sprite = sprite; count++; }
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Debug.Log($"[AssignFinalSprites] Shopkeeper: {count} update(s) in UI.prefab");
    }

    static void AssignCombinationIcons(Sprite quesopeperoni, Sprite queopina)
    {
        var db = AssetDatabase.LoadAssetAtPath<CombinationDatabase>("Assets/CombinationDatabase.asset");
        if (db == null) { Debug.LogWarning("[AssignFinalSprites] CombinationDatabase not found"); return; }
        int count = 0;
        foreach (var entry in db.combinations)
        {
            if (entry.combinationKey == "queso|pepperoni" && quesopeperoni != null) { entry.resultSprite = quesopeperoni; count++; }
            if ((entry.combinationKey == "queso|piña" || entry.combinationKey == "queso|pi\u00F1a") && queopina != null) { entry.resultSprite = queopina; count++; }
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AssignFinalSprites] ✓ Combination icons assigned ({count} entries)");
    }
}
