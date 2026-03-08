// Assets/Scripts/Playground/PlaygroundRespawnHandler.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Intercepts player death in PlaygroundScene and respawns the player at the
/// designated spawn point instead of triggering GameFlowManager / scene change.
/// </summary>
public class PlaygroundRespawnHandler : MonoBehaviour
{
    [SerializeField] private PlayerHPController playerHP;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 1.5f;

    private void Awake()
    {
        if (playerHP == null)
            playerHP = FindObjectOfType<PlayerHPController>();

        if (playerHP != null)
            playerHP.onDeathOverride = () => StartCoroutine(RespawnAfterDelay());
        else
            Debug.LogError("[PlaygroundRespawnHandler] PlayerHPController not found. Respawn will not work.");
    }

    private IEnumerator RespawnAfterDelay()
    {
        // Brief pause so the death flash/animation can play (uses real time — not affected by timeScale)
        yield return new WaitForSecondsRealtime(respawnDelay);

        // Teleport to spawn point
        if (spawnPoint != null)
            playerHP.transform.position = spawnPoint.position;
        else
            Debug.LogWarning("[PlaygroundRespawnHandler] spawnPoint not assigned — respawning in place.");

        // Restore full HP and refresh HP bar
        playerHP.RestaurarVidaCompleta();
    }

    private void OnDestroy()
    {
        // Clean up the override when scene unloads so normal gameplay is unaffected
        if (playerHP != null)
            playerHP.onDeathOverride = null;
    }
}
