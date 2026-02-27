using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ensures the EventSystem persists across scenes so UI remains interactive.
/// Prevents duplicates from spawning in new scenes.
/// </summary>
[RequireComponent(typeof(EventSystem))]
public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        foreach (var sys in systems)
        {
            if (sys.gameObject != this.gameObject)
            {
                Destroy(sys.gameObject);
                Debug.Log("[PersistentEventSystem] Destroyed duplicate EventSystem from newly loaded scene.");
            }
        }
    }
}
