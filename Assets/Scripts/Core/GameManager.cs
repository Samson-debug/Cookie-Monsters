using UnityEngine;
using CookieGame.Patterns;
using CookieGame.Audio;
using CookieGame.UI;
using CookieGame.GameStates;
using CookieGame.Gameplay;

namespace CookieGame.Core
{
    /// <summary>
    /// Main Game Manager - orchestrates all game systems
    /// Singleton pattern with DontDestroyOnLoad
    /// Follows Single Responsibility Principle by delegating to specialized managers
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private GameStateManager _stateManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PoolManager _poolManager;

        private EventManager _eventManager;
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            // Create and register all services
            _eventManager = new EventManager();
            
            ServiceLocator.Instance.Register(_eventManager);
            ServiceLocator.Instance.Register(_stateManager);
            ServiceLocator.Instance.Register(_audioManager);
            ServiceLocator.Instance.Register(_uiManager);
            ServiceLocator.Instance.Register(_poolManager);

            Debug.Log("All services initialized and registered");
        }

        private void Start()
        {
            // Start directly at main menu (Splash and Student Name screens removed)
            _stateManager.ChangeState(new MainMenuState());
        }
        
        public void PauseGame()
        {
            Time.timeScale = 0f;
            _eventManager.Publish(new GamePausedEvent());
        }
        
        private void OnDestroy()
        {
            ServiceLocator.Instance.Clear();
            _eventManager?.Clear();
        }
        
        #region Useless Code
        
        /*
        public void StartGame(bool isPracticeMode)
        {
            _eventManager.Publish(new GameStartedEvent());
            _stateManager.ChangeState(new GameplayState(isPracticeMode));
        }
        */

        /*
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            _eventManager.Publish(new GameResumedEvent());
        }
        */

        /*
        public void EndGame(int finalScore, float accuracy)
        {
            _eventManager.Publish(new GameOverEvent { finalScore = finalScore, accuracy = accuracy });
            _stateManager.ChangeState(new GameOverState(finalScore, accuracy));
        }
        */
        /*public void RestartGame()
        {
            _poolManager.ReturnAll<Cookie>("Cookie");
            Time.timeScale = 1f;
            _stateManager.ChangeState(new GameplayState(false));
        }*/
        /*public void QuitToMenu()
        {
            Time.timeScale = 1f;
            _stateManager.ChangeState(new MainMenuState());
        } */       
        #endregion
    }
}
