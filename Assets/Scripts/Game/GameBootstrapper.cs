using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Notifications;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Time;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Infrastructure.Wallet;
using HackingProject.Systems.Save;
using HackingProject.Systems.Store;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HackingProject.Game
{
    public sealed class GameBootstrapper : MonoBehaviour, ITimeServiceProvider, IVfsProvider, ISaveGameDataProvider, IMissionServiceProvider, IWalletServiceProvider, IStoreServiceProvider, IInstallServiceProvider
    {
        private const string MissionCatalogPath = "Assets/ScriptableObjects/Missions/MissionCatalog_Default.asset";

        [SerializeField] private MissionCatalogSO missionCatalog;

        private EventBus _eventBus;
        private GameStateMachine _stateMachine;
        private SaveService _saveService;
        private SaveGameData _saveData;
        private SaveMigrationService _saveMigrationService;
        private AutoSaveService _autoSaveService;
        private MissionService _missionService;
        private NotificationService _notificationService;
        private TimeService _timeService;
        private VirtualFileSystem _vfs;
        private WalletService _walletService;
        private StoreService _storeService;
        private InstallService _installService;
        private IDisposable _stateChangedSubscription;
        private IDisposable _creditsSubscription;
        private IDisposable _missionCompletedSubscription;
        private IDisposable _missionRewardSubscription;

        public EventBus EventBus => _eventBus;
        public TimeService TimeService => _timeService;
        public VirtualFileSystem Vfs => _vfs;
        public SaveGameData SaveData => _saveData;
        public MissionService MissionService => _missionService;
        public WalletService WalletService => _walletService;
        public StoreService StoreService => _storeService;
        public InstallService InstallService => _installService;

        private void Awake()
        {
            _eventBus = new EventBus();
            _stateMachine = new GameStateMachine(_eventBus);
            _timeService = new TimeService(_eventBus);
            _saveService = new SaveService(Application.persistentDataPath);
            _saveMigrationService = new SaveMigrationService();
            _vfs = DefaultVfsFactory.Create();
            _notificationService = new NotificationService(_eventBus);
            _missionCompletedSubscription = _eventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
            _missionRewardSubscription = _eventBus.Subscribe<MissionRewardGrantedEvent>(OnMissionRewardGranted);

#if UNITY_EDITOR
            if (missionCatalog == null)
            {
                missionCatalog = AssetDatabase.LoadAssetAtPath<MissionCatalogSO>(MissionCatalogPath);
                if (missionCatalog == null)
                {
                    Debug.LogWarning($"[MissionService] Missing MissionCatalog at '{MissionCatalogPath}'.");
                }
            }
#endif

            var gameplayState = new GameplayState();
            var mainMenuState = new MainMenuState(_stateMachine, gameplayState);
            var bootState = new BootState(_stateMachine, mainMenuState);

            _stateChangedSubscription = _eventBus.Subscribe<StateChangedEvent>(OnStateChanged);

            if (_saveService.TryLoad(out var loaded))
            {
                _saveData = loaded;
                var loadedVersion = _saveService.LastLoadVersion <= 0 ? 1 : _saveService.LastLoadVersion;
                _saveMigrationService.Migrate(_saveData, loadedVersion);
                _eventBus.Publish(new SaveLoadedEvent(_saveData, _saveService.LastLoadSource));
                Debug.Log($"[SaveService] Loaded save ({_saveService.LastLoadSource}).");
            }
            else
            {
                _saveData = SaveGameData.CreateDefault();
                _eventBus.Publish(new SaveLoadedEvent(_saveData, SaveLoadSource.None));
                Debug.Log("[SaveService] Created new save.");
            }

            _walletService = new WalletService(_eventBus, _saveData?.Credits ?? 0);
            _creditsSubscription = _eventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
            _storeService = new StoreService(_eventBus, _walletService, _saveData, _notificationService, _vfs);
            _installService = new InstallService(_eventBus, _saveData);
            _autoSaveService = new AutoSaveService(_eventBus, new SaveServiceWriter(_saveService), _saveData);
            _missionService = new MissionService(_eventBus, _walletService);
            if (missionCatalog != null && missionCatalog.Missions != null)
            {
                _missionService.SetCatalog(missionCatalog);
                if (missionCatalog.Missions.Count > 0)
                {
                    _missionService.SetActiveMission(missionCatalog.Missions[0]);
                }
            }

            _stateMachine.ChangeState(bootState);
        }

        private void Update()
        {
            _stateMachine.Tick();
            _timeService?.Tick(Time.unscaledDeltaTime);
            _autoSaveService?.Tick(Time.unscaledDeltaTime);
            HandleSaveInput();
        }

        private void OnDestroy()
        {
            _stateChangedSubscription?.Dispose();
            _creditsSubscription?.Dispose();
            _missionCompletedSubscription?.Dispose();
            _missionRewardSubscription?.Dispose();
            _missionService?.Dispose();
            _autoSaveService?.Dispose();
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
                EnsureSessionData();
                if (_walletService != null)
                {
                    _saveData.Credits = _walletService.Credits;
                }
                _eventBus.Publish(new SaveSessionCaptureEvent(_saveData.OsSession));
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
                _autoSaveService?.SetSaveData(_saveData);
                Debug.Log("[SaveService] Cleared save files.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Clear failed: {ex.Message}");
            }
        }

        private void EnsureSessionData()
        {
            if (_saveData == null)
            {
                return;
            }

            if (_saveData.OsSession == null)
            {
                _saveData.OsSession = new OsSessionData();
            }
        }

        private void EnsureStoreData()
        {
            if (_saveData == null)
            {
                return;
            }

            if (_saveData.OwnedAppIds == null)
            {
                _saveData.OwnedAppIds = new System.Collections.Generic.List<string>();
            }

            if (_saveData.InstalledAppIds == null)
            {
                _saveData.InstalledAppIds = new System.Collections.Generic.List<string>();
            }
        }

        private void OnMissionCompleted(MissionCompletedEvent evt)
        {
            if (_notificationService == null || evt.Mission == null)
            {
                return;
            }

            var title = string.IsNullOrWhiteSpace(evt.Mission.Title) ? evt.Mission.name : evt.Mission.Title;
            _notificationService.Post($"Mission Complete: {title}");
        }

        private void OnMissionRewardGranted(MissionRewardGrantedEvent evt)
        {
            if (_notificationService == null)
            {
                return;
            }

            if (evt.RewardCredits != 0)
            {
                _notificationService.Post($"+{evt.RewardCredits} Credits");
            }
        }

        private void OnCreditsChanged(CreditsChangedEvent evt)
        {
            if (_saveData == null)
            {
                return;
            }

            _saveData.Credits = evt.CurrentCredits;
        }
    }
}
