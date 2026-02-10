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
        private const string FrameName = "window-frame";
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
        private Vector2 _lastResizeSize;
        private Vector2 _position;

        public WindowView(VisualElement root, VisualElement frame, VisualElement titleBar, Label titleLabel, Button closeButton, VisualElement contentRoot, VisualElement resizeHandle)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            TitleBar = titleBar ?? throw new ArgumentNullException(nameof(titleBar));
            TitleLabel = titleLabel ?? throw new ArgumentNullException(nameof(titleLabel));
            CloseButton = closeButton ?? throw new ArgumentNullException(nameof(closeButton));
            ContentRoot = contentRoot ?? throw new ArgumentNullException(nameof(contentRoot));
            ResizeHandle = resizeHandle ?? throw new ArgumentNullException(nameof(resizeHandle));
        }

        public VisualElement Root { get; }
        public VisualElement Frame { get; }
        public Label TitleLabel { get; }
        public Button CloseButton { get; }
        public VisualElement TitleBar { get; }
        public VisualElement ContentRoot { get; }
        public VisualElement ResizeHandle { get; }
        public Vector2 Position => _position;
        public bool IsResizing => _isResizing;
        public Vector2 LastResizeSize => _lastResizeSize;

        public static WindowView Create(VisualTreeAsset template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var root = template.CloneTree();
            root.style.position = UnityEngine.UIElements.Position.Absolute;
            root.style.flexGrow = 0f;
            root.style.flexShrink = 0f;
            root.style.right = new StyleLength(StyleKeyword.Auto);
            root.style.bottom = new StyleLength(StyleKeyword.Auto);
            var frame = root.Q<VisualElement>(FrameName);
            var titleBar = root.Q<VisualElement>(TitleBarName);
            var titleLabel = root.Q<Label>(TitleLabelName);
            var closeButton = root.Q<Button>(CloseButtonName);
            var content = root.Q<VisualElement>(ContentName);
            var resizeHandle = root.Q<VisualElement>(ResizeHandleName);

            if (frame == null || titleBar == null || titleLabel == null || closeButton == null || content == null || resizeHandle == null)
            {
                throw new InvalidOperationException("[WindowView] Window template is missing required elements.");
            }

            resizeHandle.pickingMode = PickingMode.Position;
            resizeHandle.BringToFront();
            return new WindowView(root, frame, titleBar, titleLabel, closeButton, content, resizeHandle);
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
            Root.style.right = new StyleLength(StyleKeyword.Auto);
            Root.style.bottom = new StyleLength(StyleKeyword.Auto);
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
            var startWidth = Frame.resolvedStyle.width;
            var startHeight = Frame.resolvedStyle.height;
            if (startWidth <= 0f || startHeight <= 0f)
            {
                startWidth = Frame.layout.width;
                startHeight = Frame.layout.height;
            }

            _resizeStartSize = new Vector2(startWidth, startHeight);
            _lastResizeSize = _resizeStartSize;
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
            Frame.style.width = width;
            Frame.style.height = height;
            Root.style.width = width;
            Root.style.height = height;
            _lastResizeSize = new Vector2(width, height);
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
