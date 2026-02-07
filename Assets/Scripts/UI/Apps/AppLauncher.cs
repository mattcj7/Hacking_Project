using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Vfs;
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
        private VirtualFileSystem _vfs;

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

        public WindowView LaunchOrFocus(AppDefinitionSO app)
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
                    return existing;
                }

                _openWindows.Remove(app.Id);
            }

            var displayName = string.IsNullOrWhiteSpace(app.DisplayName) ? app.name : app.DisplayName;
            var position = app.DefaultWindowPosition == Vector2.zero ? _nextSpawnPosition : app.DefaultWindowPosition;
            var view = _windowManager.CreateWindow(displayName, position);
            if (app.DefaultWindowSize != Vector2.zero)
            {
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
            if (_vfs == null || app.ViewTemplate == null)
            {
                return false;
            }

            if (app.Id == AppId.FileManager)
            {
                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new FileManagerController(root, _vfs);
                controller.Initialize(FileManagerStartPath);
                return true;
            }

            if (app.Id == AppId.Terminal)
            {
                var root = app.ViewTemplate.CloneTree();
                view.ContentRoot.Clear();
                view.ContentRoot.Add(root);
                var controller = new TerminalController(root, _vfs);
                controller.Initialize();
                return true;
            }

            return false;
        }
    }
}
