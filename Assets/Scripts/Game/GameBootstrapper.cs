using UnityEngine;

namespace HackingProject.Game
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private GameStateMachine _stateMachine;

        private void Awake()
        {
            _stateMachine = new GameStateMachine();

            var gameplayState = new GameplayState();
            var mainMenuState = new MainMenuState(_stateMachine, gameplayState);
            var bootState = new BootState(_stateMachine, mainMenuState);

            _stateMachine.ChangeState(bootState);
        }

        private void Update()
        {
            _stateMachine.Tick();
        }
    }
}
