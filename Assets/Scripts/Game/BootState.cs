using System;

namespace HackingProject.Game
{
    public sealed class BootState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IGameState _nextState;
        private bool _hasTransitioned;

        public BootState(GameStateMachine stateMachine, IGameState nextState)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _nextState = nextState ?? throw new ArgumentNullException(nameof(nextState));
        }

        public string Name => "Boot";

        public void Enter()
        {
            _hasTransitioned = false;
        }

        public void Exit()
        {
        }

        public void Tick()
        {
            if (_hasTransitioned)
            {
                return;
            }

            _hasTransitioned = true;
            _stateMachine.ChangeState(_nextState);
        }
    }
}
