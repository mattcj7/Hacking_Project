using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.UI.Windows
{
    public sealed class WindowManager
    {
        private readonly List<WindowView> _windows = new List<WindowView>();
        private readonly VisualElement _windowsLayer;
        private readonly VisualTreeAsset _windowTemplate;

        public WindowManager(VisualElement windowsLayer, VisualTreeAsset windowTemplate = null)
        {
            _windowsLayer = windowsLayer ?? throw new ArgumentNullException(nameof(windowsLayer));
            _windowTemplate = windowTemplate;
        }

        public IReadOnlyList<WindowView> Windows => _windows;

        public WindowView CreateWindow(string title, Vector2 position)
        {
            if (_windowTemplate == null)
            {
                throw new InvalidOperationException("[WindowManager] Window template is not assigned.");
            }

            var view = WindowView.Create(_windowTemplate);
            view.SetTitle(title);
            view.SetPosition(position);
            AddWindow(view);
            return view;
        }

        public WindowView CreateWindowAt(string title, Vector2 position)
        {
            return CreateWindow(title, position);
        }

        public void AddWindow(WindowView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (_windows.Contains(view))
            {
                return;
            }

            _windows.Add(view);
            _windowsLayer.Add(view.Root);
            WireWindow(view);
        }

        public void BringToFront(WindowView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (!_windows.Remove(view))
            {
                return;
            }

            _windows.Add(view);
            view.Root.BringToFront();
        }

        public void CloseWindow(WindowView view)
        {
            if (view == null)
            {
                return;
            }

            _windows.Remove(view);
            view.Root.RemoveFromHierarchy();
        }

        private void WireWindow(WindowView view)
        {
            view.Root.RegisterCallback<PointerDownEvent>(_ => BringToFront(view));
            view.TitleBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                BringToFront(view);
                view.BeginDrag(evt.position, evt.pointerId);
                view.TitleBar.CapturePointer(evt.pointerId);
            });
            view.TitleBar.RegisterCallback<PointerMoveEvent>(evt => view.UpdateDrag(evt.position, evt.pointerId));
            view.TitleBar.RegisterCallback<PointerUpEvent>(evt =>
            {
                view.EndDrag(evt.pointerId);
                if (view.TitleBar.HasPointerCapture(evt.pointerId))
                {
                    view.TitleBar.ReleasePointer(evt.pointerId);
                }
            });
            view.TitleBar.RegisterCallback<PointerCaptureOutEvent>(_ => view.CancelDrag());
            view.CloseButton.clicked += () => CloseWindow(view);

            view.ResizeHandle.RegisterCallback<PointerDownEvent>(evt =>
            {
                BringToFront(view);
                view.BeginResize(evt.position, evt.pointerId);
                view.ResizeHandle.CapturePointer(evt.pointerId);
            });
            view.ResizeHandle.RegisterCallback<PointerMoveEvent>(evt => view.UpdateResize(evt.position, evt.pointerId));
            view.ResizeHandle.RegisterCallback<PointerUpEvent>(evt =>
            {
                view.EndResize(evt.pointerId);
                if (view.ResizeHandle.HasPointerCapture(evt.pointerId))
                {
                    view.ResizeHandle.ReleasePointer(evt.pointerId);
                }
            });
            view.ResizeHandle.RegisterCallback<PointerCaptureOutEvent>(_ => view.CancelResize());
        }
    }
}
