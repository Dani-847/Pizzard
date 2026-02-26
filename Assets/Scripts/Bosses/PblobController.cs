using System.Collections;
using UnityEngine;

namespace Pizzard.Bosses
{
    /// <summary>
    /// Boss 1: P'blob. 
    /// A slime whose mustache grows as health drops. Uses a gymkhana room system
    /// where he is invulnerable during the evasion phases.
    /// </summary>
    public class PblobController : BossBase
    {
        [Header("P'blob Specifics")]
        [SerializeField] private float mustacheScaleMultiplier = 1.0f;
        
        // True while the player is in the 'gymkhana' dodging room
        private bool isInvulnerable = true;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(GymkhanaRoutine());
        }

        public override void TakeDamage(int damage)
        {
            if (isInvulnerable)
            {
                Debug.Log("[Pblob] Invulnerable during Gymkhana phase! Attack deflected.");
                return;
            }

            base.TakeDamage(damage);
            GrowMustache();
        }

        private void GrowMustache()
        {
            // The lower the health, the bigger the mustache
            float healthPercent = (float)currentHealth / maxHealth;
            mustacheScaleMultiplier = 1.0f + (1.0f - healthPercent) * 2f; 
            
            Debug.Log($"[Pblob] Mustache grew to scale: {mustacheScaleMultiplier}");
            // TODO: Apply scale to the actual mustache child GameObject transform
        }

        private IEnumerator GymkhanaRoutine()
        {
            // Room 1: Hairballs
            Debug.Log("[Pblob] Room 1 Started: Hairballs");
            Attack1_Hairballs();
            yield return new WaitForSeconds(5f); // Wait for survival duration

            // Room 2: Pelo Areas
            Debug.Log("[Pblob] Room 2 Started: Pelo Safe Zones");
            Attack2_PeloAreas();
            yield return new WaitForSeconds(5f);

            // Room 3: Tile Memory
            Debug.Log("[Pblob] Room 3 Started: Tile Memory");
            Attack3_TileMemory();
            yield return new WaitForSeconds(5f);

            // Phase Complete -> Vulnerable
            isInvulnerable = false;
            Debug.Log("[Pblob] Gymkhana Complete! P'blob is Vulnerable! Hit him!");

            // Stay vulnerable for 5 seconds before restarting the loop
            yield return new WaitForSeconds(5f);
            
            isInvulnerable = true;
            StartCoroutine(GymkhanaRoutine());
        }

        private void Attack1_Hairballs()
        {
            Debug.Log("[Pblob Attack 1] Spawning fast and slow hairballs from the mustache.");
            // Instantiate hairball prefabs with varying rigid body velocities
        }

        private void Attack2_PeloAreas()
        {
            Debug.Log("[Pblob Attack 2] Filling room with hair. Creating shifting rhythm safe zones (Simon Says).");
            // Instantiate moving safe zone circles that the player must stay inside
        }

        private void Attack3_TileMemory()
        {
            Debug.Log("[Pblob Attack 3] Activating dangerous floor tiles. Player must remember the safe path.");
            // Activate damage triggers on a grid
        }
    }
}
