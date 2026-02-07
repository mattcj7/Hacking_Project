using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    [CreateAssetMenu(menuName = "Hacking Project/Apps/App Definition", fileName = "AppDefinition")]
    public sealed class AppDefinitionSO : ScriptableObject
    {
        public AppId Id;
        public string DisplayName;
        public Sprite Icon;
        public Vector2 DefaultWindowPosition;
        public Vector2 DefaultWindowSize;
        public VisualTreeAsset ViewTemplate;
    }
}
