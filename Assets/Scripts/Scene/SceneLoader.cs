using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pizzard.Core
{
    /// <summary>
    /// Utility wrapper for loading scenes asynchronously, simplifying Unity's SceneManager.
    /// Used by the GameFlowManager to handle state-based scene transitions.
    /// </summary>
    public static class SceneLoader
    {
        public static void LoadScene(string sceneName, Action onComplete = null)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene: '{sceneName}'. Check Build Settings.");
                return;
            }

            op.completed += operation => 
            {
                Debug.Log($"[SceneLoader] Successfully loaded scene: '{sceneName}'.");
                onComplete?.Invoke();
            };
        }
    }
}