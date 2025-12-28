using UnityEngine;
using UnityEngine.UI;
using CookieGame.Core;

namespace CookieGame.UI
{
    /// <summary>
    /// Main gameplay screen container
    /// Holds the game HUD and gameplay area
    /// </summary>
    public class GameplayScreen : UIScreen
    {
        [Header("References")]
        [SerializeField] private GameHUD _gameHUD;
        [SerializeField] private Button _pauseButton;

        private EventManager _eventManager;

        public override void Initialize()
        {
            base.Initialize();

            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();

            if (_gameHUD != null)
            {
                _gameHUD.gameObject.SetActive(true);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (_gameHUD != null)
            {
                _gameHUD.gameObject.SetActive(false);
            }
        }

        private void OnPauseClicked()
        {
            GameManager.Instance?.PauseGame();
            // Show pause menu (could be implemented as a popup)
        }
    }
}
