using System.IO;
using HackingProject.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace HackingProject.Editor.Setup
{
    public static class UIToolkitBootstrapSetup
    {
        private const string MenuPath = "Tools/Hacking Project/Setup Desktop UI";
        private const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string PanelSettingsPath = "Assets/UI/Settings/MainPanelSettings.asset";
        private const string UxmlPath = "Assets/UI/Desktop/DesktopShell.uxml";
        private const string DesktopObjectName = "Desktop UI";

        [MenuItem(MenuPath)]
        public static void SetupDesktopUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                Debug.LogError($"[UIToolkitBootstrapSetup] Missing VisualTreeAsset at {UxmlPath}.");
                return;
            }

            var panelSettings = EnsurePanelSettings();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var uiDocument = FindDesktopDocument(scene, visualTree);
            if (uiDocument == null)
            {
                var desktopObject = new GameObject(DesktopObjectName, typeof(UIDocument), typeof(DesktopShellController));
                uiDocument = desktopObject.GetComponent<UIDocument>();
            }
            else
            {
                EnsureComponent<DesktopShellController>(uiDocument.gameObject);
            }

            uiDocument.visualTreeAsset = visualTree;
            uiDocument.panelSettings = panelSettings;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UIToolkitBootstrapSetup] Desktop UI setup complete.");
        }

        private static PanelSettings EnsurePanelSettings()
        {
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panelSettings != null)
            {
                return panelSettings;
            }

            EnsureFolder("Assets/UI");
            EnsureFolder("Assets/UI/Settings");

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
            AssetDatabase.SaveAssets();
            return panelSettings;
        }

        private static UIDocument FindDesktopDocument(Scene scene, VisualTreeAsset visualTree)
        {
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var documents = root.GetComponentsInChildren<UIDocument>(true);
                foreach (var document in documents)
                {
                    if (document.visualTreeAsset == visualTree)
                    {
                        return document;
                    }
                }
            }

            foreach (var root in roots)
            {
                if (root.name != DesktopObjectName)
                {
                    continue;
                }

                var document = root.GetComponent<UIDocument>();
                if (document != null)
                {
                    return document;
                }
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.GetComponent<T>() == null)
            {
                target.AddComponent<T>();
            }
        }
    }
}
