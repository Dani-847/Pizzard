using UnityEngine;
using UnityEngine.Events;

namespace Pizzard.Bosses
{
    using Progression;

    /// <summary>
    /// Abstract base class providing common health and event features for all Bosses.
    /// </summary>
    public abstract class BossBase : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected int bossCurrencyReward = 150;
        
        [Header("Events")]
        public UnityEvent OnBossDefeated;

        protected int currentHealth;
        protected bool isDead = false;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Reduces boss health. Can be overridden in derived classes to implement invulnerability phases.
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"[{gameObject.name}] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isDead = true;
            Debug.Log($"[{gameObject.name}] Defeated! Awarding {bossCurrencyReward} Currency.");

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.AddCurrency(bossCurrencyReward);
            }

            OnBossDefeated?.Invoke();
            
            // Clean up or play generic death animation here
            gameObject.SetActive(false);
        }
    }
}
