using System;

namespace HackingProject.Infrastructure.Save
{
    [Serializable]
    public sealed class OpenWindowData
    {
        public string AppId;
        public float X;
        public float Y;
        public int ZOrder;
    }
}
