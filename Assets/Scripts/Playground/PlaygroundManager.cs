// Assets/Scripts/Playground/PlaygroundManager.cs
using System.Collections.Generic;
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
    /// Wand tier tracked independently of the real save — starts at 1, increases per shop buy.
    /// </summary>
    public static int PlaygroundWandTier = 1;

    /// <summary>
    /// Element choices made in the playground shop — persists across scene transitions.
    /// </summary>
    public static List<ElementType> PlaygroundWandElements = new List<ElementType>();

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
        // Don't clear IsPlaygroundSession here — PlayerEquip.Start() still needs it
        // when PlaygroundScene loads. It gets cleared in ClearSession().
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
        IsPlaygroundSession = true;
        // Restore tokens from cache (set by BeginShopSession) or use starting value
        _playgroundTokens = _cachedTokens;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Resets all static playground state. Call when leaving playground entirely (back to menu).
    /// </summary>
    public static void ClearSession()
    {
        IsPlaygroundSession = false;
        _cachedTokens = StartingTokens;
        PlaygroundWandTier = 1;
        PlaygroundWandElements.Clear();
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
