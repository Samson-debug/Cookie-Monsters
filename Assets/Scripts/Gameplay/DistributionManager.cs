using UnityEngine;
using CookieGame.Core;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Manages round-based cookie distribution
    /// Follows Single Responsibility Principle - handles only distribution logic
    /// </summary>
    public class DistributionManager : MonoBehaviour
    {
        private EventManager _eventManager;
        private ScoreManager _scoreManager;

        private int _totalCookies;
        private int _remainingCookies;
        private int _divisor; // Number of monsters
        private int _currentRound;
        private bool _isDistributing;

        public void Initialize(EventManager eventManager, ScoreManager scoreManager)
        {
            _eventManager = eventManager;
            _scoreManager = scoreManager;

            // Subscribe to events
            _eventManager.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager.Subscribe<CookieDroppedOnMonsterEvent>(OnCookieDropped);
        }

        private void OnDestroy()
        {
            if (_eventManager != null)
            {
                _eventManager.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
                _eventManager.Unsubscribe<CookieDroppedOnMonsterEvent>(OnCookieDropped);
            }
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent evt)
        {
            // Initialize distribution tracking for new question
            _totalCookies = evt.dividend;
            _remainingCookies = evt.dividend;
            _divisor = evt.divisor;
            _currentRound = 0;
            _isDistributing = false;

            Debug.Log($"DistributionManager: New question - {_totalCookies} cookies, {_divisor} monsters");

            // Publish cookie pile updated event for UI
            _eventManager.Publish(new CookiePileUpdatedEvent
            {
                remainingCookies = _remainingCookies,
                totalCookies = _totalCookies
            });
        }

        private void OnCookieDropped(CookieDroppedOnMonsterEvent evt)
        {
            // In GDD V2.0, dropping ONE cookie triggers ONE COMPLETE ROUND
            if (!_isDistributing && _remainingCookies > 0)
            {
                StartDistributionRound();
            }
        }

        /// <summary>
        /// Starts a new distribution round - distributes 1 cookie to ALL monsters
        /// </summary>
        private void StartDistributionRound()
        {
            if (_isDistributing)
            {
                Debug.LogWarning("Distribution already in progress!");
                return;
            }

            // Check for remainder error BEFORE distributing
            if (_remainingCookies < _divisor)
            {
                // Remainder error - not enough cookies to distribute evenly
                _eventManager.Publish(new RemainderErrorEvent
                {
                    remainingCookies = _remainingCookies,
                    divisor = _divisor
                });

                Debug.Log($"Remainder Error: {_remainingCookies} cookies left, need {_divisor} for even distribution");

                // Show answer input UI
                _eventManager.Publish(new ShowAnswerInputEvent());
                return;
            }

            _isDistributing = true;
            _currentRound++;

            Debug.Log($"Starting Distribution Round {_currentRound}");

            // Publish round started event
            _eventManager.Publish(new DistributionRoundStartedEvent
            {
                roundNumber = _currentRound,
                remainingCookies = _remainingCookies
            });

            // Distribute 1 cookie to each monster
            //DistributeCookiesToAllMonsters();
        }

        /// <summary>
        /// Distributes 1 cookie to each monster in sequence
        /// </summary>
        private void DistributeCookiesToAllMonsters()
        {
            // Each monster gets 1 cookie
            int cookiesPerMonster = 1;

            // Deduct cookies (1 per monster)
            _remainingCookies -= _divisor;

            // Add round score (100 points per round)
            _scoreManager.AddRoundScore(100);

            // Publish event to trigger sequential animations
            _eventManager.Publish(new AllMonstersReceivedCookieEvent
            {
                roundNumber = _currentRound
            });

            Debug.Log($"Round {_currentRound}: Distributed {cookiesPerMonster} cookie to each of {_divisor} monsters. Remaining: {_remainingCookies}");

            // Complete the round
            CompleteDistributionRound();
        }

        /// <summary>
        /// Completes the current distribution round
        /// </summary>
        private void CompleteDistributionRound()
        {
            // Update cookie pile UI
            _eventManager.Publish(new CookiePileUpdatedEvent
            {
                remainingCookies = _remainingCookies,
                totalCookies = _totalCookies
            });

            // Publish round completed event
            _eventManager.Publish(new DistributionRoundCompletedEvent
            {
                roundNumber = _currentRound,
                cookiesPerMonster = 1,
                remainingCookies = _remainingCookies,
                isCorrect = true // Will be validated when answer submitted
            });

            _isDistributing = false;

            // Check if distribution is complete
            if (_remainingCookies < _divisor)
            {
                OnDistributionComplete();
            }
        }

        /// <summary>
        /// Called when distribution is complete (remainder < divisor)
        /// </summary>
        private void OnDistributionComplete()
        {
            Debug.Log($"Distribution Complete! Rounds: {_currentRound}, Remainder: {_remainingCookies}");

            // Show answer input UI
            _eventManager.Publish(new ShowAnswerInputEvent());
        }

        /// <summary>
        /// Validates the submitted quotient answer
        /// </summary>
        public bool ValidateAnswer(int submittedQuotient)
        {
            int correctQuotient = _totalCookies / _divisor;
            bool isCorrect = submittedQuotient == correctQuotient;

            Debug.Log($"Answer Validation: Submitted={submittedQuotient}, Correct={correctQuotient}, Result={isCorrect}");

            if (isCorrect)
            {
                // Calculate accuracy for multiplier
                float accuracy = CalculateAccuracy();
                _scoreManager.AddAnswerScore(500, accuracy);
            }

            return isCorrect;
        }

        /// <summary>
        /// Calculates accuracy based on rounds vs correct quotient
        /// </summary>
        private float CalculateAccuracy()
        {
            int correctQuotient = _totalCookies / _divisor;

            if (correctQuotient == 0)
                return 1f;

            // Perfect accuracy if rounds match quotient exactly
            if (_currentRound == correctQuotient)
                return 1f;

            // Calculate percentage accuracy
            float accuracy = (float)correctQuotient / _currentRound;
            return Mathf.Clamp01(accuracy);
        }

        // Public getters for UI updates
        public int GetRemainingCookies() => _remainingCookies;
        public int GetTotalCookies() => _totalCookies;
        public int GetCurrentRound() => _currentRound;
        public int GetDivisor() => _divisor;
        public bool IsDistributing() => _isDistributing;
    }
}
