using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Systems.Store;
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
        private const string InstallerDialogName = "installer-dialog";
        private const string InstallerDialogLabelName = "installer-dialog-label";
        private const string InstallerConfirmName = "installer-confirm";
        private const string InstallerCancelName = "installer-cancel";
        private const string HiddenClassName = "hidden";
        private const string InstallerExtension = ".installer";

        private readonly VirtualFileSystem _vfs;
        private readonly EventBus _eventBus;
        private readonly Label _pathLabel;
        private readonly Button _backButton;
        private readonly VisualElement _entriesRoot;
        private readonly VisualElement _dialogRoot;
        private readonly Label _dialogLabel;
        private readonly Button _dialogConfirm;
        private readonly Button _dialogCancel;
        private readonly OsSessionData _sessionData;
        private readonly InstallService _installService;
        private VfsDirectory _currentDirectory;
        private InstallerPackage _pendingInstaller;

        public FileManagerController(VisualElement root, VirtualFileSystem vfs, OsSessionData sessionData, EventBus eventBus, InstallService installService)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            _sessionData = sessionData;
            _eventBus = eventBus;
            _installService = installService;
            _pathLabel = root.Q<Label>(PathLabelName);
            _backButton = root.Q<Button>(BackButtonName);
            _entriesRoot = root.Q<VisualElement>(EntriesName);
            _dialogRoot = root.Q<VisualElement>(InstallerDialogName);
            _dialogLabel = root.Q<Label>(InstallerDialogLabelName);
            _dialogConfirm = root.Q<Button>(InstallerConfirmName);
            _dialogCancel = root.Q<Button>(InstallerCancelName);
        }

        public void Initialize(string startPath)
        {
            var startNode = _vfs.Resolve(startPath) as VfsDirectory ?? _vfs.Root;
            _currentDirectory = startNode;

            if (_backButton != null)
            {
                _backButton.clicked += NavigateUp;
            }

            if (_dialogRoot != null)
            {
                _dialogRoot.AddToClassList(HiddenClassName);
            }

            if (_dialogConfirm != null)
            {
                _dialogConfirm.clicked += ConfirmInstall;
            }

            if (_dialogCancel != null)
            {
                _dialogCancel.clicked += CancelInstall;
            }

            RefreshView();
        }

        private void RefreshView()
        {
            if (_pathLabel != null)
            {
                _pathLabel.text = _currentDirectory.Path;
            }

            if (_sessionData != null)
            {
                _sessionData.FileManagerPath = _currentDirectory.Path;
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
                return;
            }

            if (node is VfsFile file)
            {
                if (IsInstallerFile(file.Name) && InstallerPackage.TryParse(file.Content, out var package))
                {
                    ShowInstallPrompt(package);
                }

                _eventBus?.Publish(new FileManagerOpenedFileEvent(file.Path));
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

        private bool IsInstallerFile(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName)
                   && fileName.EndsWith(InstallerExtension, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowInstallPrompt(InstallerPackage package)
        {
            if (_installService == null)
            {
                return;
            }

            if (_dialogRoot == null)
            {
                return;
            }

            _pendingInstaller = package;
            var displayName = string.IsNullOrWhiteSpace(package.displayName) ? package.appId : package.displayName;
            if (_dialogLabel != null)
            {
                _dialogLabel.text = $"Install {displayName}?";
            }

            _dialogRoot.RemoveFromClassList(HiddenClassName);
        }

        private void ConfirmInstall()
        {
            if (_pendingInstaller == null)
            {
                HideInstallPrompt();
                return;
            }

            if (_installService != null)
            {
                _installService.Install(_pendingInstaller.appId);
            }

            HideInstallPrompt();
        }

        private void CancelInstall()
        {
            HideInstallPrompt();
        }

        private void HideInstallPrompt()
        {
            _pendingInstaller = null;
            if (_dialogRoot != null)
            {
                _dialogRoot.AddToClassList(HiddenClassName);
            }
        }
    }
}
