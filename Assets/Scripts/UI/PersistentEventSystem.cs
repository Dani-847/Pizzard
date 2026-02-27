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
}
