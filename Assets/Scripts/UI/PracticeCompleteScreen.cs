using UnityEngine;
using UnityEngine.UI;
using CookieGame.Core;
using CookieGame.GameStates;
using CookieGame.Patterns;
using TMPro;
namespace CookieGame.UI
{
    /// <summary>
    /// Practice mode completion screen
    /// </summary>
    public class PracticeCompleteScreen : UIScreen
    {
        [Header("Results")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Buttons")]
        [SerializeField] private Button _tryAgainButton;
        [SerializeField] private Button _mainMenuButton;

        public override void Initialize()
        {
            base.Initialize();

            if (_tryAgainButton != null)
                _tryAgainButton.onClick.AddListener(OnTryAgainClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void UpdateResults(int score, float accuracy)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }

            if (_accuracyText != null)
            {
                _accuracyText.text = $"Accuracy: {accuracy:P0}";
            }

            // Motivational message based on accuracy
            if (_messageText != null)
            {
                if (accuracy >= 0.9f)
                    _messageText.text = "Excellent! You're ready for the test!";
                else if (accuracy >= 0.7f)
                    _messageText.text = "Great job! Keep practicing!";
                else if (accuracy >= 0.5f)
                    _messageText.text = "Good effort! Try again to improve!";
                else
                    _messageText.text = "Keep trying! Practice makes perfect!";
            }
        }

        private void OnTryAgainClicked()
        {
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as PracticeCompleteState;
                currentState?.OnTryAgain();
            }
        }

        private void OnMainMenuClicked()
        {
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as PracticeCompleteState;
                currentState?.OnMainMenu();
            }
        }
    }
}
