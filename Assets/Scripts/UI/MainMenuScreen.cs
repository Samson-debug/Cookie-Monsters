using UnityEngine;
using UnityEngine.UI;
using CookieGame.Core;
using CookieGame.GameStates;
using CookieGame.Patterns;

namespace CookieGame.UI
{
    /// <summary>
    /// Main menu screen
    /// </summary>
    public class MainMenuScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _practiceModeButton;
        [SerializeField] private Button _testModeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;

        [Header("Welcome")]
        [SerializeField] private Text _welcomeText;

        public override void Initialize()
        {
            base.Initialize();

            if (_practiceModeButton != null)
                _practiceModeButton.onClick.AddListener(OnPracticeModeClicked);

            if (_testModeButton != null)
                _testModeButton.onClick.AddListener(OnTestModeClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_infoButton != null)
                _infoButton.onClick.AddListener(OnInfoClicked);
        }

        protected override void OnShow()
        {
            base.OnShow();

            // Display welcome message (Student Name screen removed)
            if (_welcomeText != null)
            {
                _welcomeText.text = "Welcome to Cookie Monsters Divide!";
            }
        }

        private void OnPracticeModeClicked()
        {
            Debug.Log("Practice Mode button clicked");
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as MainMenuState;
                if (currentState != null)
                {
                    currentState.OnPracticeMode();
                }
                else
                {
                    Debug.LogError("Current state is not MainMenuState!");
                }
            }
            else
            {
                Debug.LogError("StateManager not found!");
            }
        }

        private void OnTestModeClicked()
        {
            Debug.Log("Test Mode button clicked");
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as MainMenuState;
                if (currentState != null)
                {
                    currentState.OnTestMode();
                }
                else
                {
                    Debug.LogError("Current state is not MainMenuState!");
                }
            }
            else
            {
                Debug.LogError("StateManager not found!");
            }
        }

        private void OnSettingsClicked()
        {
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as MainMenuState;
                currentState?.OnSettings();
            }
        }

        private void OnInfoClicked()
        {
            var stateManager = ServiceLocator.Instance.Get<GameStateManager>();
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState() as MainMenuState;
                currentState?.OnInfo();
            }
        }
    }
}
