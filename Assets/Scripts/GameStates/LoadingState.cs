using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Loading state - simulates loading game assets
    /// In production, this would load actual assets asynchronously
    /// </summary>
    public class LoadingState : GameState
    {
        private const float LOADING_DURATION = 1.5f;
        private float _timer;
        private EventManager _eventManager;
        private UIManager _uiManager;

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();

            _timer = LOADING_DURATION;
            _uiManager?.ShowScreen("LoadingScreen");

            Debug.Log("Entered Loading State");
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;

            // Update loading progress (simulated)
            float progress = 1f - (_timer / LOADING_DURATION);
            _uiManager?.UpdateLoadingProgress(progress);

            if (_timer <= 0f)
            {
                // Transition directly to main menu (Student Name screen removed)
                stateManager.ChangeState(new MainMenuState());
            }
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("LoadingScreen");
            Debug.Log("Exited Loading State");
        }
    }
}
