// Assets/Scripts/Playground/PlaygroundManager.cs
using UnityEngine;

public class PlaygroundManager : MonoBehaviour, ITokenSource
{
    public static PlaygroundManager Instance { get; private set; }

    private const int StartingTokens = 10;
    private int _playgroundTokens;

    public int PlaygroundTokens => _playgroundTokens;

    // Static bridge: survives scene transitions so Shop scene can read/write tokens
    public static bool IsPlaygroundSession { get; private set; }
    private static int _cachedTokens = StartingTokens;

    /// <summary>
    /// Called by PlaygroundHUDController before loading Shop scene.
    /// Saves current tokens into the static cache and raises the session flag.
    /// </summary>
    public static void BeginShopSession(int currentTokens)
    {
        IsPlaygroundSession = true;
        _cachedTokens = currentTokens;
    }

    /// <summary>
    /// Called by ShopPhaseManager when the player exits the shop back to Playground.
    /// Persists the spent-token count so PlaygroundManager restores correctly.
    /// </summary>
    public static void EndShopSession(int remainingTokens)
    {
        _cachedTokens = remainingTokens;
        IsPlaygroundSession = false;
    }

    /// <summary>
    /// Returns the static token cache (used by Shop scene before PlaygroundManager exists).
    /// </summary>
    public static int GetCachedTokens() => _cachedTokens;

    /// <summary>
    /// Spends from the static cache (used by Shop scene ITokenSource proxy).
    /// </summary>
    public static bool SpendCachedTokens(int amount)
    {
        if (_cachedTokens < amount) return false;
        _cachedTokens -= amount;
        return true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Restore tokens from cache (set by BeginShopSession) or use starting value
        _playgroundTokens = _cachedTokens;
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
        _cachedTokens = _playgroundTokens; // keep cache in sync
        return true;
    }
}
