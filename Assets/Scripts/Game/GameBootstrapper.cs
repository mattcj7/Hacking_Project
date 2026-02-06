using System;
using HackingProject.Infrastructure.Events;
using UnityEngine;

namespace HackingProject.Game
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private EventBus _eventBus;
        private GameStateMachine _stateMachine;
        private IDisposable _stateChangedSubscription;

        private void Awake()
        {
            _eventBus = new EventBus();
            _stateMachine = new GameStateMachine(_eventBus);

            var gameplayState = new GameplayState();
            var mainMenuState = new MainMenuState(_stateMachine, gameplayState);
            var bootState = new BootState(_stateMachine, mainMenuState);

            _stateChangedSubscription = _eventBus.Subscribe<StateChangedEvent>(OnStateChanged);

            _stateMachine.ChangeState(bootState);
        }

        private void Update()
        {
            _stateMachine.Tick();
        }

        private void OnDestroy()
        {
            _stateChangedSubscription?.Dispose();
        }

        private static void OnStateChanged(StateChangedEvent evt)
        {
            Debug.Log($"[EventBus] State changed: {evt.PreviousState} -> {evt.NextState}");
        }
    }
}
