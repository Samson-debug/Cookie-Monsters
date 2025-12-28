using UnityEngine;

namespace CookieGame.Patterns
{
    /// <summary>
    /// State Machine pattern for managing game states
    /// Follows Open/Closed Principle - open for extension, closed for modification
    /// </summary>
    public abstract class GameState
    {
        protected GameStateManager stateManager;

        public virtual void Enter(GameStateManager manager)
        {
            stateManager = manager;
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void Exit()
        {
        }
    }

    public class GameStateManager : MonoBehaviour
    {
        private GameState _currentState;

        public void ChangeState(GameState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter(this);
            Debug.Log($"State changed to: {newState?.GetType().Name}");
        }

        private void Update()
        {
            _currentState?.Update();
        }

        private void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        public GameState GetCurrentState() => _currentState;
    }
}
