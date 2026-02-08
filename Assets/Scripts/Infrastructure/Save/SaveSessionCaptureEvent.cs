using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Save
{
    public readonly struct SaveSessionCaptureEvent : IEvent
    {
        public SaveSessionCaptureEvent(OsSessionData session)
        {
            Session = session;
        }

        public OsSessionData Session { get; }
    }
}
