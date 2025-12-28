using UnityEngine;
using CookieGame.Core;
using CookieGame.Data;
using CookieGame.GameStates;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Manages player lives system
    /// Follows Single Responsibility Principle
    /// </summary>
    public class LivesManager : MonoBehaviour
    {
        private GameConfig _config;
        private EventManager _eventManager;

        private int _currentLives;
        private bool _isPracticeMode;

        public void Initialize(bool isPracticeMode)
        {
            _isPracticeMode = isPracticeMode;

            _config = Resources.Load<GameConfig>("GameConfig");
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }

            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            _currentLives = _config.GetLives(isPracticeMode);

            PublishLivesUpdate();
        }

        /// <summary>
        /// Loses one life
        /// Publishes event if all lives depleted
        /// </summary>
        public void LoseLife()
        {
            if (_isPracticeMode && _config.practiceModeLives == 0)
            {
                // Unlimited lives in practice mode
                return;
            }

            _currentLives--;

            if (_currentLives < 0)
            {
                _currentLives = 0;
            }

            PublishLivesUpdate();

            if (_currentLives == 0)
            {
                _eventManager.Publish(new LivesDepletedEvent());
                Debug.Log("Lives depleted - Game Over!");
            }
        }

        /// <summary>
        /// Adds one life (for powerups or achievements)
        /// </summary>
        public void AddLife()
        {
            _currentLives++;
            PublishLivesUpdate();
        }

        private void PublishLivesUpdate()
        {
            _eventManager.Publish(new LivesUpdatedEvent
            {
                remainingLives = _currentLives
            });
        }

        /// <summary>
        /// Checks if player has lives remaining
        /// </summary>
        public bool HasLives()
        {
            if (_isPracticeMode && _config.practiceModeLives == 0)
            {
                return true; // Unlimited lives
            }

            return _currentLives > 0;
        }

        public int GetCurrentLives() => _currentLives;
    }
}
