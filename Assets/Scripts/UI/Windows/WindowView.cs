using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.UI.Windows
{
    public sealed class WindowView
    {
        private const string TitleBarName = "title-bar";
        private const string TitleLabelName = "title-label";
        private const string CloseButtonName = "close-button";
        private const string ContentName = "content";
        private const string ResizeHandleName = "resize-handle";
        private const float MinWidth = 320f;
        private const float MinHeight = 200f;

        private bool _isDragging;
        private int _dragPointerId = -1;
        private Vector2 _dragStartPointer;
        private Vector2 _dragStartPosition;
        private bool _isResizing;
        private int _resizePointerId = -1;
        private Vector2 _resizeStartPointer;
        private Vector2 _resizeStartSize;
        private Vector2 _position;

        public WindowView(VisualElement root, VisualElement titleBar, Label titleLabel, Button closeButton, VisualElement contentRoot, VisualElement resizeHandle)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            TitleBar = titleBar ?? throw new ArgumentNullException(nameof(titleBar));
            TitleLabel = titleLabel ?? throw new ArgumentNullException(nameof(titleLabel));
            CloseButton = closeButton ?? throw new ArgumentNullException(nameof(closeButton));
            ContentRoot = contentRoot ?? throw new ArgumentNullException(nameof(contentRoot));
            ResizeHandle = resizeHandle ?? throw new ArgumentNullException(nameof(resizeHandle));
        }

        public VisualElement Root { get; }
        public Label TitleLabel { get; }
        public Button CloseButton { get; }
        public VisualElement TitleBar { get; }
        public VisualElement ContentRoot { get; }
        public VisualElement ResizeHandle { get; }
        public Vector2 Position => _position;
        public bool IsResizing => _isResizing;

        public static WindowView Create(VisualTreeAsset template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var root = template.CloneTree();
            var titleBar = root.Q<VisualElement>(TitleBarName);
            var titleLabel = root.Q<Label>(TitleLabelName);
            var closeButton = root.Q<Button>(CloseButtonName);
            var content = root.Q<VisualElement>(ContentName);
            var resizeHandle = root.Q<VisualElement>(ResizeHandleName);

            if (titleBar == null || titleLabel == null || closeButton == null || content == null || resizeHandle == null)
            {
                throw new InvalidOperationException("[WindowView] Window template is missing required elements.");
            }

            resizeHandle.pickingMode = PickingMode.Position;
            return new WindowView(root, titleBar, titleLabel, closeButton, content, resizeHandle);
        }

        public void SetTitle(string title)
        {
            TitleLabel.text = title ?? string.Empty;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
            Root.style.left = position.x;
            Root.style.top = position.y;
        }

        public void BeginDrag(Vector2 pointerPosition, int pointerId)
        {
            if (_isResizing)
            {
                return;
            }

            if (_isDragging)
            {
                return;
            }

            _isDragging = true;
            _dragPointerId = pointerId;
            _dragStartPointer = pointerPosition;
            _dragStartPosition = _position;
        }

        public void UpdateDrag(Vector2 pointerPosition, int pointerId)
        {
            if (_isResizing)
            {
                return;
            }

            if (!_isDragging || pointerId != _dragPointerId)
            {
                return;
            }

            var delta = pointerPosition - _dragStartPointer;
            SetPosition(_dragStartPosition + delta);
        }

        public void EndDrag(int pointerId)
        {
            if (!_isDragging || pointerId != _dragPointerId)
            {
                return;
            }

            _isDragging = false;
            _dragPointerId = -1;
        }

        public void CancelDrag()
        {
            _isDragging = false;
            _dragPointerId = -1;
        }

        public void BeginResize(Vector2 pointerPosition, int pointerId)
        {
            if (_isResizing)
            {
                return;
            }

            _isResizing = true;
            _resizePointerId = pointerId;
            _resizeStartPointer = pointerPosition;
            _resizeStartSize = new Vector2(Root.resolvedStyle.width, Root.resolvedStyle.height);
        }

        public void UpdateResize(Vector2 pointerPosition, int pointerId)
        {
            if (!_isResizing || pointerId != _resizePointerId)
            {
                return;
            }

            var delta = pointerPosition - _resizeStartPointer;
            var width = Mathf.Max(MinWidth, _resizeStartSize.x + delta.x);
            var height = Mathf.Max(MinHeight, _resizeStartSize.y + delta.y);
            Root.style.width = width;
            Root.style.height = height;
        }

        public void EndResize(int pointerId)
        {
            if (!_isResizing || pointerId != _resizePointerId)
            {
                return;
            }

            _isResizing = false;
            _resizePointerId = -1;
        }

        public void CancelResize()
        {
            _isResizing = false;
            _resizePointerId = -1;
        }
    }
}
