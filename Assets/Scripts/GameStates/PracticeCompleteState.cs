using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Practice mode completion state
    /// Shows results without pressure
    /// </summary>
    public class PracticeCompleteState : GameState
    {
        private readonly int _finalScore;
        private readonly float _accuracy;

        private EventManager _eventManager;
        private UIManager _uiManager;

        public PracticeCompleteState(int finalScore, float accuracy)
        {
            _finalScore = finalScore;
            _accuracy = accuracy;
        }

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();

            _uiManager?.ShowScreen("PracticeCompleteScreen");
            _uiManager?.UpdatePracticeResults(_finalScore, _accuracy);

            Debug.Log($"Practice Complete - Score: {_finalScore}, Accuracy: {_accuracy:P}");
        }

        public void OnTryAgain()
        {
            stateManager.ChangeState(new GameplayState(true));
        }

        public void OnMainMenu()
        {
            stateManager.ChangeState(new MainMenuState());
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("PracticeCompleteScreen");
            Debug.Log("Exited Practice Complete State");
        }
    }
}
