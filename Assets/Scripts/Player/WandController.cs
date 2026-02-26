using System.Collections.Generic;
using UnityEngine;

namespace Pizzard.Player
{
    using Elements;

    /// <summary>
    /// Handles the player's wand logic, queueing up elements (up to the current Tier limit),
    /// and evaluating the combination to "cast" a specific spell.
    /// </summary>
    public class WandController : MonoBehaviour
    {
        [Header("Wand Settings")]
        [Range(1, 3)]
        [SerializeField] private int currentWandTier = 1;
        
        // Expose internally queued elements
        private List<ElementType> queuedElements = new List<ElementType>();

        private void Update()
        {
            HandleElementInput();
            HandleCastInput();
        }

        private void HandleElementInput()
        {
            // Stop taking elements if we hit the tier cap
            if (queuedElements.Count >= currentWandTier) return;

            // Mapping 1, 2, 3 to Elements
            if (Input.GetKeyDown(KeyCode.Alpha1)) QueueElement(ElementType.queso);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) QueueElement(ElementType.pepperoni);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) QueueElement(ElementType.piña);
        }

        private void HandleCastInput()
        {
            // Right click or E or something generic to 'Cast' the queue
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.E))
            {
                if (queuedElements.Count > 0)
                {
                    Cast();
                }
            }
        }

        private void QueueElement(ElementType element)
        {
            queuedElements.Add(element);
            Debug.Log($"[WandController] Queued: {element}. Queue Size: {queuedElements.Count}/{currentWandTier}");
        }

        private void Cast()
        {
            // Evaluate the combination
            string spellId = ElementCombiner.EvaluateCombination(queuedElements);
            
            Debug.Log($"[WandController] CASTING SPELL: {spellId}");

            // TODO: Fetch bullet/utility prefab from an asset dictionary and instantiate it here.

            // Clean the queue for the next combo
            queuedElements.Clear();
        }

        /// <summary>
        /// Used by the Shop system to upgrade the player's wand capabilities.
        /// </summary>
        public void UpgradeWandTier()
        {
            currentWandTier++;
            currentWandTier = Mathf.Clamp(currentWandTier, 1, 3);
            Debug.Log($"[WandController] Wand upgraded to Tier {currentWandTier}!");
        }
    }
}
