using System;
using System.Collections.Generic;

namespace HackingProject.Infrastructure.Save
{
    [Serializable]
    public sealed class OsSessionData
    {
        public List<OpenWindowData> OpenWindows = new List<OpenWindowData>();
        public string TerminalCwdPath = "/home/user";
        public string FileManagerPath = "/home/user";
    }
}
