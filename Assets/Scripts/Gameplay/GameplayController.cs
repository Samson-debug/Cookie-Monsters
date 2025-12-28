using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CookieGame.Core;
using CookieGame.Data;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Main gameplay controller - handles all gameplay logic
    /// Simple and straightforward for a small game
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonsterController[] _allMonsters; // All 5 monsters
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private Button _submitButton; // Submit button for answers

        private EventManager _eventManager;
        private GameConfig _config;
        private LivesManager _livesManager;

        // Current question data
        private int _currentQuestionNumber;
        private int _currentDividend;
        private int _currentDivisor;
        private int _currentQuotient;
        private int _totalScore;
        private int _totalQuestions;

        // Track used questions to prevent repeats (for random generation)
        private HashSet<(int dividend, int divisor)> _usedQuestions = new HashSet<(int dividend, int divisor)>();

        // Track cookie drops per monster for validation
        private Dictionary<int, int> _cookieDropsPerMonster = new Dictionary<int, int>();
        
        // Track which unique monsters have received cookies (player can choose any monsters)
        private HashSet<int> _monstersWithCookies = new HashSet<int>();

        public void Initialize()
        {
            // Cancel any pending invokes from previous game
            CancelInvoke();
            
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _config = Resources.Load<GameConfig>("GameConfig");

            if (_config == null)
            {
                Debug.LogWarning("GameConfig not found! Using default values.");
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }

            // Find components
            if (_scoreManager == null)
                _scoreManager = FindObjectOfType<ScoreManager>();

            if (_livesManager == null)
                _livesManager = FindObjectOfType<LivesManager>();

            if (_allMonsters == null || _allMonsters.Length == 0)
            {
                _allMonsters = FindObjectsOfType<MonsterController>();
            }

            // Unsubscribe first to avoid duplicate subscriptions on restart
            _eventManager.Unsubscribe<CookieDroppedOnMonsterEvent>(OnCookieDroppedOnMonster);
            // Subscribe to cookie drop events for validation
            _eventManager.Subscribe<CookieDroppedOnMonsterEvent>(OnCookieDroppedOnMonster);

            // Reset game state
            _currentQuestionNumber = 0;
            _totalScore = 0;
            _usedQuestions.Clear();
            _cookieDropsPerMonster.Clear();
            _monstersWithCookies.Clear();

            // Determine total questions
            if (NewQuestionGenerator.Instance != null && NewQuestionGenerator.Instance.GetTotalQuestions() > 0)
            {
                _totalQuestions = NewQuestionGenerator.Instance.GetTotalQuestions();
                NewQuestionGenerator.Instance.Reset();
            }
            else
            {
                _totalQuestions = _config.totalQuestions;
            }

            // Start first question
            StartNewQuestion();
        }

        /// <summary>
        /// Starts a new question - simplified logic
        /// </summary>
        private void StartNewQuestion()
        {
            _currentQuestionNumber++;

            Debug.Log($"=== Starting Question {_currentQuestionNumber} of {_totalQuestions} ===");

            // Check if game is over
            if (_currentQuestionNumber > _totalQuestions)
            {
                OnGameOver();
                return;
            }

            // Get next question (from submitted or generate random)
            var question = GetNextQuestion();
            if (!question.HasValue)
            {
                Debug.Log("No more questions available. Ending game.");
                OnGameOver();
                return;
            }

            _currentDividend = question.Value.dividend;
            _currentDivisor = question.Value.divisor;
            _currentQuotient = _currentDividend / _currentDivisor;

            Debug.Log($"Question {_currentQuestionNumber}: {_currentDividend} ÷ {_currentDivisor} = {_currentQuotient}");

            // Show all monsters (all 5 monsters stay visible)
            SetActiveMonsters(_currentDivisor);
            ResetAllMonsters();
            
            // Reset cookie drop tracking for new question
            _cookieDropsPerMonster.Clear();
            _monstersWithCookies.Clear();

            // Publish event for other systems
            _eventManager.Publish(new QuestionGeneratedEvent
            {
                dividend = _currentDividend,
                divisor = _currentDivisor,
                correctAnswer = _currentQuotient
            });

            // Make submit button interactable for new question
            SetSubmitButtonInteractable(true);
        }

        /// <summary>
        /// Gets next question - checks submitted questions first, then generates random
        /// Prevents question repeats by tracking used questions
        /// </summary>
        private (int dividend, int divisor)? GetNextQuestion()
        {
            // Check submitted questions first
            if (NewQuestionGenerator.Instance != null && NewQuestionGenerator.Instance.HasMoreQuestions())
            {
                return NewQuestionGenerator.Instance.GetNextQuestion();
            }

            // Generate random question (prevent repeats)
            if (_config.allowedDivisors == null || _config.allowedDivisors.Length == 0)
            {
                return null;
            }

            // Try to generate a unique question (max 50 attempts to avoid infinite loop)
            int maxAttempts = 50;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int divisor = _config.allowedDivisors[Random.Range(0, _config.allowedDivisors.Length)];
                int minQuotient = Mathf.CeilToInt((float)_config.minDividend / divisor);
                int maxQuotient = Mathf.Min(6, Mathf.FloorToInt((float)_config.maxDividend / divisor));
                int quotient = Random.Range(minQuotient, maxQuotient + 1);
                int dividend = quotient * divisor;

                var question = (dividend, divisor);

                // Check if this question hasn't been used yet
                if (!_usedQuestions.Contains(question))
                {
                    _usedQuestions.Add(question);
                    return question;
                }
            }

            // If we couldn't find a unique question after max attempts, clear used set and generate one
            Debug.LogWarning("Could not generate unique question after max attempts. Clearing used questions and generating new one.");
            _usedQuestions.Clear();
            
            int finalDivisor = _config.allowedDivisors[Random.Range(0, _config.allowedDivisors.Length)];
            int finalMinQuotient = Mathf.CeilToInt((float)_config.minDividend / finalDivisor);
            int finalMaxQuotient = Mathf.Min(6, Mathf.FloorToInt((float)_config.maxDividend / finalDivisor));
            int finalQuotient = Random.Range(finalMinQuotient, finalMaxQuotient + 1);
            int finalDividend = finalQuotient * finalDivisor;

            var finalQuestion = (finalDividend, finalDivisor);
            _usedQuestions.Add(finalQuestion);
            return finalQuestion;
        }

        /// <summary>
        /// Regenerates the current question (used when wrong answer is submitted)
        /// Clears cookies, resets monsters, and respawns cookies for the same question
        /// </summary>
        private void RegenerateCurrentQuestion()
        {
            Debug.Log($"Regenerating current question: {_currentDividend} ÷ {_currentDivisor} = {_currentQuotient}");

            // Reset monsters
            ResetAllMonsters();

            // Publish question generated event again - this will trigger cookie spawners to clear and respawn
            _eventManager.Publish(new QuestionGeneratedEvent
            {
                dividend = _currentDividend,
                divisor = _currentDivisor,
                correctAnswer = _currentQuotient
            });
        }

        /// <summary>
        /// Shows all monsters (all 5 monsters stay visible)
        /// Player can only give cookies to the first 'divisor' number of monsters
        /// </summary>
        private void SetActiveMonsters(int divisor)
        {
            if (_allMonsters == null || _allMonsters.Length == 0)
            {
                Debug.LogError("No monsters assigned!");
                return;
            }

            // Show all monsters - all 5 monsters stay visible
            for (int i = 0; i < _allMonsters.Length; i++)
            {
                if (_allMonsters[i] != null)
                {
                    // All monsters are visible
                    _allMonsters[i].gameObject.SetActive(true);
                    _allMonsters[i].SetMonsterIndex(i);
                    Debug.Log($"Monster {i} ENABLED (All monsters visible)");
                }
            }
        }

        /// <summary>
        /// Resets cookie counts for all monsters
        /// </summary>
        private void ResetAllMonsters()
        {
            foreach (var monster in _allMonsters)
            {
                if (monster != null && monster.gameObject.activeInHierarchy)
                {
                    monster.ResetCookies();
                }
            }
        }

        /// <summary>
        /// Called when a cookie is dropped on a monster
        /// Validates if the drop is allowed - player can choose any monsters, but only up to 'divisor' number of different monsters
        /// If invalid (cookie dropped on a new monster when already 'divisor' monsters have cookies), player loses a life
        /// </summary>
        private void OnCookieDroppedOnMonster(CookieDroppedOnMonsterEvent evt)
        {
            // Find the monster that received the cookie
            MonsterController targetMonster = null;
            foreach (var monster in _allMonsters)
            {
                if (monster != null && monster.GetMonsterId() == evt.monsterId)
                {
                    targetMonster = monster;
                    break;
                }
            }

            if (targetMonster == null)
            {
                Debug.LogWarning($"Monster with ID {evt.monsterId} not found!");
                return;
            }

            int monsterIndex = targetMonster.GetMonsterId();

            // Check if this is a new monster (one that hasn't received cookies yet)
            bool isNewMonster = !_monstersWithCookies.Contains(monsterIndex);

            // If it's a new monster and we've already given cookies to 'divisor' number of different monsters, it's wrong
            if (isNewMonster && _monstersWithCookies.Count >= _currentDivisor)
            {
                Debug.Log($"✗ WRONG! Cookie dropped on monster {monsterIndex}, but player has already given cookies to {_monstersWithCookies.Count} different monsters (allowed: {_currentDivisor}). Player loses a life.");

                // Disable submit button since too many monsters received cookies
                SetSubmitButtonInteractable(false);

                // Show sad reaction on all monsters
                ShowAllMonstersReaction(false);

                // Publish wrong answer event (GameplayState will handle losing a life)
                _eventManager.Publish(new AnswerSubmittedEvent
                {
                    isCorrect = false,
                    submittedAnswer = -1,
                    correctAnswer = _currentQuotient,
                    timeTaken = 0f
                });

                // Wait a bit, then start next question
                Invoke(nameof(StartNewQuestion), 1.5f);
                return;
            }

            // Track cookie drop for valid monsters
            if (!_cookieDropsPerMonster.ContainsKey(monsterIndex))
            {
                _cookieDropsPerMonster[monsterIndex] = 0;
                _monstersWithCookies.Add(monsterIndex); // Mark this monster as having received cookies
            }
            _cookieDropsPerMonster[monsterIndex]++;

            // Disable submit button if player has given cookies to more monsters than divisor
            if (_monstersWithCookies.Count > _currentDivisor)
            {
                SetSubmitButtonInteractable(false);
            }

            Debug.Log($"Cookie dropped on monster {monsterIndex}. Total cookies for this monster: {_cookieDropsPerMonster[monsterIndex]}. Unique monsters with cookies: {_monstersWithCookies.Count}/{_currentDivisor}");
        }

        /// <summary>
        /// Called when player clicks Submit button
        /// Checks if exactly 'divisor' number of monsters have cookies, and each has the correct number (quotient)
        /// </summary>
        public void OnSubmitAnswer()
        {
            Debug.Log($"=== Submit Answer - Question {_currentQuestionNumber} ===");

            // Disable submit button after submission
            SetSubmitButtonInteractable(false);

            // Check if exactly 'divisor' number of monsters have received cookies
            if (_monstersWithCookies.Count != _currentDivisor)
            {
                Debug.Log($"✗ WRONG! Player gave cookies to {_monstersWithCookies.Count} monsters, but should give to exactly {_currentDivisor} monsters.");

                // Show sad reaction
                ShowAllMonstersReaction(false);

                // Publish wrong answer event (GameplayState will handle losing a life)
                _eventManager.Publish(new AnswerSubmittedEvent
                {
                    isCorrect = false,
                    submittedAnswer = -1,
                    correctAnswer = _currentQuotient,
                    timeTaken = 0f
                });

                // Wait a bit, then start next question
                Invoke(nameof(StartNewQuestion), 1.5f);
                return;
            }

            // Check each monster that received cookies has the correct count (quotient)
            bool allCorrect = true;
            foreach (int monsterIndex in _monstersWithCookies)
            {
                if (_allMonsters[monsterIndex] != null)
                {
                    int cookieCount = _allMonsters[monsterIndex].GetCookieCount();
                    Debug.Log($"Monster {monsterIndex}: Has {cookieCount} cookies (Expected: {_currentQuotient})");

                    if (cookieCount != _currentQuotient)
                    {
                        allCorrect = false;
                    }
                }
            }

            // Award points
            if (allCorrect)
            {
                // CORRECT - All monsters have quotient cookies
                int points = _config.pointsPerCorrectAnswer;
                _totalScore += points;

                Debug.Log($"✓ CORRECT! Each monster has {_currentQuotient} cookies.");
                Debug.Log($"+{points} points! Total Score: {_totalScore}");

                // Show happy reaction
                ShowAllMonstersReaction(true);

                // Publish correct answer event
                _eventManager.Publish(new AnswerSubmittedEvent
                {
                    isCorrect = true,
                    submittedAnswer = _currentQuotient,
                    correctAnswer = _currentQuotient,
                    timeTaken = 0f
                });
                
                if (_scoreManager != null)
                {
                    _scoreManager.SetScore(_totalScore);
                }
            }
            else
            {
                Debug.Log($"✗ WRONG! Correct answer: Each monster should have {_currentQuotient} cookies.");

                // Show sad reaction
                ShowAllMonstersReaction(false);

                // Publish wrong answer event
                _eventManager.Publish(new AnswerSubmittedEvent
                {
                    isCorrect = false,
                    submittedAnswer = -1, // No specific answer since manual collection
                    correctAnswer = _currentQuotient,
                    timeTaken = 0f
                });
            }

            // Wait a bit, then start next question (only on correct answer)
            Invoke(nameof(StartNewQuestion), 1.5f);
        }

        public void OnGameOver()
        {
            Debug.Log($"GAME OVER! Final Score: {_totalScore}");

            float accuracy = _totalQuestions > 0 ? (float)_totalScore / (_totalQuestions * _config.pointsPerCorrectAnswer) : 0f;

            _eventManager.Publish(new GameOverEvent
            {
                finalScore = _totalScore,
                accuracy = accuracy
            });
        }
        
        /// <summary>
        /// Shows happy or sad reaction on all active monsters
        /// </summary>
        private void ShowAllMonstersReaction(bool isHappy)
        {
            Debug.Log($"[GameplayController] Trigger monsters {(isHappy ? "happy" : "sad")} animation.");
            foreach (var monster in _allMonsters)
            {
                if(!monster.gameObject.activeSelf) continue;
                
                if (isHappy)
                {
                    monster.ShowHappyReaction();
                }
                else
                {
                    monster.ShowSadReaction();
                }
            }
        }

        /// <summary>
        /// Sets the interactability of the submit button
        /// </summary>
        private void SetSubmitButtonInteractable(bool interactable)
        {
            if (_submitButton != null)
            {
                _submitButton.interactable = interactable;
            }
        }

        private void OnDestroy()
        {
            if (_eventManager != null)
            {
                _eventManager.Unsubscribe<CookieDroppedOnMonsterEvent>(OnCookieDroppedOnMonster);
            }
        }
        public int GetCurrentScore() => _totalScore;
        
        #region Useless code
    
        // public int GetCurrentQuestionNumber() => _currentQuestionNumber;
        // public int GetTotalQuestions() => _totalQuestions;
        #endregion
    }
}
