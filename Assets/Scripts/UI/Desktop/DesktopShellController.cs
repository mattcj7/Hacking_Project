using System;
using HackingProject.Infrastructure.Time;
using HackingProject.UI.Apps;
using HackingProject.UI.Windows;
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
        private const string WindowsLayerName = "windows-layer";
        private const string TaskbarAppsName = "taskbar-apps";
        private const string TaskbarAppsClassName = "taskbar-apps";
        private const string TaskbarClassName = "taskbar";
        private const string WindowTemplatePath = "Assets/UI/Windows/Window.uxml";
        private const string AppCatalogPath = "Assets/ScriptableObjects/Apps/AppCatalog_Default.asset";

        [SerializeField] private VisualTreeAsset windowTemplate;
        [SerializeField] private AppCatalogSO appCatalog;

        private Label _clockLabel;
        private VisualElement _windowsLayer;
        private VisualElement _taskbarApps;
        private WindowManager _windowManager;
        private AppRegistry _appRegistry;
        private AppLauncher _appLauncher;
        private ITimeServiceProvider _timeServiceProvider;
        private IDisposable _timeSubscription;
        private bool _loggedMissingTemplate;
        private bool _loggedMissingAppCatalog;
        private bool _loggedMissingTimeService;
        private bool _hasStarted;

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

                if (appCatalog != null)
                {
                    _appRegistry = new AppRegistry(appCatalog.Apps);
                }
            }

            if (_appLauncher == null && _windowManager != null)
            {
                _appLauncher = new AppLauncher(_windowManager);
            }

            if (_taskbarApps != null && _appRegistry != null && _appLauncher != null)
            {
                PopulateTaskbar();
            }

            if (_hasStarted)
            {
                TryHookTimeService();
            }
        }

        private void Start()
        {
            _hasStarted = true;
            TryHookTimeService();
        }

        private void OnDisable()
        {
            _timeSubscription?.Dispose();
            _timeSubscription = null;
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
        }

        private void OnTimeSecondTicked(TimeSecondTickedEvent evt)
        {
            UpdateClock(evt.CurrentTime);
        }

        private void UpdateClock(DateTime time)
        {
            _clockLabel.text = TimeService.FormatTime(time);
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
    }
}
