using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CookieGame.Core;
using CookieGame.GameStates;
using CookieGame.Patterns;
using CookieGame.Data;

namespace CookieGame.UI
{
    /// <summary>
    /// Question submission screen - allows player to input questions before test mode
    /// </summary>
    public class QuestionSubmissionScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _addQuestionButton;
        [SerializeField] private Button _removeQuestionButton;

        [Header("Question Input Container")]
        [SerializeField] private Transform _questionInputContainer;

        [Header("Question Input Prefab")]
        [SerializeField] private GameObject _questionInputPrefab;

        [Header("Settings")]
        [SerializeField] private int _minQuestionFields = 1;
        [SerializeField] private int _initialQuestionFields = 3;

        private CookieGame.Gameplay.NewQuestionGenerator _questionGenerator;
        private List<GameObject> _questionInputFields = new List<GameObject>();
        
        // Track used questions to ensure uniqueness
        private HashSet<(int dividend, int divisor)> _usedQuestions = new HashSet<(int dividend, int divisor)>();
        private GameConfig _config;

        public override void Initialize()
        {
            base.Initialize();

            if (_submitButton != null)
                _submitButton.onClick.AddListener(OnSubmitClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);

            if (_addQuestionButton != null)
                _addQuestionButton.onClick.AddListener(OnAddQuestionClicked);

            if (_removeQuestionButton != null)
                _removeQuestionButton.onClick.AddListener(OnRemoveQuestionClicked);

            // Get NewQuestionGenerator instance
            _questionGenerator = CookieGame.Gameplay.NewQuestionGenerator.Instance;

            // Set submit button reference
            if (_questionGenerator != null && _submitButton != null)
            {
                _questionGenerator.submitBtn = _submitButton;
            }

            // Load GameConfig
            _config = Resources.Load<GameConfig>("GameConfig/GameConfig");
            if (_config == null)
            {
                Debug.LogWarning("GameConfig not found in Resources folder. Using default values.");
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }
        }

        protected override void OnShow()
        {
            base.OnShow();

            // Clear existing inputs and used questions
            ClearAllInputFields();
            _usedQuestions.Clear();

            // Create initial question input fields
            if (_questionInputContainer != null && _questionInputPrefab != null)
            {
                for (int i = 0; i < _initialQuestionFields; i++)
                {
                    AddQuestionInputField();
                }
            }

            UpdateRemoveButtonState();
        }

        private void OnSubmitClicked()
        {
            Debug.Log("Submit button clicked on Question Submission Screen");

            // Submit questions through NewQuestionGenerator
            if (_questionGenerator != null)
            {
                _questionGenerator.SubmitQuestions();
            }

            // Notify state to transition to gameplay
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as QuestionSubmissionState;
                if (currentState != null)
                {
                    currentState.OnQuestionsSubmitted();
                }
                else
                {
                    Debug.LogError("Current state is not QuestionSubmissionState!");
                }
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("Back button clicked on Question Submission Screen");

            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as QuestionSubmissionState;
                if (currentState != null)
                {
                    currentState.OnBackToMenu();
                }
            }
        }

        /// <summary>
        /// Adds a new question input field
        /// </summary>
        private void OnAddQuestionClicked()
        {
            AddQuestionInputField();
            UpdateRemoveButtonState();
            Debug.Log($"Added question input field. Total: {_questionInputFields.Count}");
        }

        /// <summary>
        /// Removes the last question input field (if above minimum)
        /// </summary>
        private void OnRemoveQuestionClicked()
        {
            if (_questionInputFields.Count > _minQuestionFields)
            {
                RemoveQuestionInputField();
                UpdateRemoveButtonState();
                Debug.Log($"Removed question input field. Total: {_questionInputFields.Count}");
            }
            else
            {
                Debug.LogWarning($"Cannot remove question field. Minimum of {_minQuestionFields} required.");
            }
        }

        /// <summary>
        /// Creates and adds a new question input field with auto-generated question
        /// </summary>
        private void AddQuestionInputField()
        {
            if (_questionInputContainer == null || _questionInputPrefab == null)
            {
                Debug.LogError("Question input container or prefab not assigned!");
                return;
            }

            GameObject inputField = Instantiate(_questionInputPrefab, _questionInputContainer);
            inputField.SetActive(true);
            _questionInputFields.Add(inputField);

            // Generate a unique division question and populate the field
            var question = GenerateUniqueDivisionQuestion();
            if (question.HasValue)
            {
                var questionInputField = inputField.GetComponent<CookieGame.Gameplay.QuestionInputField>();
                if (questionInputField != null && questionInputField.dividendTMP != null && questionInputField.divisorTMP != null)
                {
                    questionInputField.dividendTMP.text = question.Value.dividend.ToString();
                    questionInputField.divisorTMP.text = question.Value.divisor.ToString();
                    Debug.Log($"Generated question for new field: {question.Value.dividend} ÷ {question.Value.divisor}");
                }
            }
        }

        /// <summary>
        /// Removes the last question input field
        /// </summary>
        private void RemoveQuestionInputField()
        {
            if (_questionInputFields.Count > 0)
            {
                GameObject lastField = _questionInputFields[_questionInputFields.Count - 1];
                _questionInputFields.RemoveAt(_questionInputFields.Count - 1);
                
                // Remove the question from used set if it exists
                if (lastField != null)
                {
                    var questionInputField = lastField.GetComponent<CookieGame.Gameplay.QuestionInputField>();
                    if (questionInputField != null)
                    {
                        var questionData = questionInputField.GetQuestionData();
                        _usedQuestions.Remove((questionData.dividend, questionData.divisor));
                    }
                    Destroy(lastField);
                }
            }
        }

        /// <summary>
        /// Clears all question input fields
        /// </summary>
        private void ClearAllInputFields()
        {
            foreach (var field in _questionInputFields)
            {
                if (field != null)
                {
                    Destroy(field);
                }
            }
            _questionInputFields.Clear();

            // Also clear any remaining children (safety check)
            if (_questionInputContainer != null)
            {
                foreach (Transform child in _questionInputContainer)
                {
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the remove button state (enabled/disabled based on field count)
        /// </summary>
        private void UpdateRemoveButtonState()
        {
            if (_removeQuestionButton != null)
            {
                _removeQuestionButton.interactable = _questionInputFields.Count > _minQuestionFields;
            }
        }

        /// <summary>
        /// Generates a unique division question that returns a whole number using config
        /// If unique question cannot be generated after max attempts, allows repeats
        /// </summary>
        private (int dividend, int divisor)? GenerateUniqueDivisionQuestion()
        {
            if (_config == null)
            {
                Debug.LogError("GameConfig is null! Cannot generate question.");
                return null;
            }

            if (_config.allowedDivisors == null || _config.allowedDivisors.Length == 0)
            {
                Debug.LogError("No allowed divisors in config! Cannot generate question.");
                return null;
            }

            // Try to generate a unique question (max 50 attempts to avoid infinite loop)
            int maxAttempts = 50;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Select random divisor from allowed divisors
                int divisor = _config.allowedDivisors[Random.Range(0, _config.allowedDivisors.Length)];
                
                // Calculate valid quotient range (ensures dividend is within min/max and result is whole number)
                int minQuotient = Mathf.CeilToInt((float)_config.minDividend / divisor);
                int maxQuotient = Mathf.Min(6, Mathf.FloorToInt((float)_config.maxDividend / divisor));
                
                // Ensure valid range
                if (minQuotient > maxQuotient)
                {
                    continue; // Try next attempt
                }
                
                int quotient = Random.Range(minQuotient, maxQuotient + 1);
                int dividend = quotient * divisor; // Ensures whole number result

                var question = (dividend, divisor);

                // Check if this question hasn't been used yet
                if (!_usedQuestions.Contains(question))
                {
                    _usedQuestions.Add(question);
                    return question;
                }
            }

            // If we couldn't find a unique question after max attempts, generate one anyway (allow repeats)
            Debug.LogWarning("Could not generate unique question after max attempts. Generating question that may repeat.");
            
            int finalDivisor = _config.allowedDivisors[Random.Range(0, _config.allowedDivisors.Length)];
            int finalMinQuotient = Mathf.CeilToInt((float)_config.minDividend / finalDivisor);
            int finalMaxQuotient = Mathf.Min(6, Mathf.FloorToInt((float)_config.maxDividend / finalDivisor));
            
            // Ensure valid range
            if (finalMinQuotient > finalMaxQuotient)
            {
                // Fallback to a simple question
                finalDivisor = _config.allowedDivisors[0];
                finalMinQuotient = 1;
                finalMaxQuotient = Mathf.Min(6, Mathf.FloorToInt((float)_config.maxDividend / finalDivisor));
            }
            
            int finalQuotient = Random.Range(finalMinQuotient, finalMaxQuotient + 1);
            int finalDividend = finalQuotient * finalDivisor;

            var finalQuestion = (finalDividend, finalDivisor);
            _usedQuestions.Add(finalQuestion);
            return finalQuestion;
        }
    }
}
