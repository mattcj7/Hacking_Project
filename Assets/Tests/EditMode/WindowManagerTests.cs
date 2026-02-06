using System.Collections.Generic;
using HackingProject.UI.Windows;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.Tests.EditMode
{
    public sealed class WindowManagerTests
    {
        private const string TemplatePath = "Assets/UI/Windows/Window.uxml";

        [Test]
        public void BringToFront_UpdatesOrder()
        {
            var manager = new WindowManager(new VisualElement(), LoadTemplate());
            var first = manager.CreateWindow("Terminal", Vector2.zero);
            var second = manager.CreateWindow("File Manager", new Vector2(16f, 16f));

            manager.BringToFront(first);

            Assert.AreEqual(2, manager.Windows.Count);
            Assert.AreSame(first, manager.Windows[manager.Windows.Count - 1]);
            Assert.AreSame(second, manager.Windows[0]);
        }

        [Test]
        public void Close_RemovesFromList()
        {
            var manager = new WindowManager(new VisualElement(), LoadTemplate());
            var window = manager.CreateWindow("Terminal", Vector2.zero);

            manager.CloseWindow(window);

            Assert.AreEqual(0, manager.Windows.Count);
            Assert.IsFalse(ContainsWindow(manager.Windows, window));
        }

        private static VisualTreeAsset LoadTemplate()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);
            Assert.IsNotNull(template, $"Missing VisualTreeAsset at {TemplatePath}.");
            return template;
        }

        private static bool ContainsWindow(IReadOnlyList<WindowView> windows, WindowView target)
        {
            for (var i = 0; i < windows.Count; i++)
            {
                if (windows[i] == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
