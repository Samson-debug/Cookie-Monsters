using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;
using CookieGame.Audio;
using CookieGame.Gameplay;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Main gameplay state - handles UI, audio, and state transitions
    /// Gameplay logic is handled by GameplayController
    /// </summary>
    public class GameplayState : GameState
    {
        private readonly bool _isPracticeMode;

        private EventManager _eventManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;

        private ScoreManager _scoreManager;
        private LivesManager _livesManager;
        private TimerManager _timerManager;
        private GameplayController _gameplayController;
        private bool _gameEnded = false;

        public GameplayState(bool isPracticeMode)
        {
            _isPracticeMode = isPracticeMode;
        }

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            // Subscribe to events
            _eventManager.Subscribe<AnswerSubmittedEvent>(OnAnswerSubmitted);
            _eventManager.Subscribe<TimerExpiredEvent>(OnTimerExpired);
            _eventManager.Subscribe<LivesDepletedEvent>(OnLivesDepleted);
            _eventManager.Subscribe<GameOverEvent>(OnGameOver);

            _gameEnded = false;
            InitializeGameplay();

            _uiManager?.ShowScreen("GameplayScreen");
            _audioManager?.PlayMusic("GameplayMusic");

            // Start the timer UI when gameplay state is entered
            var timerUI = Object.FindObjectOfType<TimerUI>();
            if (timerUI != null)
            {
                timerUI.StartTimer();
            }

            Debug.Log($"Entered Gameplay State - Practice Mode: {_isPracticeMode}");
        }

        private void InitializeGameplay()
        {
            // Get or create gameplay controller
            var gameplayObject = GameObject.Find("GameplayController");
            if (gameplayObject == null)
            {
                gameplayObject = new GameObject("GameplayController");
            }

            _gameplayController = gameplayObject.GetComponent<GameplayController>();
            if (_gameplayController == null)
            {
                _gameplayController = gameplayObject.AddComponent<GameplayController>();
            }

            // Initialize other managers if they exist
            _scoreManager = gameplayObject.GetComponent<ScoreManager>();
            if (_scoreManager != null)
            {
                _scoreManager.Initialize();
            }

            _livesManager = gameplayObject.GetComponent<LivesManager>();
            if (_livesManager == null)
            {
                _livesManager = gameplayObject.AddComponent<LivesManager>();
            }
            // Always initialize to reset lives when starting a new game
            _livesManager.Initialize(_isPracticeMode);

            _timerManager = gameplayObject.GetComponent<TimerManager>();
            if (_timerManager != null)
            {
                _timerManager.Initialize(_isPracticeMode);
            }

            // Initialize main gameplay controller (this starts the game)
            _gameplayController.Initialize();
        }

        private void OnAnswerSubmitted(AnswerSubmittedEvent eventData)
        {
            // Handle audio/visual feedback
            if (eventData.isCorrect)
            {
                _audioManager?.PlaySFX("CorrectAnswer");
                _uiManager?.ShowVFX("CorrectVFX", Vector3.zero);
                if (_scoreManager != null)
                {
                    _scoreManager.AddCorrectAnswer();
                }
            }
            else
            {
                _audioManager?.PlaySFX("WrongAnswer");
                _uiManager?.ShowVFX("WrongVFX", Vector3.zero);
                if (_scoreManager != null)
                {
                    _scoreManager.AddWrongAnswer();
                }
                if (_livesManager != null)
                {
                    _livesManager.LoseLife();
                }
            }

            // Check if game should continue
            bool shouldContinue = true;
            if (_livesManager != null && !_livesManager.HasLives())
            {
                shouldContinue = false;
            }
            if (_timerManager != null && !_timerManager.HasTime())
            {
                shouldContinue = false;
            }

            if (!shouldContinue && !_gameEnded)
            {
                // Stop the timer when game ends
                var timerUI = Object.FindObjectOfType<TimerUI>();
                if (timerUI != null)
                {
                    timerUI.StopTimer();
                }
                EndGameplay();
            }
        }

        private void OnTimerExpired(TimerExpiredEvent eventData)
        {
            EndGameplay();
        }

        private void OnLivesDepleted(LivesDepletedEvent eventData)
        {
            if (_gameEnded) return;
            
            // Stop the timer when lives are depleted
            var timerUI = Object.FindObjectOfType<TimerUI>();
            if (timerUI != null)
            {
                timerUI.StopTimer();
            }
            EndGameplay();
        }

        private void OnGameOver(GameOverEvent eventData)
        {
            // Transition to game over state using event data
            if (_isPracticeMode)
            {
                stateManager.ChangeState(new PracticeCompleteState(eventData.finalScore, eventData.accuracy));
            }
            else
            {
                stateManager.ChangeState(new GameOverState(eventData.finalScore, eventData.accuracy));
            }
        }

        private void EndGameplay()
        {
            if (_gameEnded) return;
            _gameEnded = true;
            
            int finalScore = 0;
            float accuracy = 0f;

            // Get score from gameplay controller or score manager
            if (_gameplayController != null)
            {
                finalScore = _gameplayController.GetCurrentScore();
            }
            else if (_scoreManager != null)
            {
                finalScore = _scoreManager.GetScore();
                accuracy = _scoreManager.GetAccuracy();
            }

            // Clear submitted questions when game ends (will be re-submitted on restart)
            if (!_isPracticeMode && NewQuestionGenerator.Instance != null)
            {
                NewQuestionGenerator.Instance.ClearQuestions();
            }

            if (_isPracticeMode)
            {
                stateManager.ChangeState(new PracticeCompleteState(finalScore, accuracy));
            }
            else
            {
                stateManager.ChangeState(new GameOverState(finalScore, accuracy));
            }
        }

        public override void Update()
        {
            // State updates handled by individual managers
        }

        public override void Exit()
        {
            // Unsubscribe from events
            _eventManager.Unsubscribe<AnswerSubmittedEvent>(OnAnswerSubmitted);
            _eventManager.Unsubscribe<TimerExpiredEvent>(OnTimerExpired);
            _eventManager.Unsubscribe<LivesDepletedEvent>(OnLivesDepleted);
            _eventManager.Unsubscribe<GameOverEvent>(OnGameOver);

            _uiManager?.HideScreen("GameplayScreen");
            _audioManager?.StopMusic();

            // Cancel any pending operations in GameplayController (don't destroy, just reset)
            if (_gameplayController != null)
            {
                _gameplayController.CancelInvoke();
            }

            Debug.Log("Exited Gameplay State");
        }
    }

    // Additional events for gameplay
    public struct TimerExpiredEvent { }
    public struct LivesDepletedEvent { }
}
