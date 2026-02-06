using UnityEngine;

namespace HackingProject.Game
{
    public interface IGameState
    {
        string Name { get; }
        void Enter();
        void Exit();
        void Tick();
    }

    public sealed class GameStateMachine
    {
        private IGameState _currentState;

        public IGameState CurrentState => _currentState;

        public void ChangeState(IGameState nextState)
        {
            if (nextState == null)
            {
                Debug.LogError("[GameStateMachine] Attempted to change to a null state.");
                return;
            }

            if (_currentState == nextState)
            {
                return;
            }

            var previousState = _currentState;
            previousState?.Exit();

            _currentState = nextState;

            if (previousState == null)
            {
                Debug.Log($"[GameStateMachine] Start -> {nextState.Name}");
            }
            else
            {
                Debug.Log($"[GameStateMachine] {previousState.Name} -> {nextState.Name}");
            }

            _currentState.Enter();
        }

        public void Tick()
        {
            _currentState?.Tick();
        }
    }
}
