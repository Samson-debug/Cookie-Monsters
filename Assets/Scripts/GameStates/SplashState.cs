using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Splash screen state - shows game logo
    /// Follows State Pattern for clean state transitions
    /// </summary>
    public class SplashState : GameState
    {
        private const float SPLASH_DURATION = 2f;
        private float _timer;
        private EventManager _eventManager;
        private UIManager _uiManager;

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();

            _timer = SPLASH_DURATION;
            _uiManager?.ShowScreen("SplashScreen");

            Debug.Log("Entered Splash State");
        }

        public override void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                // Transition to loading state
                stateManager.ChangeState(new LoadingState());
            }
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("SplashScreen");
            Debug.Log("Exited Splash State");
        }
    }
}
