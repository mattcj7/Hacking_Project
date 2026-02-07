using HackingProject.UI.Apps;
using HackingProject.UI.Windows;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace HackingProject.Tests.EditMode
{
    public sealed class AppLauncherTests
    {
        private const string TemplatePath = "Assets/UI/Windows/Window.uxml";

        [Test]
        public void LaunchOrFocus_KeepsSingleInstanceAndFocuses()
        {
            var manager = new WindowManager(new VisualElement(), LoadTemplate());
            var launcher = new AppLauncher(manager);
            var terminal = new AppDefinition(AppId.Terminal, "Terminal");
            var fileManager = new AppDefinition(AppId.FileManager, "File Manager");

            var first = launcher.LaunchOrFocus(terminal);
            var second = launcher.LaunchOrFocus(fileManager);
            var reopened = launcher.LaunchOrFocus(terminal);

            Assert.AreEqual(2, manager.Windows.Count);
            Assert.AreSame(first, reopened);
            Assert.AreSame(reopened, manager.Windows[manager.Windows.Count - 1]);
            Assert.AreSame(second, manager.Windows[0]);
        }

        private static VisualTreeAsset LoadTemplate()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);
            Assert.IsNotNull(template, $"Missing VisualTreeAsset at {TemplatePath}.");
            return template;
        }
    }
}
