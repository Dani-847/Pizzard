using UnityEngine;
using UnityEditor;

public class InspectAseAssets : EditorWindow
{
    [MenuItem("Tools/Inspect Ase Animation Assets")]
    public static void Inspect()
    {
        string[] animFiles = {
            "Assets/Sprites/Spritesforfinalv1/playergoingsideawaysanimation.ase",
            "Assets/Sprites/Spritesforfinalv1/pablobwhenattacking.ase",
            "Assets/Sprites/Spritesforfinalv1/niggleworthingtonmovingsideaways.ase"
        };

        foreach (var path in animFiles)
        {
            Debug.Log($"=== {path} ===");
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in allAssets)
                Debug.Log($"  [{a.GetType().Name}] name='{a.name}'  instanceID={a.GetInstanceID()}");
        }
    }
}
