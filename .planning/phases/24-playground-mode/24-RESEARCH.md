# Phase 24: Playground Mode - Research

**Researched:** 2026-03-08
**Domain:** Unity 2D — scene management, isolated token economy, DPS metering, world-space UI, animator-driven pulse
**Confidence:** HIGH (all findings grounded in existing project source code)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Playground always starts with 10 tokens on entry — no persistence, reset every session
- Token state is in-memory only — a PlaygroundManager holds the value locally, never touches SaveManager or PlayerPrefs
- Playground shop offers the same items as the main game shop (reuse ShopUI)
- Dummy is invincible (infinite HP) — never dies, players can attack indefinitely
- DPS counter shows rolling 3-second DPS: (damage in last 3s) / 3, updated in real time
- DPS counter is a world-space canvas tag floating above the dummy — visually connected to the target
- No name label on dummy — DPS number only above the dummy
- Falling projectile falls at a fixed periodic interval (e.g., every 3 seconds)
- Spawns at a fixed position on the left side of the arena — always same spot, predictable
- Player has HP and can die; on death, player respawns at the Playground spawn point (no scene exit)
- Playground is a new Unity scene (e.g., "PlaygroundScene"), following the BossArena_X pattern
- Loaded via SceneManager from the main menu
- Player exits via a visible "Back to Menu" button in the Playground HUD
- Playground button position on main menu: below Play/Continue, above Settings
- Playground button pulses on every game launch until clicked during that session (in-memory only — NOT PlayerPrefs)
- Pulse style: scale animation (1.0 → 1.05 → 1.0 loop) using Unity Animator
- Pulse stops when the player clicks the Playground button
- A "Shop" button is always visible in the Playground HUD
- Opening the shop pauses time (Time.timeScale = 0) so projectiles stop while browsing
- Same shop UI/items as main game, running against the in-memory 10-token balance
- Reuse existing BossArena visual assets (floor/background tiles) — no new art needed
- No "Playground" scene label — the dummy and DPS counter make the context obvious

### Claude's Discretion
- Exact projectile fall speed and damage amount
- Respawn delay after player death
- DPS counter font/size relative to existing UI scale
- Shop button placement within the Playground HUD
- Any visual distinction between main-game shop and playground shop (e.g., token counter showing "10 / 10" instead of main-game currency)

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope.
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| PLAY-01 | Playground scene is accessible from the main menu via a dedicated button | MenuUI.cs button pattern + SceneLoader.LoadScene() |
| PLAY-02 | Playground has completely separate token count and progress from the main game | PlaygroundManager singleton with in-memory int — never calls ProgressionManager or SaveManager |
| PLAY-03 | Playground shop starts with 10 tokens each session | PlaygroundManager.Init() sets tokens = 10 on Awake; ShopUI needs abstracted token source |
| PLAY-04 | Playground scene includes a dummy on the right side with a live DPS counter | Invincible dummy object + world-space Canvas TextMeshPro + rolling 3s DPS ring buffer |
| PLAY-05 | Playground scene includes a falling projectile on the left that damages the player | Periodic spawner + EnemyProjectile subclass falling downward; player uses existing PlayerHPController.RecibirDaño |
| ONBD-01 | On first game launch, the Playground button blinks/pulses continuously until clicked | In-memory bool on MenuUI; Unity Animator state machine on button; trigger on click |
</phase_requirements>

---

## Summary

Phase 24 adds a fully self-contained Playground scene to the main menu. The work divides cleanly into four areas: (1) wiring a new button in MenuUI and loading the new scene, (2) isolating the token economy behind a PlaygroundManager singleton so ShopUI never touches ProgressionManager while in Playground context, (3) building the Playground scene with a dummy, DPS counter, and falling projectile hazard, and (4) adding the first-launch pulse animation to the Playground button.

The biggest technical risk is the ShopUI/ShopController coupling to ProgressionManager. Every purchase method calls `ProgressionManager.Instance.SpendCurrency()` directly — there is no abstraction. The cleanest approach is to add a `ITokenSource` interface (or a simple delegate pattern) that both ProgressionManager and PlaygroundManager implement, then have ShopUI and ShopController resolve the current token source from a context object set before opening the shop. This avoids forking ShopUI into a second class.

The DPS counter is the only net-new UI component. World-space Canvas with TextMeshPro attached to the dummy GameObject is the standard Unity pattern. The 3-second rolling window is implemented with a simple list of (time, damage) pairs — drain entries older than 3 seconds on each Update, sum remaining damage, divide by 3.

**Primary recommendation:** Build PlaygroundManager as a scene-scoped singleton (not DontDestroyOnLoad) that initialises with 10 tokens on Awake. Introduce a minimal `ITokenSource` interface to decouple ShopUI from ProgressionManager. Keep all other components (PlayerHPController, EnemyProjectile, SceneLoader) unchanged.

---

## Standard Stack

### Core (already in project)
| Component | Version | Purpose | Notes |
|-----------|---------|---------|-------|
| Unity SceneManager | Unity 2022+ | Load PlaygroundScene | Use `SceneLoader.LoadScene("PlaygroundScene")` — existing wrapper |
| PlayerHPController | existing | Player HP + damage flash | `RecibirDaño(int)` — already handles death callback to GameFlowManager; in Playground, death must NOT call GameFlowManager |
| EnemyProjectile | existing | Base class for falling hazard | Subclass or configure direction to fall downward; already tagged "EnemyProjectile" |
| ShopUI + ShopController | existing | Shop UI | Requires token source abstraction |
| ProgressionManager | existing | Main-game tokens | Must NOT be called from Playground |
| TMPro (TextMeshPro) | existing | DPS counter text | Already used throughout UI |
| Unity Animator | existing | Button pulse animation | Already used on boss controllers |

### New Components to Create
| Component | Purpose |
|-----------|---------|
| `PlaygroundManager.cs` | Scene-scoped singleton; holds `playgroundTokens`; implements `ITokenSource` |
| `ITokenSource` (interface) | Decouples ShopUI from ProgressionManager |
| `DummyDPSTracker.cs` | Receives damage events; maintains rolling 3s window; updates world-space TextMeshPro |
| `PlaygroundProjectileSpawner.cs` | Coroutine-based periodic spawner at fixed left-side position |
| `PlaygroundRespawnHandler.cs` | Listens to PlayerHPController.OnDeath equivalent; respawns player without scene change |

---

## Architecture Patterns

### Recommended Scene Structure
```
PlaygroundScene
├── [GameObjects]
│   ├── Player (existing prefab)
│   │   └── DamageReceiver (existing)
│   │   └── PlayerHPController (existing) — OnDeath overridden by PlaygroundRespawnHandler
│   ├── TrainingDummy
│   │   ├── SpriteRenderer
│   │   ├── Collider2D (trigger, tagged "Boss" to absorb player spells)
│   │   ├── DummyDPSTracker.cs
│   │   └── DPSCanvas (World Space Canvas)
│   │       └── DPSText (TextMeshPro)
│   ├── ProjectileSpawnPoint (left side)
│   │   └── PlaygroundProjectileSpawner.cs
│   ├── PlayerSpawnPoint
│   └── PlaygroundManager (GameObject)
│       └── PlaygroundManager.cs
├── [UI Canvas] (Screen Space — Overlay)
│   ├── ShopButton
│   ├── BackToMenuButton
│   ├── TokenCounterText
│   └── ShopUI (hidden by default, revealed on ShopButton click)
└── [Background/Arena] (reuse BossArena tile assets)
```

### Pattern 1: Token Source Abstraction
**What:** Interface with `int GetTokens()` and `bool SpendTokens(int amount)` methods.
**When to use:** Any time ShopUI or ShopController needs a currency balance.
**Example:**
```csharp
// New file: Assets/Scripts/Interfaces/ITokenSource.cs
public interface ITokenSource
{
    int GetTokens();
    bool SpendTokens(int amount);
}

// ProgressionManager implements it (main game):
public bool SpendTokens(int amount) => SpendCurrency(amount);
public int GetTokens() => BossCurrency;

// PlaygroundManager implements it (playground):
public int GetTokens() => playgroundTokens;
public bool SpendTokens(int amount)
{
    if (playgroundTokens < amount) return false;
    playgroundTokens -= amount;
    return true;
}
```

### Pattern 2: Rolling DPS Window
**What:** List of timestamped damage events; pruned on each Update; sum / 3.0f = DPS.
**When to use:** DummyDPSTracker.cs
**Example:**
```csharp
private List<(float time, float damage)> _hits = new();

public void RegisterHit(float damage)
{
    _hits.Add((Time.time, damage));
}

private void Update()
{
    float cutoff = Time.time - 3f;
    _hits.RemoveAll(h => h.time < cutoff);
    float dps = _hits.Count > 0 ? _hits.Sum(h => h.damage) / 3f : 0f;
    _dpsText.text = $"{dps:F1} DPS";
}
```

### Pattern 3: Playground Respawn (no scene exit)
**What:** Subscribe to the player's death event; instead of calling GameFlowManager, teleport player to spawn point and restore HP.
**When to use:** PlaygroundRespawnHandler.cs — runs in PlaygroundScene only.
**Critical detail:** `PlayerHPController.OnDeath()` calls `GameFlowManager.Instance.OnPlayerDeath()` if GameFlowManager exists. In PlaygroundScene, GameFlowManager must NOT be present, OR PlaygroundRespawnHandler must intercept before the call. The safest approach: PlaygroundRespawnHandler subscribes to a Unity Event on PlayerHPController and sets a flag, OR use the fact that without a GameFlowManager in the scene, the `deathUI` path triggers — wire `deathUI` to be null and add a `[PlaygroundDeath]` component that overrides via a new public method.

Recommended approach: Add a nullable `public Action onDeathOverride` to PlayerHPController; if set, call that instead of GameFlowManager/deathUI. PlaygroundRespawnHandler sets this in Awake.

### Pattern 4: Animator-Driven Button Pulse
**What:** Animator Controller on Playground button with two states: `Idle` and `Pulsing`. On game launch, transition to Pulsing. On button click, transition back to Idle.
**When to use:** MenuUI.cs — add `private bool _playgroundPulseActive = true` (resets on every play session automatically since it's in-memory).
**Example:**
```csharp
// In MenuUI.cs
[SerializeField] private Animator playgroundButtonAnimator;
private bool _playgroundPulseActive = true;

void Start()
{
    // existing setup...
    if (_playgroundPulseActive && playgroundButtonAnimator != null)
        playgroundButtonAnimator.SetBool("Pulsing", true);
}

public void OnClickPlayground()
{
    if (playgroundButtonAnimator != null)
        playgroundButtonAnimator.SetBool("Pulsing", false);
    _playgroundPulseActive = false;
    SceneLoader.LoadScene("PlaygroundScene");
}
```

### Anti-Patterns to Avoid
- **Calling ProgressionManager from PlaygroundScene:** Permanently mutates main-game save. Use PlaygroundManager exclusively.
- **Calling SaveManager from PlaygroundScene:** Same risk. No save calls anywhere in Playground code.
- **Using DontDestroyOnLoad for PlaygroundManager:** It would leak into main game scenes. PlaygroundManager must be a normal scene-scoped singleton.
- **Subclassing ShopUI for Playground:** Creates maintenance burden. Use the ITokenSource abstraction instead.
- **Having GameFlowManager present in PlaygroundScene:** PlayerHPController.OnDeath() would call GameFlowManager.OnPlayerDeath() and trigger game-over logic. Either exclude GameFlowManager from the scene or add the override hook.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Scene loading | Custom async wrapper | `SceneLoader.LoadScene()` (existing) | Already handles async + error logging |
| Player damage | Custom damage system | `PlayerHPController.RecibirDaño()` + `EnemyProjectile` (existing) | Invulnerability frames, flash, HP UI all handled |
| Timed projectile destruction | Manual coroutine | `EnemyProjectile.lifetime` + `Destroy(gameObject, lifetime)` | Already in base class |
| Token display in shop | New text component | `ShopUI.txtTokenCount` + `RefreshTokens()` | Already auto-creates if not wired |
| Shop item buttons | New UI | Reuse existing ShopUI panel | All items, exit lock logic, localization already there |

---

## Common Pitfalls

### Pitfall 1: PlayerHPController Death Delegates to GameFlowManager
**What goes wrong:** Player dies in Playground, GameFlowManager.OnPlayerDeath() is called, triggers normal game-over flow or scene transition.
**Why it happens:** `PlayerHPController.OnDeath()` hardcodes a call to `GameFlowManager.Instance` — it always calls it if the instance exists.
**How to avoid:** Either (a) do not include GameFlowManager in the PlaygroundScene, or (b) add an `onDeathOverride` Action to PlayerHPController that PlaygroundRespawnHandler sets.
**Warning signs:** After player death in Playground, the scene changes unexpectedly.

### Pitfall 2: ShopUI.Hide() Calls SaveManager.SaveGame()
**What goes wrong:** Closing the Playground shop auto-saves the main game state.
**Why it happens:** `ShopUI.Hide()` (line 321) explicitly calls `SaveManager.Instance?.SaveGame()`.
**How to avoid:** Add a `bool suppressSave` flag to `ShopUI.Hide(bool suppressSave = false)` and call `Hide(suppressSave: true)` from the Playground shop close button.
**Warning signs:** Main-game token balance changes after visiting Playground.

### Pitfall 3: ProgressionManager is DontDestroyOnLoad
**What goes wrong:** ProgressionManager survives the scene load into PlaygroundScene; ShopUI still reads its balance instead of PlaygroundManager's.
**Why it happens:** `ProgressionManager.Awake()` calls `DontDestroyOnLoad(gameObject)` — it persists across all scenes.
**How to avoid:** The ITokenSource abstraction is the fix. ShopUI resolves token source at runtime, not hardcoded to ProgressionManager.
**Warning signs:** Shop in Playground shows main-game token balance.

### Pitfall 4: Dummy Absorbs Player Projectiles via Wrong Tag
**What goes wrong:** Player spells pass through dummy without registering DPS hits because the dummy is not tagged correctly.
**Why it happens:** `CharacterProjectile` checks tags for collision. Dummy needs a tag that CharacterProjectile's collision handler routes damage to.
**How to avoid:** Check how `CharacterProjectile.cs` applies damage to boss objects. Tag dummy appropriately (likely "Boss" or a new "Dummy" tag). `DummyDPSTracker.RegisterHit()` must be called from that collision path.
**Warning signs:** Spells visually hit dummy but DPS counter stays at 0.

### Pitfall 5: Time.timeScale = 0 Freezes Animator Pulse
**What goes wrong:** When shop opens (Time.timeScale = 0), Animator-driven animations also freeze.
**Why it happens:** Unity Animator uses scaled time by default.
**How to avoid:** Set `playgroundButtonAnimator.updateMode = AnimatorUpdateMode.UnscaledTime` so the pulse continues independently of timeScale. Also ensure the DPS counter's Update logic uses `Time.unscaledTime` when timeScale = 0 — or simply stop updating DPS display when shop is open.
**Warning signs:** Button pulse stops whenever shop is open.

---

## Code Examples

### Adding botonPlayground to MenuUI.cs
```csharp
// In MenuUI.cs — add alongside existing button fields
[Tooltip("Opens Playground scene")]
public Button botonPlayground;
[SerializeField] private Animator playgroundButtonAnimator;
private bool _playgroundPulseActive = true;

// In Start():
if (botonPlayground != null)
    botonPlayground.onClick.AddListener(OnClickPlayground);
if (_playgroundPulseActive && playgroundButtonAnimator != null)
    playgroundButtonAnimator.SetBool("Pulsing", true);

// New method:
public void OnClickPlayground()
{
    if (playgroundButtonAnimator != null)
        playgroundButtonAnimator.SetBool("Pulsing", false);
    _playgroundPulseActive = false;
    SceneLoader.LoadScene("PlaygroundScene");
}
```

### PlaygroundManager.cs skeleton
```csharp
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
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // NOT DontDestroyOnLoad — scene-scoped only
        _playgroundTokens = StartingTokens;
    }

    // ITokenSource
    public int GetTokens() => _playgroundTokens;
    public bool SpendTokens(int amount)
    {
        if (_playgroundTokens < amount) return false;
        _playgroundTokens -= amount;
        return true;
    }
}
```

### DummyDPSTracker.cs skeleton
```csharp
// Assets/Scripts/Playground/DummyDPSTracker.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DummyDPSTracker : MonoBehaviour
{
    [SerializeField] private TextMeshPro dpsText; // world-space TMP
    private readonly List<(float time, float damage)> _hits = new();

    public void RegisterHit(float damage)
    {
        _hits.Add((Time.time, damage));
    }

    private void Update()
    {
        float cutoff = Time.time - 3f;
        _hits.RemoveAll(h => h.time < cutoff);
        float dps = _hits.Count > 0 ? _hits.Sum(h => h.damage) / 3f : 0f;
        if (dpsText != null)
            dpsText.text = $"{dps:F1}";
    }
}
```

### PlaygroundProjectileSpawner.cs skeleton
```csharp
// Assets/Scripts/Playground/PlaygroundProjectileSpawner.cs
using System.Collections;
using UnityEngine;

public class PlaygroundProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab; // EnemyProjectile subclass
    [SerializeField] private float spawnInterval = 3f;

    private void Start() => StartCoroutine(SpawnLoop());

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (Time.timeScale > 0f) // Don't spawn while shop open
                Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, -90f));
                // -90 degrees rotates "right" velocity to fall downward
        }
    }
}
```

---

## State of the Art

| Old Approach | Current Approach | Notes |
|--------------|-----------------|-------|
| `SceneManager.LoadScene()` directly | `SceneLoader.LoadScene()` wrapper | Project standard since Phase 3 |
| Animator booleans for UI state | Same — still standard | No newer pattern in this project |

---

## Open Questions

1. **How does CharacterProjectile route damage to targets?**
   - What we know: `EnemyProjectile` checks `other.CompareTag("Player")` to damage player. CharacterProjectile presumably checks `"Boss"` tag.
   - What's unclear: Exact tag or interface used for boss damage — needs verification in `CharacterProjectile.cs` before tagging the dummy.
   - Recommendation: Read `CharacterProjectile.cs` during Wave 0 and tag dummy accordingly (or add a `IDamageable` call path).

2. **Does ShopUI's exit hard-lock (bossIndex == 1, wandTier == 0) interfere in Playground?**
   - What we know: `ShopUI.RefreshExitButton()` reads `SaveManager.Instance.CurrentSave.bossIndex` and `wandTier`.
   - What's unclear: Whether SaveManager is present in PlaygroundScene (it likely is — attached to ProgressionManager which is DontDestroyOnLoad).
   - Recommendation: In Playground context, call `shopUI.RefreshExitButton()` after overriding token source, OR add a `isPlayground` flag to ShopUI that bypasses the hard-lock check.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — Unity Play Mode testing only |
| Config file | none |
| Quick run command | Play mode in Unity Editor, observe console |
| Full suite command | Play mode — manual smoke test checklist |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| PLAY-01 | Playground button loads PlaygroundScene | manual smoke | Play in Editor, click Playground button | N/A |
| PLAY-02 | Main-game tokens unchanged after Playground | manual smoke | Enter Playground, spend tokens, exit, check main game | N/A |
| PLAY-03 | Shop shows 10 tokens on Playground entry | manual smoke | Enter Playground, open shop, verify token count | N/A |
| PLAY-04 | DPS counter updates when hitting dummy | manual smoke | Fire spells at dummy, observe DPS text | N/A |
| PLAY-05 | Falling projectile damages player | manual smoke | Stand under spawn point, observe HP reduction | N/A |
| ONBD-01 | Pulse animates on launch, stops on click | manual smoke | Launch game, verify pulse; click button, verify stops | N/A |

### Sampling Rate
- **Per task commit:** Manual play test of the specific task's feature
- **Per wave merge:** Full manual smoke run through all 6 requirements
- **Phase gate:** All 6 manual checks green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] No automated test infrastructure exists — all validation is manual play mode
- [ ] `CharacterProjectile.cs` collision routing must be read before implementing dummy hit registration

---

## Sources

### Primary (HIGH confidence)
- `Assets/Scripts/UI/MenuUI.cs` — existing button wiring pattern, `OnClickJugar` flow
- `Assets/Scripts/Progression/ProgressionManager.cs` — DontDestroyOnLoad singleton pattern, `SpendCurrency`/`BossCurrency`
- `Assets/Scripts/UI/ShopUI.cs` — `RefreshTokens()`, `Hide()` auto-save, hard-lock logic
- `Assets/Scripts/UI/ShopController.cs` — direct ProgressionManager coupling
- `Assets/Scripts/Player/PlayerHPController.cs` — `RecibirDaño()`, `OnDeath()` GameFlowManager coupling
- `Assets/Scripts/ElementsAttack/Projectile/EnemyProjectile.cs` — tag-based collision, damage, velocity direction
- `Assets/Scripts/Scene/SceneLoader.cs` — `LoadScene()` wrapper
- `Assets/Scripts/Player/DamageReciever.cs` — collision entry point for player damage

### Secondary (MEDIUM confidence)
- Unity Animator docs (general knowledge): `SetBool`, `updateMode = UnscaledTime` for time-paused animations
- Unity World Space Canvas (general knowledge): Canvas component `Render Mode = World Space`, child TextMeshPro follows parent transform

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all components verified in source code
- Architecture: HIGH — patterns derived directly from existing code
- Pitfalls: HIGH — each pitfall traced to a specific line in existing source
- Open questions: MEDIUM — require one additional file read (CharacterProjectile.cs)

**Research date:** 2026-03-08
**Valid until:** 2026-04-08 (stable codebase, no fast-moving dependencies)
