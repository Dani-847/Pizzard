using UnityEngine;
using Pizzard.UI;

namespace Pizzard.Core
{
    /// <summary>
    /// Manages the Shop Game Loop phase, providing access to Shop functionalities and
    /// eventually allowing the player to exit the Shop and continue to the Pre-Boss dialog.
    /// When IsPlaygroundSession is true, wires playground tokens and routes exit to PlaygroundScene.
    /// </summary>
    public class ShopPhaseManager : MonoBehaviour
    {
        [Header("Shop Setup")]
        [SerializeField] private ShopController shopController;

        private void Start()
        {
            Debug.Log("[ShopPhaseManager] Shop session started. Welcome!");

            if (PlaygroundManager.IsPlaygroundSession)
            {
                // Wire the shop to use the playground isolated token balance via proxy
                var proxy = new PlaygroundTokenProxy();
                if (shopController != null)
                    shopController.SetTokenSource(proxy);

                // Also wire ShopUI (DontDestroyOnLoad) to the same proxy
                var shopUI = FindObjectOfType<ShopUI>(true);
                if (shopUI != null)
                {
                    shopUI.SetTokenSource(proxy);
                    shopUI.Show();
                }
            }
            else if (shopController == null)
            {
                Debug.LogWarning("[ShopPhaseManager] ShopController missing. The UI won't function.");
            }
        }

        /// <summary>
        /// Triggered by a standard "Leave Shop" UI Button.
        /// In playground mode: saves remaining tokens and returns to PlaygroundScene.
        /// In normal mode: transitions to the next phase in the Main Loop.
        /// </summary>
        public void OnLeaveShop()
        {
            if (PlaygroundManager.IsPlaygroundSession)
            {
                Debug.Log("[ShopPhaseManager] Leaving Playground shop. Returning to PlaygroundScene.");
                PlaygroundManager.EndShopSession(PlaygroundManager.GetCachedTokens());
                SceneLoader.LoadScene("PlaygroundScene");
                return;
            }

            Debug.Log("[ShopPhaseManager] Leaving Shop. Transitioning to PreBossDialog...");
            GameFlowManager.Instance.ChangeState(GameState.PreBossDialogue);
        }
    }

    /// <summary>
    /// Thin ITokenSource proxy that reads/writes the static PlaygroundManager token cache.
    /// Used when Shop scene loads during a playground session (PlaygroundManager doesn't exist yet).
    /// </summary>
    public class PlaygroundTokenProxy : ITokenSource
    {
        public int GetTokens() => PlaygroundManager.GetCachedTokens();
        public bool SpendTokens(int amount) => PlaygroundManager.SpendCachedTokens(amount);
    }
}
