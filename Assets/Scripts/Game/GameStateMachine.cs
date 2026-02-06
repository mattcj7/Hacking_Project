using HackingProject.Infrastructure.Events;
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
        private readonly EventBus _eventBus;
        private IGameState _currentState;

        public IGameState CurrentState => _currentState;

        public GameStateMachine(EventBus eventBus)
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
        }

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

            var previousStateName = previousState?.Name ?? "None";
            _eventBus.Publish(new StateChangedEvent(previousStateName, nextState.Name));

            _currentState.Enter();
        }

        public void Tick()
        {
            _currentState?.Tick();
        }
    }
}
