using UnityEngine;
using CookieGame.Core;
using CookieGame.Data;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Generates division questions for the game
    /// Follows Single Responsibility Principle - only handles question generation
    /// </summary>
    public class QuestionGenerator : MonoBehaviour
    {
        private GameConfig _config;
        private EventManager _eventManager;
        private int _currentDividend;
        private int _currentDivisor;
        private int _currentAnswer;

        public void Initialize()
        {
            _config = Resources.Load<GameConfig>("GameConfig");
            if (_config == null)
            {
                Debug.LogWarning("GameConfig not found in Resources folder. Using default values.");
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
        }

        /// <summary>
        /// Generates a new division question
        /// Uses submitted questions if available, otherwise generates random questions
        /// Ensures the answer is always a whole number
        /// Quotient is limited to maximum 6 for gameplay balance
        /// </summary>
        /// <returns>True if a question was generated, false if no more questions available</returns>
        public bool GenerateNewQuestion()
        {
            // Check if there are submitted questions to use
            if (NewQuestionGenerator.Instance != null && NewQuestionGenerator.Instance.HasMoreQuestions())
            {
                var question = NewQuestionGenerator.Instance.GetNextQuestion();
                if (question.HasValue)
                {
                    _currentDividend = question.Value.dividend;
                    _currentDivisor = question.Value.divisor;
                    _currentAnswer = _currentDividend / _currentDivisor; // Calculate quotient

                    // Publish event
                    _eventManager.Publish(new QuestionGeneratedEvent
                    {
                        dividend = _currentDividend,
                        divisor = _currentDivisor,
                        correctAnswer = _currentAnswer
                    });

                    Debug.Log($"Using Submitted Question: {_currentDividend} ÷ {_currentDivisor} = {_currentAnswer}");
                    return true;
                }
            }

            // Fall back to random generation (for practice mode or if no questions submitted)
            // Select random divisor
            _currentDivisor = _config.allowedDivisors[Random.Range(0, _config.allowedDivisors.Length)];

            // Calculate answer (quotient) - LIMIT TO MAX 6
            int minQuotient = Mathf.CeilToInt((float)_config.minDividend / _currentDivisor);
            int maxQuotient = Mathf.Min(6, Mathf.FloorToInt((float)_config.maxDividend / _currentDivisor));

            _currentAnswer = Random.Range(minQuotient, maxQuotient + 1);

            // Calculate dividend (must be evenly divisible)
            _currentDividend = _currentAnswer * _currentDivisor;

            // Publish event
            _eventManager.Publish(new QuestionGeneratedEvent
            {
                dividend = _currentDividend,
                divisor = _currentDivisor,
                correctAnswer = _currentAnswer
            });

            Debug.Log($"Generated Random Question: {_currentDividend} ÷ {_currentDivisor} = {_currentAnswer} (Quotient ≤ 6)");
            return true;
        }

        /// <summary>
        /// Checks if the submitted answer is correct
        /// </summary>
        public bool CheckAnswer(int submittedAnswer)
        {
            bool isCorrect = submittedAnswer == _currentAnswer;

            _eventManager.Publish(new AnswerSubmittedEvent
            {
                isCorrect = isCorrect,
                submittedAnswer = submittedAnswer,
                correctAnswer = _currentAnswer,
                timeTaken = 0f // Can be tracked if needed
            });

            return isCorrect;
        }

        public int GetCurrentDividend() => _currentDividend;
        public int GetCurrentDivisor() => _currentDivisor;
        public int GetCorrectAnswer() => _currentAnswer;
    }
}
