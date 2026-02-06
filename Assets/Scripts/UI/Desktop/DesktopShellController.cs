using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.UI.Desktop
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class DesktopShellController : MonoBehaviour
    {
        private const string ClockLabelName = "clock-label";

        private Label _clockLabel;
        private float _nextUpdateTime;

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            if (document == null)
            {
                Debug.LogWarning("[DesktopShellController] Missing UIDocument.");
                return;
            }

            _clockLabel = document.rootVisualElement?.Q<Label>(ClockLabelName);
            if (_clockLabel == null)
            {
                Debug.LogWarning($"[DesktopShellController] Clock label '{ClockLabelName}' not found.");
                return;
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

        private void UpdateClock()
        {
            _clockLabel.text = DateTime.Now.ToString("HH:mm:ss");
            _nextUpdateTime = Time.unscaledTime + 1f;
        }
    }
}
