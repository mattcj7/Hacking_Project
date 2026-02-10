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
            _windowsLayer.schedule.Execute(() => ClampWindowToBounds(view)).ExecuteLater(0);
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
            ClampWindowToBounds(view);
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
                if (view.IsResizing)
                {
                    return;
                }

                BringToFront(view);
                view.BeginDrag(evt.position, evt.pointerId);
                view.TitleBar.CapturePointer(evt.pointerId);
            });
            view.TitleBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (view.IsResizing)
                {
                    return;
                }

                view.UpdateDrag(evt.position, evt.pointerId);
            });
            view.TitleBar.RegisterCallback<PointerUpEvent>(evt =>
            {
                view.EndDrag(evt.pointerId);
                if (view.TitleBar.HasPointerCapture(evt.pointerId))
                {
                    view.TitleBar.ReleasePointer(evt.pointerId);
                }

                ClampWindowToBounds(view);
            });
            view.TitleBar.RegisterCallback<PointerCaptureOutEvent>(_ => view.CancelDrag());
            view.CloseButton.clicked += () => CloseWindow(view);

            view.ResizeHandle.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                evt.StopPropagation();

                BringToFront(view);
                view.BeginResize(evt.position, evt.pointerId);
                view.ResizeHandle.CapturePointer(evt.pointerId);
            });
            view.ResizeHandle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                evt.StopPropagation();
                view.UpdateResize(evt.position, evt.pointerId);
            });
            view.ResizeHandle.RegisterCallback<PointerUpEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                evt.StopPropagation();

                view.EndResize(evt.pointerId);
                if (view.ResizeHandle.HasPointerCapture(evt.pointerId))
                {
                    view.ResizeHandle.ReleasePointer(evt.pointerId);
                }

                ClampWindowToBounds(view);
            });
            view.ResizeHandle.RegisterCallback<PointerCaptureOutEvent>(_ => view.CancelResize());
        }

        private void ClampWindowToBounds(WindowView view)
        {
            if (view == null)
            {
                return;
            }

            var boundsWidth = _windowsLayer.resolvedStyle.width;
            var boundsHeight = _windowsLayer.resolvedStyle.height;
            if (boundsWidth <= 0f || boundsHeight <= 0f)
            {
                return;
            }

            var windowWidth = view.Root.resolvedStyle.width;
            var windowHeight = view.Root.resolvedStyle.height;
            if (windowWidth <= 0f || windowHeight <= 0f)
            {
                return;
            }

            var titleBarHeight = view.TitleBar.resolvedStyle.height;
            if (titleBarHeight <= 0f)
            {
                titleBarHeight = 32f;
            }

            const float MinVisibleX = 80f;
            var minLeft = -windowWidth + MinVisibleX;
            var maxLeft = boundsWidth - MinVisibleX;
            if (maxLeft < minLeft)
            {
                var midpoint = (minLeft + maxLeft) * 0.5f;
                minLeft = midpoint;
                maxLeft = midpoint;
            }

            var minTop = -windowHeight + titleBarHeight;
            var maxTop = boundsHeight - titleBarHeight;
            if (maxTop < minTop)
            {
                var midpoint = (minTop + maxTop) * 0.5f;
                minTop = midpoint;
                maxTop = midpoint;
            }

            var clampedLeft = Mathf.Clamp(view.Position.x, minLeft, maxLeft);
            var clampedTop = Mathf.Clamp(view.Position.y, minTop, maxTop);

            if (Mathf.Abs(clampedLeft - view.Position.x) > 0.1f || Mathf.Abs(clampedTop - view.Position.y) > 0.1f)
            {
                view.SetPosition(new Vector2(clampedLeft, clampedTop));
            }
        }
    }
}
