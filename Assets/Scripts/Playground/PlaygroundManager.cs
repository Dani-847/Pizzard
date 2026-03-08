// Assets/Scripts/Playground/PlaygroundManager.cs
using UnityEngine;

public class PlaygroundManager : MonoBehaviour, ITokenSource
{
    public static PlaygroundManager Instance { get; private set; }

    private const int StartingTokens = 10;
    private int _playgroundTokens;

    public int PlaygroundTokens => _playgroundTokens;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Critical: NOT DontDestroyOnLoad — must be scene-scoped only
        _playgroundTokens = StartingTokens;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ITokenSource implementation
    public int GetTokens() => _playgroundTokens;

    public bool SpendTokens(int amount)
    {
        if (_playgroundTokens < amount) return false;
        _playgroundTokens -= amount;
        return true;
    }
}
