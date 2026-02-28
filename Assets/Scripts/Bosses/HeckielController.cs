using UnityEngine;
using Pizzard.Elements;

namespace Pizzard.Bosses
{
    /// <summary>
    /// Boss 2: Hec'kiel.
    /// An elemental dragon that reflects the last element it was hit by.
    /// Splits into two attacking phases when health drops below 50%.
    /// </summary>
    public class HeckielController : BossBase
    {
        [Header("Hec'kiel Settings")]
        [SerializeField] private float attackInterval = Core.GameBalance.Bosses.Heckiel.AttackInterval;

        private ElementType lastReceivedElement = ElementType.None;
        private bool isPhaseTwo = false;
        private float attackTimer;

        protected override void Awake()
        {
            base.Awake();
            attackTimer = attackInterval;
        }

        private void Update()
        {
            if (isDead) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                PerformRandomAttack();
                attackTimer = attackInterval;
            }
        }

        public override void TakeDamage(int damage)
        {
            // Note: Ideally the projectile passing 'damage' would also pass its 'ElementType' 
            // but for Prototype structural purposes, we reduce health first:
            base.TakeDamage(damage);

            // Check for phase transition
            if (!isPhaseTwo && currentHealth <= (maxHealth * Core.GameBalance.Bosses.Heckiel.Phase2ThresholdPercent))
            {
                EnterPhaseTwo();
            }
        }

        /// <summary>
        /// Example method meant to be called by the player's projectile upon impact.
        /// </summary>
        public void RegisterElementHit(ElementType type)
        {
            lastReceivedElement = type;
        }

        private void PerformRandomAttack()
        {
            int attackIndex = Random.Range(1, 4);

            if (isPhaseTwo)
            {
                Debug.Log("[Heckiel] Phase 2 Split Attack! Firing from TWO origins!");
                // Implementation: Instantiate two different attacks merging together.
            }
            else
            {
                switch(attackIndex)
                {
                    case 1:
                        Attack1_RandomElements();
                        break;
                    case 2:
                        Attack2_ReflectLastElement();
                        break;
                    case 3:
                        Attack3_CombineElements();
                        break;
                }
            }
        }

        private void EnterPhaseTwo()
        {
            isPhaseTwo = true;
            Debug.Log("[Heckiel] Health critical! Dragon splitting into two parts!");
            // Implementation: Spawn/enable a secondary dragon head GameObject here
        }

        private void Attack1_RandomElements()
        {
            Debug.Log("[Heckiel Attack 1] Firing 2 random elements.");
        }

        private void Attack2_ReflectLastElement()
        {
            Debug.Log($"[Heckiel Attack 2] Reflecting player's last element: {lastReceivedElement}");
        }

        private void Attack3_CombineElements()
        {
            Debug.Log($"[Heckiel Attack 3] Combining random element with {lastReceivedElement} to shoot back.");
        }
    }
}
