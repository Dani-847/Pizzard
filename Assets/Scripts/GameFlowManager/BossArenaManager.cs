using UnityEngine;

namespace Pizzard.Core
{
    using Bosses;

    /// <summary>
    /// Governs the Boss Fight GameState. Listens to the active BossBase attached in the inspector
    /// and safely transitions the player out of combat when the boss is defeated.
    /// </summary>
    public class BossArenaManager : MonoBehaviour
    {
        [Header("Arena Setup")]
        [Tooltip("Attach the active BossBase object here for the manager to track its lifecycle.")]
        [SerializeField] private BossBase activeBoss;

        private void Start()
        {
            if (activeBoss == null)
            {
                Debug.LogError("[BossArenaManager] No BossBase assigned. The phase won't progress!");
                return;
            }

            Debug.Log($"[BossArenaManager] Phase started for boss: {activeBoss.gameObject.name}");
            
            // Listen for the victory event
            activeBoss.OnBossDefeated.AddListener(HandleBossDefeated);
        }

        private void HandleBossDefeated()
        {
            Debug.Log("[BossArenaManager] Boss Defeated. Returning to Shop Loop.");
            activeBoss.OnBossDefeated.RemoveListener(HandleBossDefeated);
            
            // For now, return to the Shop scene. Later we might want an Outro dialog here.
            GameFlowManager.Instance.ChangeState(GameState.Shop);
        }
    }
}
