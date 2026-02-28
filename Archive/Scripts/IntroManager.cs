using UnityEngine;
using Pizzard.UI; // DialogController

namespace Pizzard.Core
{
    /// <summary>
    /// Manages the introductory narrative phase. 
    /// Sends text to the Dialog UI and triggers a switch to the Shop state when completed.
    /// </summary>
    public class IntroManager : MonoBehaviour
    {
        [SerializeField] private DialogController dialogController;

        // Placeholder tutorial/intro script
        private readonly string[] introText = new string[]
        {
            "Welcome to the start of the prototype!",
            "You'll learn about elements here.",
            "Combine Quezo, Pepperoni, and Pina to conquer.",
            "Let's check out the Shop next!"
        };

        private void Start()
        {
            if (dialogController == null)
            {
                Debug.LogError("[IntroManager] DialogController is not assigned! Skipping Intro...");
                TransitionToShop();
                return;
            }

            // Start dialogue, setup callback to exit phase
            dialogController.StartDialog(introText, TransitionToShop);
        }

        private void TransitionToShop()
        {
            Debug.Log("[IntroManager] Intro dialog finished. Loading Shop.");
            GameFlowManager.Instance.ChangeState(GameState.Shop);
        }
    }
}
