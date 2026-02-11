using System;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Time;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Infrastructure.Wallet;
using HackingProject.UI.Apps;
using HackingProject.UI.Windows;
using HackingProject.Systems.Store;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HackingProject.UI.Desktop
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class DesktopShellController : MonoBehaviour
    {
        private const string ClockLabelName = "clock-label";
        private const string CreditsLabelName = "credits-label";
        private const string WindowsLayerName = "windows-layer";
        private const string TaskbarAppsName = "taskbar-apps";
        private const string TaskbarAppsClassName = "taskbar-apps";
        private const string TaskbarClassName = "taskbar";
        private const string ToastContainerName = "toast-container";
        private const string WindowTemplatePath = "Assets/UI/Windows/Window.uxml";
        private const string AppCatalogPath = "Assets/ScriptableObjects/Apps/AppCatalog_Default.asset";
        private const string StoreCatalogPath = "Assets/ScriptableObjects/Store/StoreCatalog_Default.asset";
        private const string PackageDatabasePath = "Assets/ScriptableObjects/Apps/AppPackageDatabase_Default.asset";

        [SerializeField] private VisualTreeAsset windowTemplate;
        [SerializeField] private AppCatalogSO appCatalog;
        [SerializeField] private StoreCatalogSO storeCatalog;
        [SerializeField] private AppPackageDatabaseSO appPackageDatabase;

        private Label _clockLabel;
        private Label _creditsLabel;
        private VisualElement _windowsLayer;
        private VisualElement _taskbarApps;
        private VisualElement _toastContainer;
        private WindowManager _windowManager;
        private AppRegistry _appRegistry;
        private AppLauncher _appLauncher;
        private ToastController _toastController;
        private ITimeServiceProvider _timeServiceProvider;
        private IMissionServiceProvider _missionServiceProvider;
        private IWalletServiceProvider _walletServiceProvider;
        private IStoreServiceProvider _storeServiceProvider;
        private IInstallServiceProvider _installServiceProvider;
        private IVfsProvider _vfsProvider;
        private ISaveGameDataProvider _saveDataProvider;
        private IDisposable _timeSubscription;
        private IDisposable _saveSessionSubscription;
        private IDisposable _creditsSubscription;
        private IDisposable _appInstalledSubscription;
        private bool _loggedMissingTemplate;
        private bool _loggedMissingAppCatalog;
        private bool _loggedMissingTimeService;
        private bool _loggedMissingMissionService;
        private bool _loggedMissingWalletService;
        private bool _loggedMissingToastContainer;
        private bool _loggedMissingSaveProvider;
        private bool _loggedMissingStoreCatalog;
        private bool _loggedMissingPackageDatabase;
        private bool _loggedMissingStoreService;
        private bool _hasStarted;
        private bool _sessionRestored;

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            if (document == null)
            {
                Debug.LogWarning("[DesktopShellController] Missing UIDocument.");
                return;
            }

            var root = document.rootVisualElement;
            _clockLabel = root?.Q<Label>(ClockLabelName);
            if (_clockLabel == null)
            {
                Debug.LogWarning($"[DesktopShellController] Clock label '{ClockLabelName}' not found.");
                return;
            }

            _creditsLabel = root?.Q<Label>(CreditsLabelName);
            if (_creditsLabel == null)
            {
                Debug.LogWarning($"[DesktopShellController] Credits label '{CreditsLabelName}' not found.");
            }

            _windowsLayer = root?.Q<VisualElement>(WindowsLayerName);
            if (_windowsLayer == null && root != null)
            {
                _windowsLayer = new VisualElement { name = WindowsLayerName };
                _windowsLayer.AddToClassList("windows-layer");

                var taskbar = root.Q<VisualElement>(className: TaskbarClassName);
                if (taskbar != null)
                {
                    var taskbarIndex = root.IndexOf(taskbar);
                    root.Insert(taskbarIndex, _windowsLayer);
                }
                else
                {
                    root.Add(_windowsLayer);
                }
            }

            if (_windowsLayer == null)
            {
                Debug.LogWarning($"[DesktopShellController] Windows layer '{WindowsLayerName}' not found.");
            }

            _toastContainer = root?.Q<VisualElement>(ToastContainerName);
            if (_toastContainer == null)
            {
                if (!_loggedMissingToastContainer)
                {
                    Debug.LogWarning($"[DesktopShellController] Toast container '{ToastContainerName}' not found.");
                    _loggedMissingToastContainer = true;
                }
            }

            _taskbarApps = root?.Q<VisualElement>(TaskbarAppsName);
            if (_taskbarApps == null && root != null)
            {
                var taskbar = root.Q<VisualElement>(className: TaskbarClassName);
                if (taskbar != null)
                {
                    _taskbarApps = new VisualElement { name = TaskbarAppsName };
                    _taskbarApps.AddToClassList(TaskbarAppsClassName);
                    taskbar.Insert(1, _taskbarApps);
                }
            }

            if (_taskbarApps == null)
            {
                Debug.LogWarning($"[DesktopShellController] Taskbar apps container '{TaskbarAppsName}' not found.");
            }

            if (windowTemplate == null)
            {
#if UNITY_EDITOR
                windowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WindowTemplatePath);
#endif
                if (windowTemplate == null)
                {
                    if (!_loggedMissingTemplate)
                    {
                        Debug.LogError($"[DesktopShellController] Window template missing on '{gameObject.name}' in scene '{gameObject.scene.path}'.");
                        _loggedMissingTemplate = true;
                    }
                }
            }

            if (_windowManager == null && _windowsLayer != null && windowTemplate != null)
            {
                _windowManager = new WindowManager(_windowsLayer, windowTemplate);
            }

            if (_appRegistry == null)
            {
                if (appCatalog == null)
                {
#if UNITY_EDITOR
                    appCatalog = AssetDatabase.LoadAssetAtPath<AppCatalogSO>(AppCatalogPath);
                    if (appCatalog == null && !_loggedMissingAppCatalog)
                    {
                        Debug.LogError($"[DesktopShellController] Missing AppCatalog at '{AppCatalogPath}' on '{gameObject.name}' in scene '{gameObject.scene.path}'.");
                        _loggedMissingAppCatalog = true;
                    }
#endif
                }

                if (appPackageDatabase == null)
                {
#if UNITY_EDITOR
                    appPackageDatabase = AssetDatabase.LoadAssetAtPath<AppPackageDatabaseSO>(PackageDatabasePath);
                    if (appPackageDatabase == null && !_loggedMissingPackageDatabase)
                    {
                        Debug.LogError($"[DesktopShellController] Missing AppPackageDatabase at '{PackageDatabasePath}' on '{gameObject.name}' in scene '{gameObject.scene.path}'.");
                        _loggedMissingPackageDatabase = true;
                    }
#endif
                }

                if (storeCatalog == null)
                {
#if UNITY_EDITOR
                    storeCatalog = AssetDatabase.LoadAssetAtPath<StoreCatalogSO>(StoreCatalogPath);
                    if (storeCatalog == null && !_loggedMissingStoreCatalog)
                    {
                        Debug.LogError($"[DesktopShellController] Missing StoreCatalog at '{StoreCatalogPath}' on '{gameObject.name}' in scene '{gameObject.scene.path}'.");
                        _loggedMissingStoreCatalog = true;
                    }
#endif
                }

                if (appCatalog != null)
                {
                    var apps = new System.Collections.Generic.List<AppDefinitionSO>();
                    for (var i = 0; i < appCatalog.Apps.Count; i++)
                    {
                        var app = appCatalog.Apps[i];
                        if (app != null)
                        {
                            apps.Add(app);
                        }
                    }

                    if (_saveDataProvider == null)
                    {
                        _saveDataProvider = FindSaveDataProvider();
                    }

                    var installed = _saveDataProvider?.SaveData?.InstalledAppIds;
                    if (installed != null && appPackageDatabase != null)
                    {
                        for (var i = 0; i < installed.Count; i++)
                        {
                            var entry = installed[i];
                            if (string.IsNullOrWhiteSpace(entry))
                            {
                                continue;
                            }

                            if (!Enum.TryParse(entry, out AppId appId))
                            {
                                continue;
                            }

                            if (appPackageDatabase.TryGetById(appId, out var app))
                            {
                                var exists = false;
                                for (var j = 0; j < apps.Count; j++)
                                {
                                    if (apps[j] != null && apps[j].Id == appId)
                                    {
                                        exists = true;
                                        break;
                                    }
                                }

                                if (!exists)
                                {
                                    apps.Add(app);
                                }
                            }
                        }
                    }

                    _appRegistry = new AppRegistry(apps);
                }
            }

            if (_appLauncher == null && _windowManager != null)
            {
                _appLauncher = new AppLauncher(_windowManager);
                TryHookVfs();
                TryHookSession();
            }

            if (_taskbarApps != null && _appRegistry != null && _appLauncher != null)
            {
                PopulateTaskbar();
            }

            TryHookTimeService();
            TryHookNotifications();
            TryHookMissionService();
            TryHookStoreServices();
            TryHookWalletService();
            TryHookVfs();
            TryHookSession();
        }

        private void Start()
        {
            _hasStarted = true;
            TryHookTimeService();
            TryHookNotifications();
            TryHookMissionService();
            TryHookStoreServices();
            TryHookWalletService();
            TryHookVfs();
            TryHookSession();
        }

        private void OnDisable()
        {
            _timeSubscription?.Dispose();
            _timeSubscription = null;
            _saveSessionSubscription?.Dispose();
            _saveSessionSubscription = null;
            _creditsSubscription?.Dispose();
            _creditsSubscription = null;
            _appInstalledSubscription?.Dispose();
            _appInstalledSubscription = null;
            _toastController?.Dispose();
            _toastController = null;
        }

        private void PopulateTaskbar()
        {
            _taskbarApps.Clear();
            var apps = _appRegistry.InstalledApps;
            var validCount = 0;
            for (var i = 0; i < apps.Count; i++)
            {
                if (apps[i] != null)
                {
                    validCount++;
                }
            }

            var added = 0;
            for (var i = 0; i < apps.Count; i++)
            {
                var app = apps[i];
                if (app == null)
                {
                    continue;
                }

                var appDefinition = app;
                var button = new Button(() => _appLauncher.LaunchOrFocus(appDefinition))
                {
                    text = string.IsNullOrWhiteSpace(appDefinition.DisplayName) ? appDefinition.name : appDefinition.DisplayName
                };
                added++;
                var marginRight = added < validCount ? 8f : 0f;
                button.style.marginRight = marginRight;
                _taskbarApps.Add(button);
            }
        }

        private void TryHookTimeService()
        {
            if (_timeSubscription != null || _clockLabel == null)
            {
                return;
            }

            if (_timeServiceProvider == null)
            {
                _timeServiceProvider = FindTimeServiceProvider();
            }

            if (_timeServiceProvider == null || _timeServiceProvider.EventBus == null || _timeServiceProvider.TimeService == null)
            {
                if (_hasStarted && !_loggedMissingTimeService)
                {
                    Debug.LogWarning("[DesktopShellController] TimeService provider not found.");
                    _loggedMissingTimeService = true;
                }

                return;
            }

            _timeSubscription = _timeServiceProvider.EventBus.Subscribe<TimeSecondTickedEvent>(OnTimeSecondTicked);
            UpdateClock(_timeServiceProvider.TimeService.CurrentTime);

            if (_appLauncher != null)
            {
                _appLauncher.SetEventBus(_timeServiceProvider.EventBus);
            }
        }

        private void TryHookWalletService()
        {
            if (_creditsSubscription != null || _creditsLabel == null)
            {
                return;
            }

            if (_walletServiceProvider == null)
            {
                _walletServiceProvider = FindWalletServiceProvider();
            }

            if (_walletServiceProvider == null || _walletServiceProvider.WalletService == null)
            {
                if (_hasStarted && !_loggedMissingWalletService)
                {
                    Debug.LogWarning("[DesktopShellController] WalletService provider not found.");
                    _loggedMissingWalletService = true;
                }

                return;
            }

            if (_timeServiceProvider == null)
            {
                _timeServiceProvider = FindTimeServiceProvider();
            }

            if (_timeServiceProvider?.EventBus == null)
            {
                return;
            }

            _creditsSubscription = _timeServiceProvider.EventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
            UpdateCredits(_walletServiceProvider.WalletService.Credits);
        }

        private void TryHookNotifications()
        {
            if (_toastController != null || _toastContainer == null)
            {
                return;
            }

            if (_timeServiceProvider == null)
            {
                _timeServiceProvider = FindTimeServiceProvider();
            }

            if (_timeServiceProvider?.EventBus == null)
            {
                return;
            }

            _toastController = new ToastController(_toastContainer, _timeServiceProvider.EventBus);
        }

        private void TryHookMissionService()
        {
            if (_appLauncher == null)
            {
                return;
            }

            if (_missionServiceProvider == null)
            {
                _missionServiceProvider = FindMissionServiceProvider();
            }

            if (_missionServiceProvider == null || _missionServiceProvider.MissionService == null)
            {
                if (_hasStarted && !_loggedMissingMissionService)
                {
                    Debug.LogWarning("[DesktopShellController] MissionService provider not found.");
                    _loggedMissingMissionService = true;
                }

                return;
            }

            _appLauncher.SetMissionService(_missionServiceProvider.MissionService);
        }

        private void TryHookStoreServices()
        {
            if (_appLauncher == null)
            {
                return;
            }

            if (_storeServiceProvider == null)
            {
                _storeServiceProvider = FindStoreServiceProvider();
            }

            if (_installServiceProvider == null)
            {
                _installServiceProvider = FindInstallServiceProvider();
            }

            if (_storeServiceProvider?.StoreService == null || _installServiceProvider?.InstallService == null)
            {
                if (_hasStarted && !_loggedMissingStoreService)
                {
                    Debug.LogWarning("[DesktopShellController] Store/Install service provider not found.");
                    _loggedMissingStoreService = true;
                }

                return;
            }

            if (storeCatalog != null)
            {
                _appLauncher.SetStoreServices(storeCatalog, _storeServiceProvider.StoreService, _installServiceProvider.InstallService);
            }

            if (_timeServiceProvider == null)
            {
                _timeServiceProvider = FindTimeServiceProvider();
            }

            if (_timeServiceProvider?.EventBus != null && _appInstalledSubscription == null)
            {
                _appInstalledSubscription = _timeServiceProvider.EventBus.Subscribe<AppInstalledEvent>(OnAppInstalled);
            }
        }

        private void TryHookVfs()
        {
            if (_appLauncher == null)
            {
                return;
            }

            if (_vfsProvider == null)
            {
                _vfsProvider = FindVfsProvider();
            }

            if (_vfsProvider?.Vfs == null)
            {
                return;
            }

            _appLauncher.SetVirtualFileSystem(_vfsProvider.Vfs);
        }

        private void TryHookSession()
        {
            if (_appLauncher == null)
            {
                return;
            }

            if (_saveDataProvider == null)
            {
                _saveDataProvider = FindSaveDataProvider();
            }

            if (_saveDataProvider == null || _saveDataProvider.SaveData == null)
            {
                if (_hasStarted && !_loggedMissingSaveProvider)
                {
                    Debug.LogWarning("[DesktopShellController] SaveGameData provider not found.");
                    _loggedMissingSaveProvider = true;
                }

                return;
            }

            var session = _saveDataProvider.SaveData.OsSession ?? (_saveDataProvider.SaveData.OsSession = new OsSessionData());
            _appLauncher.SetSessionData(session);

            if (!_sessionRestored && _appRegistry != null)
            {
                _appLauncher.RestoreSession(_appRegistry, session);
                _sessionRestored = true;
            }

            if (_timeServiceProvider != null && _saveSessionSubscription == null)
            {
                _saveSessionSubscription = _timeServiceProvider.EventBus.Subscribe<SaveSessionCaptureEvent>(OnSaveSessionCapture);
            }
        }

        private void OnTimeSecondTicked(TimeSecondTickedEvent evt)
        {
            UpdateClock(evt.CurrentTime);
        }

        private void OnCreditsChanged(CreditsChangedEvent evt)
        {
            UpdateCredits(evt.CurrentCredits);
        }

        private void OnAppInstalled(AppInstalledEvent evt)
        {
            if (_taskbarApps == null || _appRegistry == null || _appLauncher == null)
            {
                return;
            }

            if (appPackageDatabase != null && !string.IsNullOrWhiteSpace(evt.AppId) && Enum.TryParse(evt.AppId, out AppId appId))
            {
                if (appPackageDatabase.TryGetById(appId, out var app))
                {
                    _appRegistry.AddInstalledApp(app);
                }
            }

            PopulateTaskbar();
        }

        private void UpdateClock(DateTime time)
        {
            _clockLabel.text = TimeService.FormatTime(time);
        }

        private void UpdateCredits(int credits)
        {
            if (_creditsLabel == null)
            {
                return;
            }

            _creditsLabel.text = $"Credits: {credits}";
        }

        private static ITimeServiceProvider FindTimeServiceProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ITimeServiceProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static IVfsProvider FindVfsProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IVfsProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static ISaveGameDataProvider FindSaveDataProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ISaveGameDataProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static IWalletServiceProvider FindWalletServiceProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IWalletServiceProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static IMissionServiceProvider FindMissionServiceProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IMissionServiceProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static IStoreServiceProvider FindStoreServiceProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IStoreServiceProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static IInstallServiceProvider FindInstallServiceProvider()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInstallServiceProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private void OnSaveSessionCapture(SaveSessionCaptureEvent evt)
        {
            if (evt.Session == null || _appLauncher == null)
            {
                return;
            }

            _appLauncher.CaptureSession(evt.Session);
        }
    }
}
