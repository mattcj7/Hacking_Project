using System;
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

        [SerializeField] private VisualTreeAsset windowTemplate;

        private Label _clockLabel;
        private VisualElement _windowsLayer;
        private VisualElement _taskbarApps;
        private float _nextUpdateTime;
        private WindowManager _windowManager;
        private AppRegistry _appRegistry;
        private AppLauncher _appLauncher;
        private bool _loggedMissingTemplate;

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
                _appRegistry = new AppRegistry(new[]
                {
                    new AppDefinition(AppId.Terminal, "Terminal"),
                    new AppDefinition(AppId.FileManager, "File Manager")
                });
            }

            if (_appLauncher == null && _windowManager != null)
            {
                _appLauncher = new AppLauncher(_windowManager);
            }

            if (_taskbarApps != null && _appRegistry != null && _appLauncher != null)
            {
                PopulateTaskbar();
            }

            UpdateClock();
        }

        private void Update()
        {
            if (_clockLabel == null)
            {
                return;
            }

            if (Time.unscaledTime < _nextUpdateTime)
            {
                return;
            }

            UpdateClock();
        }

        private void PopulateTaskbar()
        {
            _taskbarApps.Clear();
            var apps = _appRegistry.InstalledApps;
            for (var i = 0; i < apps.Count; i++)
            {
                var app = apps[i];
                var appDefinition = app;
                var button = new Button(() => _appLauncher.LaunchOrFocus(appDefinition))
                {
                    text = appDefinition.DisplayName
                };
                var marginRight = i < apps.Count - 1 ? 8f : 0f;
                button.style.marginRight = marginRight;
                _taskbarApps.Add(button);
            }
        }

        private void UpdateClock()
        {
            _clockLabel.text = DateTime.Now.ToString("HH:mm:ss");
            _nextUpdateTime = Time.unscaledTime + 1f;
        }
    }
}
