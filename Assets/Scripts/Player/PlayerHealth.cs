using UnityEngine;
using UnityEngine.Events;

namespace Pizzard.Player
{
    /// <summary>
    /// Tracks a discrete number of hit points for the player.
    /// Fires events when damaged or dead so UI can react and GameFlowManager can restart loops.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHits = 3;
        
        [Header("Events")]
        public UnityEvent<int> OnDamageTaken;
        public UnityEvent OnDeath;

        public int CurrentHits { get; private set; }
        private bool isDead = false;

        private void Awake()
        {
            CurrentHits = maxHits;
        }

        /// <summary>
        /// Call to register a discrete hit against the player.
        /// </summary>
        public void TakeDamage(int damageHits = 1)
        {
            if (isDead) return;

            CurrentHits -= damageHits;
            CurrentHits = Mathf.Max(0, CurrentHits); // Prevent negative health

            Debug.Log($"[PlayerHealth] Took {damageHits} hit(s). Current HP: {CurrentHits}/{maxHits}");
            OnDamageTaken?.Invoke(CurrentHits);

            if (CurrentHits <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            Debug.Log("[PlayerHealth] Player has died.");
            OnDeath?.Invoke();

            // Optional future feature: Trigger GameFlowManager state to generic Game Over screen
        }

        public void Heal(int healHits)
        {
            if (isDead) return;

            CurrentHits += healHits;
            CurrentHits = Mathf.Min(maxHits, CurrentHits);
            Debug.Log($"[PlayerHealth] Healed. Current HP: {CurrentHits}/{maxHits}");
        }
    }
}
