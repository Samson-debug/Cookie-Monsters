using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Manages submitted questions for test mode
    /// Stores questions and provides them to QuestionGenerator
    /// </summary>
    public class NewQuestionGenerator : MonoBehaviour
    {
        public Button submitBtn;

        private List<(int dividend, int divisor)> questions = new();
        private int _currentQuestionIndex = 0;
        private bool _hasSubmittedQuestions = false;

        private static NewQuestionGenerator _instance;
        public static NewQuestionGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = GameObject.Find("NewQuestionGenerator");
                    if (obj == null)
                    {
                        obj = new GameObject("NewQuestionGenerator");
                        DontDestroyOnLoad(obj);
                    }
                    _instance = obj.GetComponent<NewQuestionGenerator>();
                    if (_instance == null)
                    {
                        _instance = obj.AddComponent<NewQuestionGenerator>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (submitBtn != null)
            {
                //submitBtn.onClick.AddListener(SubmitQuestions);
            }
        }

        /// <summary>
        /// Public method to submit questions from UI
        /// </summary>
        public void SubmitQuestions()
        {
            // Clear old questions first to ensure clean state
            ClearQuestions();

            var questionFields =
                FindObjectsByType<QuestionInputField>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (var q in questionFields)
            {
                var questionData = q.GetQuestionData();
                // Only add valid questions (non-zero divisor)
                if (questionData.divisor > 0 && questionData.dividend > 0)
                {
                    questions.Add(questionData);
                    Debug.Log($"[NewQuestionGenerator] Added question: {questionData.dividend} ÷ {questionData.divisor}");
                }
            }

            _hasSubmittedQuestions = questions.Count > 0;
            _currentQuestionIndex = 0; // Reset index for new submission
            Debug.Log($"[NewQuestionGenerator] Submitted {questions.Count} questions");
        }

        /// <summary>
        /// Gets the next submitted question, or null if all questions are used
        /// </summary>
        public (int dividend, int divisor)? GetNextQuestion()
        {
            if (!_hasSubmittedQuestions || _currentQuestionIndex >= questions.Count)
            {
                return null;
            }

            var question = questions[_currentQuestionIndex];
            _currentQuestionIndex++;
            return question;
        }

        /// <summary>
        /// Checks if there are more questions available
        /// </summary>
        public bool HasMoreQuestions()
        {
            return _hasSubmittedQuestions && _currentQuestionIndex < questions.Count;
        }

        /// <summary>
        /// Gets the total number of submitted questions
        /// </summary>
        public int GetTotalQuestions()
        {
            return questions.Count;
        }

        /// <summary>
        /// Resets the question index (for restarting)
        /// </summary>
        public void Reset()
        {
            _currentQuestionIndex = 0;
        }

        /// <summary>
        /// Clears all submitted questions
        /// </summary>
        public void ClearQuestions()
        {
            questions.Clear();
            _currentQuestionIndex = 0;
            _hasSubmittedQuestions = false;
        }
    }
}