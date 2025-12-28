using UnityEngine;

namespace CookieGame.Data
{
    /// <summary>
    /// Main game configuration ScriptableObject
    /// Follows Open/Closed Principle - config can be extended without code changes
    /// Allows game designers to modify game parameters without touching code
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Cookie Game/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        [Tooltip("Total number of questions per game")]
        public int totalQuestions = 3;

        [Tooltip("Maximum number of monsters (divisor limit)")]
        public int maxMonsters = 5;

        [Tooltip("Time limit for test mode (in seconds)")]
        public float testModeTimeLimit = 60f;

        [Tooltip("Time limit for practice mode (in seconds, 0 = unlimited)")]
        public float practiceModeTimeLimit = 120f;

        [Tooltip("Number of lives in test mode")]
        public int testModeLives = 3;

        [Tooltip("Number of lives in practice mode (0 = unlimited)")]
        public int practiceModeLives = 5;

        [Header("Scoring")]
        [Tooltip("Points for correct answer")]
        public int pointsPerCorrectAnswer = 10;

        [Tooltip("Points for wrong answer")]
        public int pointsPerWrongAnswer = 0;

        [Tooltip("Bonus points for fast answers (< 5 seconds)")]
        public int fastAnswerBonus = 50;

        [Tooltip("Time threshold for fast answer bonus (in seconds)")]
        public float fastAnswerThreshold = 5f;

        [Header("Question Generation")]
        [Tooltip("Minimum dividend value")]
        public int minDividend = 4;

        [Tooltip("Maximum dividend value")]
        public int maxDividend = 20;

        [Tooltip("Allowed divisors (must be <= maxMonsters and evenly divide)")]
        public int[] allowedDivisors = { 2, 3, 4, 5 };

        [Header("Animation Settings")]
        [Tooltip("Cookie fall duration when distributed")]
        public float cookieFallDuration = 0.5f;

        [Tooltip("Monster eating animation duration")]
        public float monsterEatDuration = 0.8f;

        [Tooltip("Monster reaction animation duration")]
        public float monsterReactionDuration = 1f;

        [Header("Audio Settings")]
        [Range(0f, 1f)]
        [Tooltip("Master volume")]
        public float masterVolume = 0.8f;

        [Range(0f, 1f)]
        [Tooltip("SFX volume")]
        public float sfxVolume = 0.7f;

        [Range(0f, 1f)]
        [Tooltip("Music volume")]
        public float musicVolume = 0.5f;

        /// <summary>
        /// Gets time limit based on game mode
        /// </summary>
        public float GetTimeLimit(bool isPracticeMode)
        {
            return isPracticeMode ? practiceModeTimeLimit : testModeTimeLimit;
        }

        /// <summary>
        /// Gets number of lives based on game mode
        /// </summary>
        public int GetLives(bool isPracticeMode)
        {
            return isPracticeMode ? practiceModeLives : testModeLives;
        }

        /// <summary>
        /// Validates configuration on enable
        /// </summary>
        private void OnValidate()
        {
            // Ensure valid ranges
            testModeTimeLimit = Mathf.Max(10f, testModeTimeLimit);
            practiceModeTimeLimit = Mathf.Max(0f, practiceModeTimeLimit);
            testModeLives = Mathf.Max(1, testModeLives);
            practiceModeLives = Mathf.Max(0, practiceModeLives);
            minDividend = Mathf.Max(2, minDividend);
            maxDividend = Mathf.Max(minDividend, maxDividend);
            pointsPerCorrectAnswer = Mathf.Max(0, pointsPerCorrectAnswer);

            // Ensure at least one divisor
            if (allowedDivisors == null || allowedDivisors.Length == 0)
            {
                allowedDivisors = new int[] { 2, 3, 4, 5 };
            }
        }
    }
}
