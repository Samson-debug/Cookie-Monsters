using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;
using CookieGame.Audio;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Question submission state - allows player to submit questions before test mode
    /// </summary>
    public class QuestionSubmissionState : GameState
    {
        private EventManager _eventManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            // Reset questions when entering submission screen
            if (CookieGame.Gameplay.NewQuestionGenerator.Instance != null)
            {
                CookieGame.Gameplay.NewQuestionGenerator.Instance.ClearQuestions();
                CookieGame.Gameplay.NewQuestionGenerator.Instance.Reset();
            }

            _uiManager?.ShowScreen("QuestionSubmissionScreen");
            _audioManager?.PlayMusic("MenuMusic");

            Debug.Log("Entered Question Submission State");
        }

        public void OnQuestionsSubmitted()
        {
            // Hide question submission screen
            _uiManager?.HideScreen("QuestionSubmissionScreen");
            
            // Start test mode gameplay
            stateManager.ChangeState(new GameplayState(false));
        }

        public void OnBackToMenu()
        {
            stateManager.ChangeState(new MainMenuState());
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("QuestionSubmissionScreen");
            Debug.Log("Exited Question Submission State");
        }
    }
}
