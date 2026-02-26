using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pizzard.EditorTools
{
    [InitializeOnLoad]
    public static class BuildSettingsSetup
    {
        static BuildSettingsSetup()
        {
            EditorApplication.delayCall += SetupScenes;
        }

        [MenuItem("Tools/Pizzard/Setup Build Settings")]
        public static void SetupScenes()
        {
            string[] requiredScenes = new string[]
            {
                "Assets/FlowScenes/MainMenu.unity",
                "Assets/FlowScenes/IntroDialog.unity",
                "Assets/FlowScenes/Shop.unity",
                "Assets/FlowScenes/PreBossDialog.unity",
                "Assets/FlowScenes/BossArena_1.unity",
                "Assets/FlowScenes/BossArena_2.unity",
                "Assets/FlowScenes/BossArena_3.unity",
                "Assets/FlowScenes/BossArena_4.unity",
                "Assets/FlowScenes/PostBossDialog.unity",
                "Assets/FlowScenes/Credits.unity"
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            foreach (string scenePath in requiredScenes)
            {
                var sceneObj = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneObj != null)
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
                else
                {
                    Debug.LogWarning($"[BuildSettingsSetup] Scene not found in Project: {scenePath}");
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("[BuildSettingsSetup] Build Settings updated successfully!");
        }
    }
}
