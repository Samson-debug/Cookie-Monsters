using UnityEngine;
using UnityEngine.UI;
using CookieGame.Core;
using CookieGame.GameStates;
using CookieGame.Patterns;
using TMPro;

namespace CookieGame.UI
{
    /// <summary>
    /// Game over screen showing final results
    /// </summary>
    public class GameOverScreen : UIScreen
    {
        [Header("Results")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private GameObject _newHighScoreIndicator;
        [SerializeField] private TextMeshProUGUI _gradeText;

        [Header("Buttons")]
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _mainMenuButton;

        public override void Initialize()
        {
            base.Initialize();

            if (_playAgainButton != null)
                _playAgainButton.onClick.AddListener(OnPlayAgainClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void UpdateResults(int score, float accuracy, int highScore)
        {
            bool isNewHighScore = score > highScore;

            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }

            if (_accuracyText != null)
            {
                _accuracyText.text = $"Accuracy: {accuracy:P0}";
            }

            if (_highScoreText != null)
            {
                _highScoreText.text = $"High Score: {(isNewHighScore ? score : highScore)}";
            }

            if (_newHighScoreIndicator != null)
            {
                _newHighScoreIndicator.SetActive(isNewHighScore);
            }

            // Calculate grade
            if (_gradeText != null)
            {
                string grade = CalculateGrade(accuracy);
                _gradeText.text = $"Grade: {grade}";
            }
        }

        private string CalculateGrade(float accuracy)
        {
            if (accuracy >= 0.95f) return "A+";
            if (accuracy >= 0.9f) return "A";
            if (accuracy >= 0.85f) return "B+";
            if (accuracy >= 0.8f) return "B";
            if (accuracy >= 0.75f) return "C+";
            if (accuracy >= 0.7f) return "C";
            if (accuracy >= 0.6f) return "D";
            return "F";
        }

        public void OnPlayAgainClicked()
        {
            Debug.Log($"[GameOverScreen] Play again btn pressed!");
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as GameOverState;
                currentState?.OnPlayAgain();
            }
        }

        private void OnMainMenuClicked()
        {
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as GameOverState;
                currentState?.OnMainMenu();
            }
        }

        public void TestFunc()
        {
            Debug.Log("[GameOverScreen] Test Func ran!");
        }
    }
}
