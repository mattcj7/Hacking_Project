using System;

namespace HackingProject.Infrastructure.Save
{
    [Serializable]
    public sealed class SaveEnvelope
    {
        public int Version = 1;
        public string PayloadJson;
        public string PayloadSha256Hex;
    }
}
