using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;
using CookieGame.Audio;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Game over state - shows final score and results
    /// Saves high score and displays achievements
    /// </summary>
    public class GameOverState : GameState
    {
        private readonly int _finalScore;
        private readonly float _accuracy;

        private EventManager _eventManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;

        public GameOverState(int finalScore, float accuracy)
        {
            _finalScore = finalScore;
            _accuracy = accuracy;
        }

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            SaveScore();

            _uiManager?.ShowScreen("GameOverScreen");
            _uiManager?.UpdateGameOverResults(_finalScore, _accuracy, GetHighScore());

            // Play different music based on performance
            if (_accuracy >= 0.8f)
            {
                _audioManager?.PlaySFX("Victory");
            }
            else
            {
                _audioManager?.PlaySFX("GameOver");
            }

            Debug.Log($"Game Over - Score: {_finalScore}, Accuracy: {_accuracy:P}");
        }

        private void SaveScore()
        {
            int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
            if (_finalScore > currentHighScore)
            {
                PlayerPrefs.SetInt("HighScore", _finalScore);
                PlayerPrefs.Save();
                Debug.Log($"New High Score: {_finalScore}");
            }
        }

        private int GetHighScore()
        {
            return PlayerPrefs.GetInt("HighScore", 0);
        }

        public void OnPlayAgain()
        {
            // For test mode, restart from question submission screen
            stateManager.ChangeState(new QuestionSubmissionState());
        }

        public void OnMainMenu()
        {
            stateManager.ChangeState(new MainMenuState());
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("GameOverScreen");
            Debug.Log("Exited Game Over State");
        }
    }
}
