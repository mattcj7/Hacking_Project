using System;
using System.Collections.Generic;
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

        public AppLauncher(WindowManager windowManager)
        {
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        }

        public WindowView LaunchOrFocus(AppDefinition app)
        {
            if (_openWindows.TryGetValue(app.Id, out var existing))
            {
                if (IsWindowOpen(existing))
                {
                    _windowManager.BringToFront(existing);
                    return existing;
                }

                _openWindows.Remove(app.Id);
            }

            var view = _windowManager.CreateWindow(app.DisplayName, _nextSpawnPosition);
            SetPlaceholderContent(view, app.DisplayName);
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
    }
}
