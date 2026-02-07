using System;
using System.IO;
using HackingProject.UI.Apps;
using HackingProject.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace HackingProject.Editor.Setup
{
    public static class UIToolkitBootstrapSetup
    {
        private const string MenuPath = "Tools/Hacking Project/Setup Desktop UI";
        private const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string PanelSettingsPath = "Assets/UI/Settings/MainPanelSettings.asset";
        private const string UxmlPath = "Assets/UI/Desktop/DesktopShell.uxml";
        private const string WindowUxmlPath = "Assets/UI/Windows/Window.uxml";
        private const string AppCatalogPath = "Assets/ScriptableObjects/Apps/AppCatalog_Default.asset";
        private const string DesktopObjectName = "Desktop UI";
        private const int Display1Index = 0;
        private static bool _setupQueued;
        private static UnityEngine.Object[] _previousSelection = Array.Empty<UnityEngine.Object>();
        private static UnityEngine.Object _previousActive;
        private static readonly string[] WindowSearchFilter = { "Window t:VisualTreeAsset" };
        private const string WindowTemplatePropertyName = "windowTemplate";
        private const string AppCatalogPropertyName = "appCatalog";

        [MenuItem(MenuPath)]
        public static void SetupDesktopUI()
        {
            if (_setupQueued)
            {
                return;
            }

            _previousSelection = Selection.objects;
            _previousActive = Selection.activeObject;
            Selection.activeObject = null;
            Selection.objects = Array.Empty<UnityEngine.Object>();

            _setupQueued = true;
            EditorApplication.delayCall += RunSetup;
        }

        private static void RunSetup()
        {
            EditorApplication.delayCall -= RunSetup;
            _setupQueued = false;

            try
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
                if (visualTree == null)
                {
                    Debug.LogError($"[UIToolkitBootstrapSetup] Missing VisualTreeAsset at {UxmlPath}.");
                    return;
                }

                var windowTemplatePath = WindowUxmlPath;
                var windowTemplate = LoadWindowTemplate(ref windowTemplatePath);
                if (windowTemplate == null)
                {
                    Debug.LogError($"[UIToolkitBootstrapSetup] Missing Window VisualTreeAsset at {WindowUxmlPath}.");
                    return;
                }

                var appCatalog = AssetDatabase.LoadAssetAtPath<AppCatalogSO>(AppCatalogPath);
                if (appCatalog == null)
                {
                    Debug.LogWarning($"[UIToolkitBootstrapSetup] Missing AppCatalog at {AppCatalogPath}.");
                }

                var panelSettings = EnsurePanelSettings();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                EnsureCameraSetup();

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

                var controller = uiDocument.GetComponent<DesktopShellController>();
                if (controller != null)
                {
                    var serializedController = new SerializedObject(controller);
                    serializedController.Update();
                    var templateProperty = serializedController.FindProperty(WindowTemplatePropertyName);
                    var catalogProperty = serializedController.FindProperty(AppCatalogPropertyName);
                    if (templateProperty != null)
                    {
                        if (templateProperty.objectReferenceValue != windowTemplate)
                        {
                            Undo.RecordObject(controller, "Assign Window Template");
                            templateProperty.objectReferenceValue = windowTemplate;
                            serializedController.ApplyModifiedProperties();
                            EditorUtility.SetDirty(controller);
                            EditorSceneManager.MarkSceneDirty(scene);
                        }

                        Debug.Log($"[UIToolkitBootstrapSetup] Assigned window template: {windowTemplatePath}");
                    }

                    if (catalogProperty != null && catalogProperty.objectReferenceValue == null && appCatalog != null)
                    {
                        Undo.RecordObject(controller, "Assign App Catalog");
                        catalogProperty.objectReferenceValue = appCatalog;
                        serializedController.ApplyModifiedProperties();
                        EditorUtility.SetDirty(controller);
                        EditorSceneManager.MarkSceneDirty(scene);
                        Debug.Log($"[UIToolkitBootstrapSetup] Assigned app catalog: {AppCatalogPath}");
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[UIToolkitBootstrapSetup] Desktop UI setup complete.");
            }
            finally
            {
                RestoreSelection();
            }
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

        private static VisualTreeAsset LoadWindowTemplate(ref string windowTemplatePath)
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WindowUxmlPath);
            if (template != null)
            {
                return template;
            }

            var guids = AssetDatabase.FindAssets(WindowSearchFilter[0]);
            for (var i = 0; i < guids.Length; i++)
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.Equals(candidatePath, WindowUxmlPath, StringComparison.OrdinalIgnoreCase))
                {
                    windowTemplatePath = candidatePath;
                    return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(candidatePath);
                }
            }

            for (var i = 0; i < guids.Length; i++)
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (candidatePath.EndsWith("/Window.uxml", StringComparison.OrdinalIgnoreCase))
                {
                    windowTemplatePath = candidatePath;
                    return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(candidatePath);
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

        private static void EnsureCameraSetup()
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (cameras.Length == 0)
            {
                var cameraObject = new GameObject("Main Camera", typeof(Camera));
                var camera = cameraObject.GetComponent<Camera>();
                camera.enabled = true;
                camera.targetDisplay = Display1Index;
                cameraObject.tag = "MainCamera";
                return;
            }

            Camera enabledDisplayCamera = null;
            for (var i = 0; i < cameras.Length; i++)
            {
                var candidate = cameras[i];
                if (candidate.enabled && candidate.targetDisplay == Display1Index)
                {
                    enabledDisplayCamera = candidate;
                    break;
                }
            }

            if (enabledDisplayCamera == null)
            {
                var candidate = FirstEnabledCamera(cameras) ?? cameras[0];
                candidate.enabled = true;
                candidate.targetDisplay = Display1Index;
                enabledDisplayCamera = candidate;
            }

            var hasMainCameraTag = false;
            for (var i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].CompareTag("MainCamera"))
                {
                    hasMainCameraTag = true;
                    break;
                }
            }

            if (!hasMainCameraTag)
            {
                enabledDisplayCamera.gameObject.tag = "MainCamera";
            }
        }

        private static Camera FirstEnabledCamera(Camera[] cameras)
        {
            for (var i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].enabled)
                {
                    return cameras[i];
                }
            }

            return null;
        }

        private static void RestoreSelection()
        {
            Selection.objects = _previousSelection ?? Array.Empty<UnityEngine.Object>();
            if (_previousActive != null)
            {
                Selection.activeObject = _previousActive;
            }

            _previousSelection = Array.Empty<UnityEngine.Object>();
            _previousActive = null;
        }
    }
}
