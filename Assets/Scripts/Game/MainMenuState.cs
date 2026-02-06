using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HackingProject.Game
{
    public sealed class MainMenuState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IGameState _gameplayState;

        public MainMenuState(GameStateMachine stateMachine, IGameState gameplayState)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _gameplayState = gameplayState ?? throw new ArgumentNullException(nameof(gameplayState));
        }

        public string Name => "MainMenu";

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public void Tick()
        {
            if (IsGameplayRequested())
            {
                _stateMachine.ChangeState(_gameplayState);
            }
        }

        private static bool IsGameplayRequested()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.G);
#endif
        }
    }
}
