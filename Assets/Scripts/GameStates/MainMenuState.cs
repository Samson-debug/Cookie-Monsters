using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Core;
using CookieGame.UI;
using CookieGame.Audio;

namespace CookieGame.GameStates
{
    /// <summary>
    /// Main menu state - allows player to choose Practice or Test mode
    /// </summary>
    public class MainMenuState : GameState
    {
        private EventManager _eventManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;

        public override void Enter(GameStateManager manager)
        {
            base.Enter(manager);

            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            _uiManager?.ShowScreen("MainMenuScreen");
            _audioManager?.PlayMusic("MenuMusic");

            Debug.Log("Entered Main Menu State");
        }

        public void OnPracticeMode()
        {
            stateManager.ChangeState(new GameplayState(true));
        }

        public void OnTestMode()
        {
            stateManager.ChangeState(new QuestionSubmissionState());
        }

        public void OnSettings()
        {
            _uiManager?.ShowScreen("SettingsScreen");
        }

        public void OnInfo()
        {
            _uiManager?.ShowScreen("InfoScreen");
        }

        public override void Exit()
        {
            _uiManager?.HideScreen("MainMenuScreen");
            _audioManager?.StopMusic();
            Debug.Log("Exited Main Menu State");
        }
    }
}
