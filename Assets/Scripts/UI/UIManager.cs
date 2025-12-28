using UnityEngine;
using System.Collections.Generic;
using CookieGame.Core;

namespace CookieGame.UI
{
    /// <summary>
    /// Centralized UI Manager
    /// Follows Single Responsibility Principle - manages UI screens and transitions
    /// Uses dictionary for O(1) screen lookup
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [System.Serializable]
        public class ScreenEntry
        {
            public string screenName;
            public UIScreen screen;
        }

        [Header("Screens")]
        [SerializeField] private List<ScreenEntry> _screens = new List<ScreenEntry>();

        [Header("HUD")]
        [SerializeField] private GameHUD _gameHUD;

        private Dictionary<string, UIScreen> _screenLookup;
        private UIScreen _currentScreen;
        private EventManager _eventManager;

        private void Awake()
        {
            // Keep all screen GameObjects active for event subscriptions
            // Visibility is controlled by CanvasGroup (set in UIScreen.Initialize)
            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            // Initialize screens (this will set CanvasGroup alpha to 0)
            InitializeScreens();

            Debug.Log("UIManager: All screens initialized (visible via CanvasGroup)");
        }

        private void InitializeScreens()
        {
            _screenLookup = new Dictionary<string, UIScreen>();

            foreach (var entry in _screens)
            {
                if (entry.screen != null && !string.IsNullOrEmpty(entry.screenName))
                {
                    _screenLookup[entry.screenName] = entry.screen;
                    entry.screen.Initialize();
                }
            }

            Debug.Log($"UI Manager initialized with {_screenLookup.Count} screens");
        }

        /// <summary>
        /// Shows the specified screen
        /// </summary>
        public void ShowScreen(string screenName)
        {
            if (_screenLookup.TryGetValue(screenName, out UIScreen screen))
            {
                _currentScreen = screen;
                screen.Show();

                _eventManager?.Publish(new ScreenChangedEvent { screenName = screenName });

                Debug.Log($"Showing screen: {screenName}");
            }
            else
            {
                Debug.LogWarning($"Screen not found: {screenName}");
            }
        }

        /// <summary>
        /// Hides the specified screen
        /// </summary>
        public void HideScreen(string screenName)
        {
            if (_screenLookup.TryGetValue(screenName, out UIScreen screen))
            {
                screen.Hide();
                Debug.Log($"Hiding screen: {screenName}");
            }
        }

        /// <summary>
        /// Hides current screen and shows new one
        /// </summary>
        public void TransitionToScreen(string screenName)
        {
            _currentScreen?.Hide();
            ShowScreen(screenName);
        }

        /// <summary>
        /// Updates loading progress
        /// </summary>
        public void UpdateLoadingProgress(float progress)
        {
            if (_screenLookup.TryGetValue("LoadingScreen", out UIScreen screen))
            {
                (screen as LoadingScreen)?.UpdateProgress(progress);
            }
        }

        /// <summary>
        /// Updates practice results
        /// </summary>
        public void UpdatePracticeResults(int score, float accuracy)
        {
            if (_screenLookup.TryGetValue("PracticeCompleteScreen", out UIScreen screen))
            {
                (screen as PracticeCompleteScreen)?.UpdateResults(score, accuracy);
            }
        }

        /// <summary>
        /// Updates game over results
        /// </summary>
        public void UpdateGameOverResults(int score, float accuracy, int highScore)
        {
            if (_screenLookup.TryGetValue("GameOverScreen", out UIScreen screen))
            {
                (screen as GameOverScreen)?.UpdateResults(score, accuracy, highScore);
            }
        }

        /// <summary>
        /// Shows VFX at specified position
        /// </summary>
        public void ShowVFX(string vfxName, Vector3 position)
        {
            _eventManager?.Publish(new VFXRequestEvent
            {
                vfxName = vfxName,
                position = position
            });
        }

        public UIScreen GetCurrentScreen() => _currentScreen;
        public GameHUD GetGameHUD() => _gameHUD;
    }
}
