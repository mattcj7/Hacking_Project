using System;
using HackingProject.Infrastructure.Vfs;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    public sealed class FileManagerController
    {
        private const string PathLabelName = "path-label";
        private const string BackButtonName = "back-button";
        private const string EntriesName = "entries";
        private const string EntryClassName = "filemanager-entry";
        private const string DirectoryClassName = "filemanager-entry-dir";

        private readonly VirtualFileSystem _vfs;
        private readonly Label _pathLabel;
        private readonly Button _backButton;
        private readonly VisualElement _entriesRoot;
        private VfsDirectory _currentDirectory;

        public FileManagerController(VisualElement root, VirtualFileSystem vfs)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            _pathLabel = root.Q<Label>(PathLabelName);
            _backButton = root.Q<Button>(BackButtonName);
            _entriesRoot = root.Q<VisualElement>(EntriesName);
        }

        public void Initialize(string startPath)
        {
            var startNode = _vfs.Resolve(startPath) as VfsDirectory ?? _vfs.Root;
            _currentDirectory = startNode;

            if (_backButton != null)
            {
                _backButton.clicked += NavigateUp;
            }

            RefreshView();
        }

        private void RefreshView()
        {
            if (_pathLabel != null)
            {
                _pathLabel.text = _currentDirectory.Path;
            }

            if (_entriesRoot != null)
            {
                _entriesRoot.Clear();
                var children = _currentDirectory.Children;
                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    var isDirectory = child is VfsDirectory;
                    var label = isDirectory ? $"{child.Name}/" : child.Name;
                    var button = new Button(() => OnEntryClicked(child))
                    {
                        text = label
                    };
                    button.AddToClassList(EntryClassName);
                    if (isDirectory)
                    {
                        button.AddToClassList(DirectoryClassName);
                    }

                    _entriesRoot.Add(button);
                }
            }

            if (_backButton != null)
            {
                _backButton.SetEnabled(_currentDirectory.Parent != null);
            }
        }

        private void OnEntryClicked(VfsNode node)
        {
            if (node is VfsDirectory directory)
            {
                _currentDirectory = directory;
                RefreshView();
            }
        }

        private void NavigateUp()
        {
            if (_currentDirectory.Parent == null)
            {
                return;
            }

            _currentDirectory = _currentDirectory.Parent;
            RefreshView();
        }
    }
}
