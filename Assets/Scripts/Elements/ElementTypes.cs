using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pizzard.Elements
{

    /// <summary>
    /// Static utility that evaluates a list of queued elements and returns 
    /// the name/ID of the resulting combined spell according to the design spec.
    /// </summary>
    public static class ElementCombiner
    {
        // Simple string building for the ID (e.g., "Queso_Pepperoni_Pina")
        public static string EvaluateCombination(List<ElementType> combo)
        {
            if (combo == null || combo.Count == 0) return "Fart"; // Dud spell

            // We sort if order doesn't matter, but the spec says "el orden es importante"
            // So we join the enum names exactly as input.
            return string.Join("_", combo.Select(e => e.ToString()));
        }
    }
}
