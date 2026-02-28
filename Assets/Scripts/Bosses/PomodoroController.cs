using System.Collections;
using UnityEngine;

namespace Pizzard.Bosses
{
    /// <summary>
    /// Boss 3: Pomodoro Paganini (The Oven).
    /// Stationary boss. Only takes damage when the player reflects specific projectiles back at it
    /// using the Dash mechanic.
    /// </summary>
    public class PomodoroController : BossBase
    {
        [Header("Pomodoro Settings")]
        [SerializeField] private float attackInterval = Core.GameBalance.Bosses.Pomodoro.AttackInterval;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(AttackRoutine());
        }

        public override void TakeDamage(int damage)
        {
            // In a real implementation, we might check a flag on the projectile
            // to see if it was "Reflected" before calling base.TakeDamage.
            // For now, we assume any damage arriving here bypassed his normal invulnerability.
            
            Debug.Log("[Pomodoro] Hit by reflected projectile!");
            base.TakeDamage(damage);
        }

        // Custom method called by standard wands to show he's immune
        public void BlockStandardAttack()
        {
            Debug.Log("[Pomodoro] *Clang!* The oven door is shut! Standard attacks do nothing.");
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
            int attackType = Random.Range(1, 3);
            
            if (attackType == 1)
            {
                Attack1_DeflectableProjectiles();
            }
            else
            {
                Attack2_AreaOfEffectBursts();
            }
        }

        private void Attack1_DeflectableProjectiles()
        {
            Debug.Log("[Pomodoro Attack 1] Spawning slow, heavy projectiles that can be dashed into to reflect.");
            // Instantiate "Ping-Pong" style projectiles targeting the player
        }

        private void Attack2_AreaOfEffectBursts()
        {
            Debug.Log("[Pomodoro Attack 2] Spawning ground AoE fire warnings.");
            // Instantiate non-deflectable ground hazards that the player must simply avoid
        }
    }
}
