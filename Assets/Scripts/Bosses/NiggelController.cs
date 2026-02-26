using System.Collections;
using UnityEngine;

namespace Pizzard.Bosses
{
    using Progression;

    /// <summary>
    /// Boss 4: Niggel Worthington (The Rich Guy).
    /// Steals currency and health from the player, buffing his own speed or damage resistance.
    /// </summary>
    public class NiggelController : BossBase
    {
        [Header("Niggel Settings")]
        [SerializeField] private float attackInterval = 3f;
        [SerializeField] private float stealRange = 2.5f;
        [SerializeField] private int currencyStealAmount = 10;
        
        [Header("Buffs")]
        public float speedMultiplier = 1.0f;

        private Transform playerTransform; // Needs to be assigned or found dynamically

        protected override void Awake()
        {
            base.Awake();
            
            // Temporary auto-find for the Prototype structure
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(attackInterval);
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            int attackType = Random.Range(1, 4);

            switch (attackType)
            {
                case 1:
                    Attack1_ThrowMoney();
                    break;
                case 2:
                    Attack2_RichDash();
                    break;
                case 3:
                    Attack3_StealStats();
                    break;
            }
        }

        private void Attack1_ThrowMoney()
        {
            Debug.Log("[Niggel Attack 1] Throwing heavy bags of coins at the player.");
            // Instantiate coin bag projectiles
        }

        private void Attack2_RichDash()
        {
            Debug.Log($"[Niggel Attack 2] Dashing across the room with Speed Multiplier: {speedMultiplier}");
            // Move via Rigidbody / Transform interpolation
        }

        private void Attack3_StealStats()
        {
            Debug.Log("[Niggel Attack 3] Attempting to steal stats from the player!");
            
            if (playerTransform == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= stealRange)
            {
                bool stoleCurrency = false;

                if (ProgressionManager.Instance != null && ProgressionManager.Instance.BossCurrency >= currencyStealAmount)
                {
                    // Force-spend currency to 'steal' it without purchasing anything
                    ProgressionManager.Instance.SpendCurrency(currencyStealAmount);
                    stoleCurrency = true;
                    Debug.Log($"[Niggel] Stole {currencyStealAmount} currency! Buffing Speed!");
                    
                    // Buff Niggel
                    speedMultiplier += 0.2f; 
                }
                
                if (!stoleCurrency)
                {
                    // Fallback to stealing health if currency is low
                    Debug.Log("[Niggel] Player is broke! Stealing Health instead!");
                    // playerTransform.GetComponent<PlayerHealth>().TakeDamage(1); 
                }
            }
            else
            {
                Debug.Log("[Niggel] Player was outside steal range. Attack missed.");
            }
        }
    }
}
