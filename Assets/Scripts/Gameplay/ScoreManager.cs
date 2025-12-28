using UnityEngine;
using CookieGame.Core;
using CookieGame.Data;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Manages game score and accuracy calculation
    /// Follows Single Responsibility Principle
    /// GDD V2.0: 100 points per round, 500 for correct answer, multipliers based on accuracy
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private GameConfig _config;
        private EventManager _eventManager;

        private int _score;
        private int _totalQuestions;
        private int _correctAnswers;
        private float _questionStartTime;

        // GDD V2.0: New scoring constants
        private const int POINTS_PER_ROUND = 100;
        private const int POINTS_PER_CORRECT_ANSWER = 500;
        private const float MULTIPLIER_PERFECT = 2.0f;    // 100% accuracy
        private const float MULTIPLIER_GOOD = 1.5f;       // >80% accuracy
        private const float MULTIPLIER_NORMAL = 1.0f;     // <80% accuracy

        public void Initialize()
        {
            _config = Resources.Load<GameConfig>("GameConfig");
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }

            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            _score = 0;
            _totalQuestions = 0;
            _correctAnswers = 0;

            // Subscribe to question generated to track time
            _eventManager.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            _questionStartTime = Time.time;
        }

        /// <summary>
        /// GDD V2.0: Adds 100 points for each distribution round
        /// </summary>
        public void AddRoundScore(int points)
        {
            _score += points;

            // Publish round score event
            _eventManager.Publish(new RoundScoreAddedEvent
            {
                roundScore = points,
                totalScore = _score
            });

            Debug.Log($"Round Score Added: +{points} points (Total: {_score})");
        }

        /// <summary>
        /// GDD V2.0: Adds 500 points for correct answer with accuracy multiplier
        /// </summary>
        /// <param name="basePoints">Base points (500)</param>
        /// <param name="accuracy">Accuracy from 0.0 to 1.0</param>
        public void AddAnswerScore(int basePoints, float accuracy)
        {
            _correctAnswers++;
            _totalQuestions++;

            // Determine multiplier based on accuracy
            float multiplier = GetMultiplier(accuracy);

            // Calculate final points
            int finalPoints = Mathf.RoundToInt(basePoints * multiplier);
            _score += finalPoints;

            Debug.Log($"Answer Score: {basePoints} x {multiplier:F1} = {finalPoints} points (Accuracy: {accuracy:P0})");

            PublishScoreUpdate(multiplier);
        }

        /// <summary>
        /// GDD V2.0: Determines score multiplier based on accuracy
        /// 100% = 2x, >80% = 1.5x, <80% = 1x
        /// </summary>
        private float GetMultiplier(float accuracy)
        {
            if (accuracy >= 1.0f)
                return MULTIPLIER_PERFECT;
            else if (accuracy > 0.8f)
                return MULTIPLIER_GOOD;
            else
                return MULTIPLIER_NORMAL;
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// GDD V2.0 uses AddAnswerScore() instead
        /// </summary>
        public void AddCorrectAnswer()
        {
            // For backward compatibility, use perfect accuracy multiplier
            AddAnswerScore(POINTS_PER_CORRECT_ANSWER, 1.0f);
        }

        /// <summary>
        /// Records wrong answer (no points)
        /// </summary>
        public void AddWrongAnswer()
        {
            _totalQuestions++;
            PublishScoreUpdate(1.0f);
        }

        private void PublishScoreUpdate(float multiplier = 1.0f)
        {
            _eventManager.Publish(new ScoreUpdatedEvent
            {
                newScore = _score,
                totalQuestions = _totalQuestions,
                correctAnswers = _correctAnswers,
                multiplier = multiplier
            });
        }

        /// <summary>
        /// Gets current score
        /// </summary>
        public int GetScore() => _score;

        /// <summary>
        /// Sets the score directly (for simple game loop)
        /// </summary>
        public void SetScore(int score)
        {
            _score = score;
            PublishScoreUpdate(1.0f);
        }

        /// <summary>
        /// Calculates accuracy percentage
        /// </summary>
        public float GetAccuracy()
        {
            if (_totalQuestions == 0) return 0f;
            return (float)_correctAnswers / _totalQuestions;
        }

        public int GetCorrectAnswers() => _correctAnswers;
        public int GetTotalQuestions() => _totalQuestions;

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
        }
    }
}
