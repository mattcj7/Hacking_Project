using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Time;
using HackingProject.Infrastructure.Vfs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HackingProject.Game
{
    public sealed class GameBootstrapper : MonoBehaviour, ITimeServiceProvider, IVfsProvider
    {
        private EventBus _eventBus;
        private GameStateMachine _stateMachine;
        private SaveService _saveService;
        private SaveGameData _saveData;
        private TimeService _timeService;
        private VirtualFileSystem _vfs;
        private IDisposable _stateChangedSubscription;

        public EventBus EventBus => _eventBus;
        public TimeService TimeService => _timeService;
        public VirtualFileSystem Vfs => _vfs;

        private void Awake()
        {
            _eventBus = new EventBus();
            _stateMachine = new GameStateMachine(_eventBus);
            _timeService = new TimeService(_eventBus);
            _saveService = new SaveService(Application.persistentDataPath);
            _vfs = DefaultVfsFactory.Create();

            var gameplayState = new GameplayState();
            var mainMenuState = new MainMenuState(_stateMachine, gameplayState);
            var bootState = new BootState(_stateMachine, mainMenuState);

            _stateChangedSubscription = _eventBus.Subscribe<StateChangedEvent>(OnStateChanged);

            if (_saveService.TryLoad(out var loaded))
            {
                _saveData = loaded;
                _eventBus.Publish(new SaveLoadedEvent(_saveData, _saveService.LastLoadSource));
                Debug.Log($"[SaveService] Loaded save ({_saveService.LastLoadSource}).");
            }
            else
            {
                _saveData = SaveGameData.CreateDefault();
                _eventBus.Publish(new SaveLoadedEvent(_saveData, SaveLoadSource.None));
                Debug.Log("[SaveService] Created new save.");
            }

            _stateMachine.ChangeState(bootState);
        }

        private void Update()
        {
            _stateMachine.Tick();
            _timeService?.Tick(Time.unscaledDeltaTime);
            HandleSaveInput();
        }

        private void OnDestroy()
        {
            _stateChangedSubscription?.Dispose();
        }

        private static void OnStateChanged(StateChangedEvent evt)
        {
            Debug.Log($"[EventBus] State changed: {evt.PreviousState} -> {evt.NextState}");
        }

        private void HandleSaveInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f5Key.wasPressedThisFrame)
            {
                TrySave();
            }

            if (keyboard.f9Key.wasPressedThisFrame)
            {
                TryClearSave();
            }
        }

        private void TrySave()
        {
            try
            {
                _saveService.Save(_saveData);
                _eventBus.Publish(new SaveCompletedEvent(_saveData));
                Debug.Log("[SaveService] Save completed.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Save failed: {ex.Message}");
            }
        }

        private void TryClearSave()
        {
            try
            {
                _saveService.DeleteAll();
                _saveData = SaveGameData.CreateDefault();
                Debug.Log("[SaveService] Cleared save files.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Clear failed: {ex.Message}");
            }
        }
    }
}
