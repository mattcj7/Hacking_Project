using HackingProject.Infrastructure.Save;
using HackingProject.UI.Apps;
using HackingProject.UI.Windows;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.Tests.EditMode
{
    public sealed class OsSessionTests
    {
        private const string TemplatePath = "Assets/UI/Windows/Window.uxml";

        [Test]
        public void CaptureSession_BuildsOpenWindows()
        {
            var manager = new WindowManager(new VisualElement(), LoadTemplate());
            var launcher = new AppLauncher(manager);
            var terminal = CreateDefinition(AppId.Terminal, "Terminal");
            var fileManager = CreateDefinition(AppId.FileManager, "File Manager");

            launcher.LaunchOrFocus(terminal, new Vector2(12f, 34f));
            launcher.LaunchOrFocus(fileManager, new Vector2(56f, 78f));

            var session = new OsSessionData();
            launcher.CaptureSession(session);

            Assert.AreEqual(2, session.OpenWindows.Count);
            Assert.IsTrue(HasEntry(session, "Terminal", 12f, 34f));
            Assert.IsTrue(HasEntry(session, "FileManager", 56f, 78f));
        }

        [Test]
        public void RestoreSession_OpensWindowsAtPositions()
        {
            var manager = new WindowManager(new VisualElement(), LoadTemplate());
            var launcher = new AppLauncher(manager);
            var terminal = CreateDefinition(AppId.Terminal, "Terminal");
            var fileManager = CreateDefinition(AppId.FileManager, "File Manager");
            var registry = new AppRegistry(new[] { terminal, fileManager });

            var session = new OsSessionData();
            session.OpenWindows.Add(new OpenWindowData { AppId = "Terminal", X = 10f, Y = 20f, ZOrder = 0 });
            session.OpenWindows.Add(new OpenWindowData { AppId = "FileManager", X = 30f, Y = 40f, ZOrder = 1 });

            launcher.RestoreSession(registry, session);

            Assert.AreEqual(2, manager.Windows.Count);
            Assert.IsTrue(launcher.TryGetOpenWindow(AppId.Terminal, out var terminalView));
            Assert.IsTrue(launcher.TryGetOpenWindow(AppId.FileManager, out var fileManagerView));
            Assert.AreEqual(new Vector2(10f, 20f), terminalView.Position);
            Assert.AreEqual(new Vector2(30f, 40f), fileManagerView.Position);
        }

        private static bool HasEntry(OsSessionData session, string appId, float x, float y)
        {
            for (var i = 0; i < session.OpenWindows.Count; i++)
            {
                var entry = session.OpenWindows[i];
                if (entry != null && entry.AppId == appId && Mathf.Approximately(entry.X, x) && Mathf.Approximately(entry.Y, y))
                {
                    return true;
                }
            }

            return false;
        }

        private static AppDefinitionSO CreateDefinition(AppId id, string displayName)
        {
            var definition = ScriptableObject.CreateInstance<AppDefinitionSO>();
            definition.Id = id;
            definition.DisplayName = displayName;
            return definition;
        }

        private static VisualTreeAsset LoadTemplate()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);
            Assert.IsNotNull(template, $"Missing VisualTreeAsset at {TemplatePath}.");
            return template;
        }
    }
}
