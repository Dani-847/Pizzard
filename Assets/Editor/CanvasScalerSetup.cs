using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor utility to bulk-configure all Canvas components in the project
/// to use "Scale With Screen Size" (1920x1080 reference, match 0.5).
/// Run from GSD > Setup Canvas Scalers menu.
/// </summary>
public static class CanvasScalerSetup
{
    [MenuItem("GSD/Setup Canvas Scalers")]
    public static void SetupAllCanvasScalers()
    {
        // Find all CanvasScaler components in the scene (including inactive)
        var scalers = Object.FindObjectsOfType<CanvasScaler>(true);
        int count = 0;

        foreach (var scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            EditorUtility.SetDirty(scaler);
            count++;
        }

        // Also check prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var prefabScalers = prefab.GetComponentsInChildren<CanvasScaler>(true);
            foreach (var scaler in prefabScalers)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                EditorUtility.SetDirty(prefab);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[CanvasScalerSetup] Configured {count} CanvasScaler(s) to Scale With Screen Size (1920x1080, match 0.5).");
    }
}
