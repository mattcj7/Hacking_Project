using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Systems.Store;
using HackingProject.UI.Windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    public sealed class AppLauncher
    {
        private readonly Dictionary<AppId, WindowView> _openWindows = new Dictionary<AppId, WindowView>();
        private readonly WindowManager _windowManager;
        private Vector2 _nextSpawnPosition = new Vector2(64f, 72f);
        private readonly Vector2 _spawnOffset = new Vector2(48f, 36f);
        private EventBus _eventBus;
        private MissionService _missionService;
        private StoreCatalogSO _storeCatalog;
        private StoreService _storeService;
        private InstallService _installService;
        private VirtualFileSystem _vfs;
        private OsSessionData _sessionData;

        private const string FileManagerStartPath = "/home/user";
        private const string TerminalStartPath = "/home/user";

        public AppLauncher(WindowManager windowManager)
        {
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        }

        public void SetVirtualFileSystem(VirtualFileSystem vfs)
        {
            _vfs = vfs;
        }

        public void SetEventBus(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void SetMissionService(MissionService missionService)
        {
            _missionService = missionService;
        }

        public void SetStoreServices(StoreCatalogSO catalog, StoreService storeService, InstallService installService)
        {
            _storeCatalog = catalog;
            _storeService = storeService;
            _installService = installService;
        }

        public void SetSessionData(OsSessionData sessionData)
        {
            _sessionData = sessionData;
        }

        public WindowView LaunchOrFocus(AppDefinitionSO app)
        {
            return LaunchOrFocus(app, null);
        }

        public WindowView LaunchOrFocus(AppDefinitionSO app, Vector2? positionOverride)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (_openWindows.TryGetValue(app.Id, out var existing))
            {
                if (IsWindowOpen(existing))
                {
                    _windowManager.BringToFront(existing);
                    if (positionOverride.HasValue)
                    {
                        existing.SetPosition(positionOverride.Value);
                    }
                    return existing;
                }

                _openWindows.Remove(app.Id);
            }

            var displayName = string.IsNullOrWhiteSpace(app.DisplayName) ? app.name : app.DisplayName;
            var position = positionOverride ?? (app.DefaultWindowPosition == Vector2.zero ? _nextSpawnPosition : app.DefaultWindowPosition);
            var view = _windowManager.CreateWindow(displayName, position);
            if (app.DefaultWindowSize != Vector2.zero)
            {
                view.Frame.style.width = app.DefaultWindowSize.x;
                view.Frame.style.height = app.DefaultWindowSize.y;
                view.Root.style.width = app.DefaultWindowSize.x;
                view.Root.style.height = app.DefaultWindowSize.y;
            }

            if (!TryBuildAppContent(app, view, displayName))
            {
                SetPlaceholderContent(view, displayName);
            }

            _openWindows[app.Id] = view;
            _nextSpawnPosition += _spawnOffset;
            return view;
        }

        public bool TryGetOpenWindow(AppId id, out WindowView view)
        {
            return _openWindows.TryGetValue(id, out view) && IsWindowOpen(view);
        }

        public void RestoreSession(AppRegistry registry, OsSessionData sessionData)
        {
            if (registry == null || sessionData == null)
            {
                return;
            }

            _sessionData = sessionData;
            var entries = sessionData.OpenWindows;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.AppId))
                {
                    continue;
                }

                if (!Enum.TryParse(entry.AppId, out AppId appId))
                {
                    continue;
                }

                if (!registry.TryGetById(appId, out var app))
                {
                    continue;
                }

                LaunchOrFocus(app, new Vector2(entry.X, entry.Y));
            }
        }

        public void CaptureSession(OsSessionData sessionData)
        {
            if (sessionData == null)
            {
                return;
            }

            if (sessionData.OpenWindows == null)
            {
                sessionData.OpenWindows = new System.Collections.Generic.List<OpenWindowData>();
            }

            sessionData.OpenWindows.Clear();
            var windows = _windowManager.Windows;
            for (var i = 0; i < windows.Count; i++)
            {
                var view = windows[i];
                if (!TryGetAppId(view, out var appId))
                {
                    continue;
                }

                sessionData.OpenWindows.Add(new OpenWindowData
                {
                    AppId = appId.ToString(),
                    X = view.Position.x,
                    Y = view.Position.y,
                    ZOrder = i
                });
            }
        }

        private bool IsWindowOpen(WindowView view)
        {
            var windows = _windowManager.Windows;
            for (var i = 0; i < windows.Count; i++)
            {
                if (windows[i] == view)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetPlaceholderContent(WindowView view, string appName)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            view.ContentRoot.Clear();
            view.ContentRoot.Add(new Label($"{appName} (placeholder)"));
        }

        private bool TryBuildAppContent(AppDefinitionSO app, WindowView view, string displayName)
        {
            if (app.ViewTemplate == null)
            {
                return false;
            }

            if (app.Id == AppId.FileManager)
            {
                if (_vfs == null)
                {
                    return false;
                }

                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new FileManagerController(root, _vfs, _sessionData, _eventBus);
                var startPath = _sessionData != null && !string.IsNullOrWhiteSpace(_sessionData.FileManagerPath)
                    ? _sessionData.FileManagerPath
                    : FileManagerStartPath;
                controller.Initialize(startPath);
                return true;
            }

            if (app.Id == AppId.Terminal)
            {
                if (_vfs == null)
                {
                    return false;
                }

                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new TerminalController(root, _vfs, _sessionData, _eventBus);
                controller.Initialize();
                return true;
            }

            if (app.Id == AppId.Missions)
            {
                if (_missionService == null)
                {
                    return false;
                }

                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new MissionsController(root, _missionService, _eventBus);
                controller.Initialize();
                return true;
            }

            if (app.Id == AppId.Store)
            {
                if (_storeCatalog == null || _storeService == null || _installService == null || _eventBus == null)
                {
                    return false;
                }

                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new StoreController(root, _storeCatalog, _storeService, _installService, _eventBus);
                controller.Initialize();
                return true;
            }

            if (app.Id == AppId.Notes)
            {
                if (_vfs == null)
                {
                    return false;
                }

                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new NotesController(root, _vfs);
                controller.Initialize();
                return true;
            }

            return false;
        }

        private bool TryGetAppId(WindowView view, out AppId appId)
        {
            foreach (var pair in _openWindows)
            {
                if (pair.Value == view)
                {
                    appId = pair.Key;
                    return true;
                }
            }

            appId = default;
            return false;
        }
    }
}
