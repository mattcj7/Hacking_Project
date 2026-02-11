using System;
using HackingProject.Infrastructure.Vfs;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    public sealed class NotesController
    {
        private const string PathLabelName = "notes-path";
        private const string ContentLabelName = "notes-content";
        private const string NotesPath = "/home/user/docs/notes.txt";

        private readonly VirtualFileSystem _vfs;
        private readonly Label _pathLabel;
        private readonly Label _contentLabel;

        public NotesController(VisualElement root, VirtualFileSystem vfs)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            _pathLabel = root.Q<Label>(PathLabelName);
            _contentLabel = root.Q<Label>(ContentLabelName);
        }

        public void Initialize()
        {
            if (_pathLabel != null)
            {
                _pathLabel.text = NotesPath;
            }

            if (_contentLabel == null)
            {
                return;
            }

            var file = _vfs.Resolve(NotesPath) as VfsFile;
            _contentLabel.text = file != null ? file.Content : "Notes file not found.";
        }
    }
}
